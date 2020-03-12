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
using Horker.Notebook;
using Horker.Notebook.Models;
using Horker.Notebook.ViewModels;

namespace Horker.Notebook.Cmdlets
{
    [Cmdlet("Get", "NotebookOutput")]
    [OutputType(typeof(string))]
    public class GetNotebookOutput : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = false)]
        public int Offset { get; set; } = int.MinValue;

        [Parameter(Position = 1, Mandatory = false)]
        public int Index { get; set; } = int.MinValue;

        protected override void BeginProcessing()
        {
            if ((Offset == int.MinValue && Index == int.MinValue) || (Offset != int.MinValue && Index != int.MinValue))
            {
                WriteError(new ErrorRecord(new RuntimeException("Either Offset or Index should be specified"), "", ErrorCategory.InvalidArgument, null));
                return;
            }

            if (Offset > int.MinValue)
            {
                var active = SessionViewModel.ActiveOutput;
                Index = active.Index - Offset;
            }
            else
            {
                --Index;
            }

            var roundtrip = ApplicationInstance.SessionViewModel[Index];
            WriteObject(roundtrip.Output);
        }
    }
}
