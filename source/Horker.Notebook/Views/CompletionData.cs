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
        public int FirstOffset { get; private set; }
        public int EndOffset { get; private set; }

        public bool NoCompletionFound;

        public CompletionData()
        {
            Text = "(No completion found)";
            Description = null;
            NoCompletionFound = true;
        }

        public CompletionData(CompletionResult result, int firstOffset, int endOffset)
        {
            Text = result.CompletionText;
            Description = result.ToolTip;
            FirstOffset = firstOffset;
            EndOffset = endOffset;
        }

        public CompletionData(string text, string description, int firstOffset, int endOffset)
        {
            Text = text;
            Description = description;
            FirstOffset = firstOffset;
            EndOffset = endOffset;
        }

        public System.Windows.Media.ImageSource Image => null;

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content => Text;

        public object Description { get; private set; }

        public double Priority => 0.0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            if (NoCompletionFound)
                return;

            textArea.Document.Replace(FirstOffset, EndOffset - FirstOffset, Text);
        }
    }
}
