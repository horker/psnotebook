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
        [Parameter(Position = 0, Mandatory = false)]
        public string ScriptFile = "";

        protected override void BeginProcessing()
        {
            var path = typeof(StartNotebook).Assembly.Location;
            path = path.Substring(0, path.LastIndexOf("\\"));
            path = Path.Combine(path, "NotebookApp.exe");

            if (!string.IsNullOrEmpty(ScriptFile))
            {
                var current = SessionState.Path.CurrentFileSystemLocation.Path;
                ScriptFile = Path.Combine(current, ScriptFile);
            }

            WriteVerbose($"Script file: {ScriptFile}");

            Process.Start(path, ScriptFile);
        }
    }
}
