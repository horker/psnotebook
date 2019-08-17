using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Horker.Notebook.ViewModels;

namespace Horker.Notebook
{
    class Host : PSHost
    {
        private SessionViewModel _sessionViewModel;
        private HostUserInterface _hostUserInterface;
        private Action<int> _exitCallback;

        public Host(SessionViewModel sessionViewModel, Action<int> exitCallback)
        {
            _sessionViewModel = sessionViewModel;
            _hostUserInterface = new HostUserInterface(sessionViewModel);
            _exitCallback = exitCallback;
        }

        private CultureInfo _currentCulture = Thread.CurrentThread.CurrentCulture;

        public override CultureInfo CurrentCulture => _currentCulture;

        public override CultureInfo CurrentUICulture => _currentCulture;

        private Guid _instanceId = Guid.NewGuid();

        public override Guid InstanceId => _instanceId;

        public override string Name => "PowerShell Notebook";

        public override PSHostUserInterface UI => _hostUserInterface;

        private Version _version = new Version(0, 1, 0);

        public override Version Version => _version;

        public override PSObject PrivateData => new PSObject(_sessionViewModel);

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException();
        }

        public override void NotifyBeginApplication()
        {
            throw new NotImplementedException();
        }

        public override void NotifyEndApplication()
        {
            throw new NotImplementedException();
        }

        public override void SetShouldExit(int exitCode)
        {
            _exitCallback.Invoke(exitCode);
        }
    }
}
