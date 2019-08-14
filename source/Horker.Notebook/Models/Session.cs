using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
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

        private Roundtrip _activeRoundtrip;

        public Roundtrip ActiveRoundtrip => _activeRoundtrip;

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

        public void CreateNewRoundtrip()
        {
            var r = new Roundtrip(_executionQueue);
            _sessionViewModel.AddRoundtripViewModel(r.ViewModel);
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
            SessionViewModel.ActiveOutput = _sessionViewModel.GetLastItem();

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
                    _powerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
                    _powerShell.Invoke();
                }
            }
            catch (RuntimeException ex)
            {
                DisplayError(ex);
            }
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
            Roundtrip roundtrip = null;

            CreateNewRoundtrip();

            InitializeCurrentSession();

            var stopWatch = new Stopwatch();

            try
            {
                while (true)
                {
                    _sessionViewModel.CommandPrompt = GetPrompt();

                    var input = new PSDataCollection<PSObject>();
                    input.Complete();

                    var output = new PSDataCollection<PSObject>();

                    roundtrip = _executionQueue.Dequeue();
                    _activeRoundtrip = roundtrip;

                    var commandLine = roundtrip.ViewModel.CommandLine;
                    _powerShell.Commands.Clear();
                    _powerShell.AddScript(commandLine);
                    _powerShell.AddCommand("Out-NotebookInternal");
                    _powerShell.Commands.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);

                    roundtrip.ViewModel.Hidden();
                    roundtrip.ViewModel.Clear();

                    try
                    {
                        ResetCancelState();
                        stopWatch.Restart();

                        var asyncResult = _powerShell.BeginInvoke(input, output);

                        if (_sessionViewModel.IsLastItem(roundtrip.ViewModel))
                            _sessionViewModel.ScrollToBottom();

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

                    if (_sessionViewModel.IsLastItem(roundtrip.ViewModel))
                        CreateNewRoundtrip();
                    else
                    {
                        var rr = _sessionViewModel.GetNextRoundtripViewModel(roundtrip.ViewModel);
                        rr.Focus();
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
    }
}
