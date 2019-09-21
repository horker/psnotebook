using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Horker.Notebook.Views
{
    public class CompletionData : ICompletionData
    {
        private bool _noCompletionFound;

        public CompletionData()
        {
            Text = "<No completion found>";
            Description = "No completion found";
            _noCompletionFound = true;
        }

        public CompletionData(CompletionResult result)
        {
            Text = result.CompletionText;
            Description = result.ToolTip;
        }

        public System.Windows.Media.ImageSource Image => null;

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => Text;

        public object Description { get; private set; }

        public double Priority => 0.0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            if (_noCompletionFound)
                return;

            textArea.Document.Replace(completionSegment, Text);
        }
    }
}
