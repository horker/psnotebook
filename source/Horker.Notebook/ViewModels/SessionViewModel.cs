using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Horker.Notebook.ViewModels
{
    public class SessionViewModel
    {
        private Views.Session _sessionControl;

        public SessionViewModel(Views.Session sessionControl)
        {
            _sessionControl = sessionControl;
            sessionControl.ViewModel = this;
        }

        public bool IsLastItem(RoundtripViewModel r)
        {
            bool result = false;
            _sessionControl.Dispatcher.Invoke(() => {
                result = r == _sessionControl.Items.Last();
            });
            return result;
        }

        public void AddRoundtripViewModel(RoundtripViewModel r)
        {
            _sessionControl.Dispatcher.Invoke(() => {
                r.Index = _sessionControl.Items.Count;
                _sessionControl.Items.Add(r);
            });
        }

        public RoundtripViewModel GetNextRoundtripViewModel(RoundtripViewModel r)
        {
            var i = r.Index;

            if (i >= _sessionControl.Items.Count - 1)
                return _sessionControl.Items.Last();

            return _sessionControl.Items[i + 1];
        }

        public void ScrollToBottom()
        {
            _sessionControl.Dispatcher.Invoke(() => {
                _sessionControl.ScrollViewer.ScrollToBottom();
            });
        }

        public void RemoveRoundtrip(RoundtripViewModel r)
        {
            if (_sessionControl.Items.Count <= 1)
                return;

            _sessionControl.Dispatcher.Invoke(() => {
                var index = r.Index;
                Debug.Assert(index == _sessionControl.Items.IndexOf(r));
                _sessionControl.Items.RemoveAt(index);

                for (var i = index; i < _sessionControl.Items.Count; ++i)
                    _sessionControl.Items[i].Index = i;

                if (index == _sessionControl.Items.Count)
                    _sessionControl.Items.Last().Focus();
                else
                    _sessionControl.Items[index].Focus();
            });
        }
    }
}
