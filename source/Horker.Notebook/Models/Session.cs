using System;
using System.Collections.Generic;
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

        private Roundtrip _activeRoundtrip;

        public Roundtrip ActiveRoundtrip => _activeRoundtrip;

        public Session(SessionViewModel sessionViewModel)
        {
            _sessionViewModel = sessionViewModel;

            _executionQueue = new ExecutionQueue();

            // Setting up the PowerShell engine.

            var host = new Host(sessionViewModel, (int e) => { _exitCode = e; _executionQueue.Cancel(); });

            _runspace = RunspaceFactory.CreateRunspace(host);
            _runspace.ApartmentState = ApartmentState.STA;
            _runspace.ThreadOptions = PSThreadOptions.UseNewThread;
            _runspace.Open();

            _powerShell = PowerShell.Create();
            _powerShell.Runspace = _runspace;

            // Import this module itself to define cmdlets.
            _powerShell.AddCommand("Import-Module").AddParameter("Assembly", typeof(Startup).Assembly);
            _powerShell.Invoke();

            // Load profile files.
            // TODO: handle errors

            PSCommand[] profileCommands = Microsoft.Samples.PowerShell.Host.HostUtilities.GetProfileCommands("Notebook");
            foreach (PSCommand command in profileCommands)
            {
                _powerShell.Commands = command;
                _powerShell.Invoke();
            }
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

        public int StartExecutionLoop()
        {
            Roundtrip roundtrip = null;

//            _powerShell.Streams.Error.DataAdded += (object sender, DataAddedEventArgs args) => {
//                var error = _powerShell.Streams.Error.Last();
//                var message = string.Format(
//                    _errorMessageFormat,
//                    error,
//                    error.InvocationInfo?.PositionMessage,
//                    error.CategoryInfo,
//                    error.FullyQualifiedErrorId);

//                roundtrip.ViewModel.WriteWholeLine(message, Brushes.IndianRed);
//            };

            CreateNewRoundtrip();

            try
            {
                while (true)
                {
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

                    roundtrip.ViewModel.Clear();

                    try
                    {
                        var asyncResult = _powerShell.BeginInvoke(input, output);

                        if (_sessionViewModel.IsLastItem(roundtrip.ViewModel))
                            _sessionViewModel.ScrollToBottom();

                        _powerShell.EndInvoke(asyncResult);
                    }
                    catch (RuntimeException ex)
                    {
                        var message = string.Format(
                            _errorMessageFormat,
                            ex.ErrorRecord,
                            ex.ErrorRecord.InvocationInfo?.PositionMessage,
                            ex.ErrorRecord.CategoryInfo,
                            ex.ErrorRecord.FullyQualifiedErrorId);

                        roundtrip.ViewModel.WriteWholeLine(message, Brushes.IndianRed);
                    }

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
    }
}
