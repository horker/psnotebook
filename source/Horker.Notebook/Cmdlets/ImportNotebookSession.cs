using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Horker.Notebook.Models;
using Horker.Notebook.ViewModels;

namespace Horker.Notebook.Cmdlets
{
    [Cmdlet("Import", "NotebookSession")]
    [OutputType(typeof(void))]
    public class ImportNotebookSession : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string Path { get; set; }

        protected override void BeginProcessing()
        {
            var fullPath = System.IO.Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, Path);

            var sessionViewModel = Host.PrivateData.BaseObject as SessionViewModel;
            if (sessionViewModel == null)
            {
                WriteError(new ErrorRecord(new ApplicationException("Current host is not PowerShell Notebook"), null, ErrorCategory.InvalidOperation, null));
                return;
            }

            sessionViewModel.EnqueueLoadSessionRequest(fullPath);
        }
    }
}
