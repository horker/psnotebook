using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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

        // References to related objects

        private Views.Session _sessionControl;

        private Models.Session _model;

        public Models.Session Model
        {
            get => _model;
            set => _model = value;
        }

        public static RoundtripViewModel ActiveOutput { get; set; }

        // View properties

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

        private string _progressMessage;

        public string ProgressMessage
        {
            get => _progressMessage;
            set
            {
                _progressMessage = value;
                OnPropertyChanged(nameof(ProgressMessage));
            }
        }

        private double _progress;

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressPercentString));
            }
        }

        public string ProgressPercentString => $"{_progress}%";

        private string _fileName;

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
                OnPropertyChanged(nameof(TitleString));
            }
        }

        public string TitleString
        {
            get
            {
                if (string.IsNullOrEmpty(_fileName))
                    return "PowerShell Notebook";
                return Regex.Replace(_fileName, "(.+[/\\\\])?(.+)$", "$2") + " - PowerShlell Notebook";
            }
        }

        // Constructor

        public SessionViewModel(Views.Session sessionControl)
        {
            _sessionControl = sessionControl;
            _items = new ObservableCollection<RoundtripViewModel>();
        }

        // Methods

        private void Reindex(int start)
        {
            _sessionControl.Dispatcher.Invoke(() => {
                for (var i = start; i < Items.Count; ++i)
                    Items[i].Index = i;
            });
        }

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

        public void AddRoundtripViewModel(RoundtripViewModel r, int position = -1)
        {
            _sessionControl.Dispatcher.Invoke(() => {
                r.Index = Items.Count;
                if (position == -1)
                    Items.Add(r);
                else
                {
                    Items.Insert(position, r);
                }
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

        public void InsertRoundtrip(RoundtripViewModel r)
        {
            var index = r.Index + 1;
            _model.CreateNewRoundtrip(false, index);
            _sessionControl.Dispatcher.Invoke(() => {
                Reindex(index);
                Items[index].Focus();
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

                Reindex(index);

                if (index == Items.Count)
                    Items.Last().Focus();
                else
                    Items[index].Focus();
            });
        }

        public void Clear()
        {
            _sessionControl.Dispatcher.Invoke(() => {
                Items.Clear();
            });
        }

        public void ShowProgress()
        {
            _sessionControl.Dispatcher.Invoke(() => {
                _sessionControl.ShowProgress();
            });
        }

        public void HideProgress()
        {
            _sessionControl.Dispatcher.Invoke(() => {
                _sessionControl.HideProgress();
            });
        }

        public void WriteProgress(string message, double progress)
        {
            ShowProgress();
            ProgressMessage = message;
            Progress = progress;
        }

        public void NotifyExecuteAll()
        {
            foreach (var item in Items)
                item.NotifyExecute(false);
        }

        public void NotifyCancel()
        {
            _model.NotifyCancel();
        }

        public bool HasFileName()
        {
            return !string.IsNullOrEmpty(FileName);
        }

        public void SaveSession(string fileName = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(fileName))
                    FileName = fileName;
                _model.SaveSession();
            }
            catch (Exception ex)
            {
                _sessionControl.Dispatcher.Invoke(() => {
                    MessageBox.Show(ex.Message, "Error Occurred on Saving");
                });
            }
        }

        public void EnqueueLoadSessionRequest(string fileName)
        {
            _model.EnqueueLoadSessionRequest(fileName);
        }

        public void ShowMessageBox(string message, string caption)
        {
            _sessionControl.Dispatcher.Invoke(() => {
                MessageBox.Show(message, caption);
            });
        }
    }
}
