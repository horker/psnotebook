using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Horker.Models
{
    public class ExecutionQueue
    {
        private BlockingCollection<Roundtrip> _queue;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public ExecutionQueue()
        {
            _queue = new BlockingCollection<Roundtrip>();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        public void Enqueue(Roundtrip r)
        {
            _queue.Add(r, _cancellationToken);
        }

        public Roundtrip Dequeue()
        {
            return _queue.Take(_cancellationToken);
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
