using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horker.ViewModels;

namespace Horker.Models
{
    public class Roundtrip
    {
        private RoundtripViewModel _viewModel;
        private ExecutionQueue _executionQueue;

        public RoundtripViewModel ViewModel => _viewModel;

        public Roundtrip(ExecutionQueue q)
        {
            _viewModel = new RoundtripViewModel(this);
            _executionQueue = q;
        }

        public void NotifyExecute()
        {
            _executionQueue.Enqueue(this);
        }
    }
}
