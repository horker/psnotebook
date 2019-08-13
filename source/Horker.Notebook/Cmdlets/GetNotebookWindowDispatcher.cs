using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Horker.Cmdlets
{
    [Cmdlet("Get", "NotebookWindowDispatcher")]
    public class GetNotebookWindowDispatcher : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            WriteObject(Models.Startup.Dispatcher);
        }
    }
}
