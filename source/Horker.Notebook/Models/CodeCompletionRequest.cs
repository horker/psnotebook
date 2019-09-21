using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Horker.Notebook.Models
{
    public class CodeCompletionRequest : ExecutionQueueItem
    {
        private ManualResetEvent _event;

        public string Input { get; private set; }
        public int CaretOffset { get; private set; }

        private CommandCompletion _completionResult;

        public CommandCompletion CompletionResult
        {
            get => _completionResult;

            set
            {
                _completionResult = value;
                _event.Set();
            }
        }

        public CodeCompletionRequest(string input, int caretOffset)
        {
            Input = input;
            CaretOffset = caretOffset;

            _event = new ManualResetEvent(false);
        }

        public void WaitToComplete()
        {
            _event.WaitOne(3 * 1000);
        }
    }
}
