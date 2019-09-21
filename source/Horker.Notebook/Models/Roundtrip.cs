using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Horker.Notebook.ViewModels;

namespace Horker.Notebook.Models
{
    public class Roundtrip
    {
        private RoundtripViewModel _viewModel;
        private ExecutionQueue _executionQueue;
        private CodeCompletionRequest _codeCompletionRequest;

        public RoundtripViewModel ViewModel => _viewModel;

        public Roundtrip(ExecutionQueue q)
        {
            _viewModel = new RoundtripViewModel(this);
            _executionQueue = q;
        }

        public void NotifyExecute(bool moveToNext)
        {
            _executionQueue.Enqueue(new  ExecutionRequest(this, moveToNext));
            _viewModel.ShowExecutionWaiting();
        }

        public bool RequestCodeCompletion(string input, int caretOffset)
        {
            if (!_executionQueue.IsEmpty())
            {
                // When execution is ongoing, the completion request is silently ignored to avoid queue congestion.
                // (User would re-enter the TAB key after execution complete.)
                return false;
            }

            _codeCompletionRequest = new CodeCompletionRequest(input, caretOffset);
            _executionQueue.Enqueue(_codeCompletionRequest);

            return true;
        }

        public CommandCompletion WaitForCodeCompletion()
        {
            _codeCompletionRequest.WaitToComplete();

            var result = _codeCompletionRequest.CompletionResult;
            _codeCompletionRequest = null;
            return result;
        }
    }
}
