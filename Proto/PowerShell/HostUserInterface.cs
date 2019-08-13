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
using Horker.ViewModels;

namespace Horker
{
    class HostUserInterface : PSHostUserInterface
    {
        private SessionViewModel _session;

        public HostUserInterface(SessionViewModel session)
        {
            _session = session;
        }

        public override PSHostRawUserInterface RawUI => null;

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
            RoundtripViewModel.Active.Write(value);
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            RoundtripViewModel.Active.Write(value,
                ConsoleColorToBrushConverter.GetBrush(foregroundColor),
                ConsoleColorToBrushConverter.GetBrush(backgroundColor));
        }

        public override void WriteDebugLine(string message)
        {
            RoundtripViewModel.Active.WriteWholeLine(
                string.Format(CultureInfo.CurrentCulture, "DEBUG: {0}", message),
                Brushes.Goldenrod, null);
        }

        public override void WriteErrorLine(string message)
        {
            RoundtripViewModel.Active.WriteWholeLine(
                string.Format(CultureInfo.CurrentCulture, "ERROR: {0}", message),
                Brushes.IndianRed, null);
        }

        public override void WriteLine(string value)
        {
            RoundtripViewModel.Active.WriteLine(value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            throw new NotImplementedException();
        }

        public override void WriteVerboseLine(string message)
        {
            RoundtripViewModel.Active.WriteWholeLine(
                string.Format(CultureInfo.CurrentCulture, "VERBOSE: {0}", message),
                Brushes.Goldenrod, null);
        }

        public override void WriteWarningLine(string message)
        {
            RoundtripViewModel.Active.WriteWholeLine(
                string.Format(CultureInfo.CurrentCulture, "WARNING: {0}", message),
                Brushes.Goldenrod, null);
        }
    }
}
