using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Horker.Notebook.ViewModels;

namespace Horker.Notebook.Models
{
    public class Session
    {
        private SessionViewModel _sessionViewModel;

        private ExecutionQueue _executionQueue;

        private int _exitCode;

        private Runspace _runspace;
        private PowerShell _powerShell;

        private ManualResetEvent _cancelEvent;
        private bool _cancelled;

        public Session(SessionViewModel sessionViewModel)
        {
            _sessionViewModel = sessionViewModel;
            sessionViewModel.Model = this;

            _executionQueue = new ExecutionQueue();

            // Setting up the PowerShell engine.

            var host = new Host(sessionViewModel, (int e) => { _exitCode = e; _executionQueue.Cancel(); });

            _runspace = RunspaceFactory.CreateRunspace(host);
            _runspace.ApartmentState = ApartmentState.STA;
            _runspace.ThreadOptions = PSThreadOptions.UseNewThread;
            _runspace.Open();

            _powerShell = PowerShell.Create();
            _powerShell.Runspace = _runspace;

            _cancelEvent = new ManualResetEvent(false);
        }

        public Roundtrip CreateNewRoundtrip(bool wait, int position = -1)
        {
            var r = new Roundtrip(_executionQueue);
            _sessionViewModel.AddRoundtripViewModel(r.ViewModel, position);

            if (wait)
                r.ViewModel.CreatedEvent.WaitOne();

            return r;
        }

        private static readonly string _errorMessageFormat =
            "{0}\r\n" +
            "{1}\r\n" +
            "    + CategoryInfo          : {2}\r\n" +
            "    + FullyQualifiedErrorId : {3}\r\n";

        private void DisplayError(RuntimeException ex)
        {
            var message = string.Format(
                _errorMessageFormat,
                ex.ErrorRecord,
                ex.ErrorRecord.InvocationInfo?.PositionMessage,
                ex.ErrorRecord.CategoryInfo,
                ex.ErrorRecord.FullyQualifiedErrorId);

            SessionViewModel.ActiveOutput.WriteWholeLine(message, Brushes.IndianRed);
        }

        private void InitializeCurrentSession()
        {
            var r = CreateNewRoundtrip(true);
            SessionViewModel.ActiveOutput = r.ViewModel;

            try
            {
                // Import this module itself to define cmdlets.

                _powerShell.AddCommand("Import-Module").AddParameter("Assembly", typeof(Startup).Assembly);
                _powerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                _powerShell.Invoke();

                // Load profile files.
                // TODO: handle errors

                PSCommand[] profileCommands = Microsoft.Samples.PowerShell.Host.HostUtilities.GetProfileCommands("Notebook");
                foreach (PSCommand command in profileCommands)
                {
                    _powerShell.Commands = command;
                    _powerShell.AddCommand("Out-NotebookInternal");
                    _powerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                    _powerShell.Invoke();
                }
            }
            catch (RuntimeException ex)
            {
                DisplayError(ex);
            }

            if (!r.ViewModel.IsOutputEmpty())
                CreateNewRoundtrip(true);

            _sessionViewModel.HideProgress();
        }

        private string GetPrompt()
        {
            _powerShell.Commands.Clear();
            _powerShell.Commands.AddCommand("Prompt");
            _powerShell.Commands.AddCommand("Out-String").AddParameter("Stream");
            try
            {
                var result = _powerShell.Invoke();
                return ((string)result[0].BaseObject).Replace('\r', ' ').Replace('\n', ' ');
            }
            catch (RuntimeException ex)
            {
                return ex.ToString();
            }
        }

        public int StartExecutionLoop()
        {
            InitializeCurrentSession();

            Roundtrip roundtrip = null;
            var stopWatch = new Stopwatch();

            try
            {
                while (true)
                {
                    _sessionViewModel.CommandPrompt = GetPrompt();

                    var input = new PSDataCollection<PSObject>();
                    input.Complete();

                    var output = new PSDataCollection<PSObject>();

                    var queueItem = _executionQueue.Dequeue();

                    if (queueItem is LoadSessionRequest)
                    {
                        LoadSession();
                        continue;
                    }

                    if (queueItem is CodeCompletionRequest codeCompletionRequest)
                    {
                        DoCodeCompletion(codeCompletionRequest);
                        continue;
                    }

                    var request = queueItem as ExecutionRequest;
                    roundtrip = request.Roundtrip;
                    SessionViewModel.ActiveOutput = roundtrip.ViewModel;

                    var commandLine = roundtrip.ViewModel.CommandLine;
                    _powerShell.Commands.Clear();
                    _powerShell.AddScript(commandLine);
                    _powerShell.AddCommand("Out-NotebookInternal");
                    _powerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                    roundtrip.ViewModel.Hidden();
                    roundtrip.ViewModel.Clear();
                    roundtrip.ViewModel.ShowExecuting();

                    try
                    {
                        ResetCancelState();
                        stopWatch.Restart();

                        var asyncResult = _powerShell.BeginInvoke(input, output);

                        WaitHandle.WaitAny(new WaitHandle[] { asyncResult.AsyncWaitHandle, _cancelEvent });

                        if (_cancelled)
                        {
                            _powerShell.Stop();
                            roundtrip.ViewModel.WriteWholeLine("^C");
                        }

                        _powerShell.EndInvoke(asyncResult);
                    }
                    catch (RuntimeException ex)
                    {
                        DisplayError(ex);
                    }

                    stopWatch.Stop();

                    _sessionViewModel.TimeTaken = stopWatch.Elapsed;
                    _sessionViewModel.HideProgress();
                    roundtrip.ViewModel.ShowEditing();

                    if (request.MoveToNext)
                    {
                        if (_sessionViewModel.IsLastItem(roundtrip.ViewModel))
                            CreateNewRoundtrip(true);
                        else
                        {
                            var rr = _sessionViewModel.GetNextRoundtripViewModel(roundtrip.ViewModel);
                            rr.Focus();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // pass
            }

            return _exitCode;
        }

        public void ResetCancelState()
        {
            _cancelEvent.Reset();
            _cancelled = false;
        }

        public void NotifyCancel()
        {
            _cancelEvent.Set();
            _cancelled = true;
        }

        // Reader and writer

        private static readonly string _fileHeader = "#!Notebook v1";
        private static readonly string _commandLineHeader = "#!CommandLine";

        public void SaveSession(TextWriter writer)
        {
            writer.WriteLine(_fileHeader);
            writer.WriteLine();

            foreach (var item in _sessionViewModel.Items)
            {
                writer.WriteLine(_commandLineHeader);
                if (!string.IsNullOrEmpty(item.CommandLine))
                    writer.WriteLine(item.CommandLine);
            }
        }

        public void SaveSession()
        {
            Debug.Assert(!string.IsNullOrEmpty(_sessionViewModel.FileName));

            var name = _sessionViewModel.FileName;

            using (var writer = new StreamWriter(name, false, Encoding.UTF8))
            {
                SaveSession(writer);
            }

            _sessionViewModel.AddRecentlyUsedFile(name);
        }

        public void LoadSession(TextReader reader)
        {
            var commandLines = new List<string>();
            var line = reader.ReadLine();
            if (line != _fileHeader)
            {
                commandLines.Add(reader.ReadToEnd());
            }
            else
            {
                line = reader.ReadLine();
                if (line != "")
                    throw new ApplicationException("Invalid file format");

                line = reader.ReadLine();
                if (line != _commandLineHeader)
                    throw new ApplicationException("Invalid file format");

                var builder = new StringBuilder();

                while (true)
                {
                    line = reader.ReadLine();
                    if (line == null)
                        break;

                    if (line == _commandLineHeader)
                    {
                        // Remove newline at end of code.
                        builder.Remove(builder.Length - 2, 2);
                        commandLines.Add(builder.ToString());
                        builder.Clear();
                    }
                    else
                    {
                        builder.AppendLine(line);
                    }
                }

                commandLines.Add(builder.ToString());
            }

            _sessionViewModel.Clear();

            foreach (var c in commandLines)
            {
                var r = CreateNewRoundtrip(true);
                r.ViewModel.WriteCommandLine(c);
            }

            _sessionViewModel.IsTextChanged = false;
        }

        public void LoadSession()
        {
            try
            {
                using (var reader = new StreamReader(_sessionViewModel.FileName, Encoding.UTF8))
                {
                    LoadSession(reader);
                }
                _sessionViewModel.AddRecentlyUsedFile(_sessionViewModel.FileName);
            }
            catch (Exception ex)
            {
                _sessionViewModel.ShowMessageBox(ex.Message, "Error occurred on loading");
            }
        }

        public void EnqueueLoadSessionRequest(string fileName)
        {
            _sessionViewModel.FileName = fileName;
            _executionQueue.Enqueue(new LoadSessionRequest());
        }

        // Code completion

        private void DoCodeCompletion(CodeCompletionRequest request)
        {
            var result = CommandCompletion.CompleteInput(request.Input, request.CaretOffset, null, _powerShell);
            request.CompletionResult = result;
        }
    }
}
