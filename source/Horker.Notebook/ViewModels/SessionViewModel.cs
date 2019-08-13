using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Horker.ViewModels
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
                _sessionControl.Items.Add(r);
            });
        }

        public RoundtripViewModel GetNextRoundtripViewModel(RoundtripViewModel r)
        {
            var i = r.Index - 1; // Roundtrip indexes begin at 1.
            _sessionControl.Dispatcher.Invoke(() => {
                RoundtripViewModel rr = null;
                while (i >= 0)
                {
                    rr = _sessionControl.Items[i];
                    if (r == rr)
                        break;
                    --i;
                }
            });

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
    }
}
