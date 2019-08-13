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
    }
}
