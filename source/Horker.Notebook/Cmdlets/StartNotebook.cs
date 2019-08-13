using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Horker.Cmdlets
{
    [Cmdlet("Start", "Notebook")]
    public class StartNotebook : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            Models.Startup.Start();
        }
    }
}
