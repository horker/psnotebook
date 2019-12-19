using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Horker.Notebook.ViewModels;
using Microsoft.PowerShell.Commands;

namespace Horker.Notebook.Cmdlets
{
    [Cmdlet("Out", "NotebookInternal")]
    public class OutNotebookInernal : OutDefaultCommand
    {
        public static Models.Session Session { get; set; }

        protected override void ProcessRecord()
        {
            if (Session.IsCancelled)
                throw new TerminateException();

            var obj = InputObject?.BaseObject;
            if (obj != null && obj is UIElement uiElement)
                SessionViewModel.ActiveOutput.WriteUIElement(uiElement);
            else
                base.ProcessRecord();
        }
    }
}
