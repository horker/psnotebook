using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Horker.Notebook.Models
{
    public class ExecutionQueue
    {
        private BlockingCollection<Roundtrip> _queue;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private Roundtrip _loadSessionRequest;

        public ExecutionQueue()
        {
            _queue = new BlockingCollection<Roundtrip>();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            _loadSessionRequest = new Roundtrip(null);
        }

        public void Enqueue(Roundtrip r)
        {
            _queue.Add(r, _cancellationToken);
        }

        public void EnqueueLoadSessionRequest()
        {
            _queue.Add(_loadSessionRequest, _cancellationToken);
        }

        public Roundtrip Dequeue()
        {
            return _queue.Take(_cancellationToken);
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        public bool IsLoadSessionRequest(Roundtrip r)
        {
            return ReferenceEquals(r, _loadSessionRequest);
        }
    }
}
