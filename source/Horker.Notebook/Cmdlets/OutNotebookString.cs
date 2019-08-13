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
    [Cmdlet("Out", "NotebookString")]
    public class OutNotebookString : OutStringCommand
    {
        protected override void ProcessRecord()
        {
            if (InputObject.BaseObject is UIElement uiElement)
                RoundtripViewModel.Active.WriteUIElement(uiElement);
            else
                base.ProcessRecord();
        }
    }
}
