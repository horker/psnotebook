using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Horker.Notebook.ViewModels
{
    public class SessionViewModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info)
        {
            _sessionControl?.Dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
            });
        }

        private Views.Session _sessionControl;

        public static RoundtripViewModel ActiveOutput { get; set; }

        private ObservableCollection<RoundtripViewModel> _items;

        public ObservableCollection<RoundtripViewModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        private string _commandPrompt;

        public string CommandPrompt
        {
            get => _commandPrompt;
            set
            {
                _commandPrompt = value;
                OnPropertyChanged(nameof(CommandPrompt));
            }
        }

        private TimeSpan _timeTaken;

        public TimeSpan TimeTaken
        {
            get => _timeTaken;
            set
            {
                _timeTaken = value;
                OnPropertyChanged(nameof(TimeTaken));
            }
        }

        public SessionViewModel(Views.Session sessionControl)
        {
            _sessionControl = sessionControl;
            _items = new ObservableCollection<RoundtripViewModel>();
        }

        // Helper methods

        public RoundtripViewModel GetLastItem()
        {
            RoundtripViewModel r = null;
            _sessionControl.Dispatcher.Invoke(() => {
                r = Items.Last();
            });
            return r;
        }

        public bool IsLastItem(RoundtripViewModel r)
        {
            bool result = false;
            _sessionControl.Dispatcher.Invoke(() => {
                result = r == Items.Last();
            });
            return result;
        }

        public void AddRoundtripViewModel(RoundtripViewModel r)
        {
            _sessionControl.Dispatcher.Invoke(() => {
                r.Index = Items.Count;
                Items.Add(r);
            });
        }

        public RoundtripViewModel GetNextRoundtripViewModel(RoundtripViewModel r)
        {
            var i = r.Index;

            if (i >= Items.Count - 1)
                return Items.Last();

            return Items[i + 1];
        }

        public void ScrollToBottom()
        {
            _sessionControl.Dispatcher.Invoke(() => {
                _sessionControl.ScrollViewer.ScrollToBottom();
            });
        }

        public void RemoveRoundtrip(RoundtripViewModel r)
        {
            if (Items.Count <= 1)
                return;

            _sessionControl.Dispatcher.Invoke(() => {
                var index = r.Index;
                Debug.Assert(index == Items.IndexOf(r));
                Items.RemoveAt(index);

                for (var i = index; i < Items.Count; ++i)
                    Items[i].Index = i;

                if (index == Items.Count)
                    Items.Last().Focus();
                else
                    Items[index].Focus();
            });
        }
    }
}
