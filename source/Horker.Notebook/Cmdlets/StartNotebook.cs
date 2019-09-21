using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Horker.Notebook.Cmdlets
{
    [Cmdlet("Start", "Notebook")]
    public class StartNotebook : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            var path = typeof(StartNotebook).Assembly.Location;
            path = path.Substring(0, path.LastIndexOf("\\"));
            path = Path.Combine(path, "NotebookApp.exe");

            Process.Start(path);
        }
    }
}
