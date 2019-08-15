using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Horker.Notebook.Cmdlets
{
    [Cmdlet("Invoke", "WpfAction")]
    public class InvokeWpfAction : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public ScriptBlock Action { get; set; }

        protected override void BeginProcessing()
        {
            System.Collections.ObjectModel.Collection<PSObject> results = null;
            Models.CurrentState.Dispatcher.Invoke(() => {
                results = InvokeCommand.InvokeScript(false, Action, null);
            });

            foreach (var r in results)
                WriteObject(r);
        }
    }
}