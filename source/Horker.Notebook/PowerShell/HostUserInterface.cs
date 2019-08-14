using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Horker.Notebook.ViewModels;

namespace Horker.Notebook
{
    class HostUserInterface : PSHostUserInterface
    {
        private SessionViewModel _session;

        public HostUserInterface(SessionViewModel session)
        {
            _session = session;
            _rawUI = new HostRawUserInterface();
        }

        private HostRawUserInterface _rawUI;

        public override PSHostRawUserInterface RawUI => _rawUI;

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            throw new NotImplementedException();
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException();
        }

        public override void Write(string value)
        {
            SessionViewModel.ActiveOutput.Write(value);
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            SessionViewModel.ActiveOutput.Write(value,
                ConsoleColorToBrushConverter.GetBrush(foregroundColor),
                ConsoleColorToBrushConverter.GetBrush(backgroundColor));
        }

        public override void WriteDebugLine(string message)
        {
            SessionViewModel.ActiveOutput.WriteWholeLine(
                string.Format(CultureInfo.CurrentCulture, "DEBUG: {0}", message),
                Brushes.Goldenrod, null);
        }

        public override void WriteErrorLine(string message)
        {
            SessionViewModel.ActiveOutput.WriteWholeLine(
                message, Brushes.IndianRed, null);
        }

        public override void WriteLine(string value)
        {
            SessionViewModel.ActiveOutput.WriteLine(value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            var message = record.Activity + " " + record.StatusDescription + " " + record.CurrentOperation;
            if (record.SecondsRemaining > 0)
                message += "(remaining " + record.SecondsRemaining + " seconds)";

            var percent = record.PercentComplete >= 0 ? record.PercentComplete : 0;

            _session.WriteProgress(message, percent);
        }

        public override void WriteVerboseLine(string message)
        {
            SessionViewModel.ActiveOutput.WriteWholeLine(
                string.Format(CultureInfo.CurrentCulture, "VERBOSE: {0}", message),
                Brushes.Goldenrod, null);
        }

        public override void WriteWarningLine(string message)
        {
            SessionViewModel.ActiveOutput.WriteWholeLine(
                string.Format(CultureInfo.CurrentCulture, "WARNING: {0}", message),
                Brushes.Goldenrod, null);
        }
    }
}
