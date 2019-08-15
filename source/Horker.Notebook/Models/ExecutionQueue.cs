using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Horker.Notebook.Models
{
    public class ExecutionQueueItem
    {
        private static ExecutionQueueItem _loadSessionRequest = new ExecutionQueueItem(null, false);

        public Roundtrip Roundtrip { get; set; }
        public bool MoveToNext { get; set; }

        public ExecutionQueueItem(Roundtrip r, bool moveToNext)
        {
            Roundtrip = r;
            MoveToNext = moveToNext;
        }

        public static ExecutionQueueItem GetLoadSessionRequest()
        {
            return _loadSessionRequest;
        }

        public bool IsLoadSessionRequest()
        {
            return ReferenceEquals(this, _loadSessionRequest);
        }
    }

    public class ExecutionQueue
    {
        private BlockingCollection<ExecutionQueueItem> _queue;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public ExecutionQueue()
        {
            _queue = new BlockingCollection<ExecutionQueueItem>();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        public void Enqueue(Roundtrip r, bool moveToNext)
        {
            _queue.Add(new ExecutionQueueItem(r, moveToNext), _cancellationToken);
        }

        public void EnqueueLoadSessionRequest()
        {
            _queue.Add(ExecutionQueueItem.GetLoadSessionRequest(), _cancellationToken);
        }

        public ExecutionQueueItem Dequeue()
        {
            return _queue.Take(_cancellationToken);
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
