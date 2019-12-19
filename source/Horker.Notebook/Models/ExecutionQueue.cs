using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Horker.Notebook.Models
{
    public interface ExecutionQueueItem
    {
    }

    public class ExecutionRequest : ExecutionQueueItem
    {
        private static ExecutionRequest _loadSessionRequest = new ExecutionRequest(null, false);

        public Roundtrip Roundtrip { get; set; }
        public bool MoveToNext { get; set; }

        public ExecutionRequest(Roundtrip r, bool moveToNext)
        {
            Roundtrip = r;
            MoveToNext = moveToNext;
        }

        public static ExecutionRequest GetLoadSessionRequest()
        {
            return _loadSessionRequest;
        }
    }

    public class LoadSessionRequest : ExecutionQueueItem
    {
        public bool RunAfterLoad { get; private set; }

        public LoadSessionRequest(bool runAfterLoad)
        {
            RunAfterLoad = runAfterLoad;
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

        public void Enqueue(ExecutionQueueItem item)
        {
            _queue.Add(item, _cancellationToken);
        }

        public bool IsEmpty()
        {
            return _queue.Count == 0;
        }

        public ExecutionQueueItem Dequeue()
        {
            // lock??
            return _queue.Take(_cancellationToken);
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public IEnumerable<ExecutionQueueItem> Enumerate()
        {
            return _queue.GetConsumingEnumerable();
        }
    }
}
