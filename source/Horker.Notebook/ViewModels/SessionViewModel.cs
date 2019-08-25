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
using System.Windows.Controls;

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

        public UIElementCollection ViewItems => _sessionControl.StackPanel.Children;

        public IEnumerable<RoundtripViewModel> Items
        {
            get
            {
                foreach (var r in _sessionControl.StackPanel.Children)
                    yield return (r as Views.Roundtrip).ViewModel;
            }
        }

        public int ItemCount => _sessionControl.StackPanel.Children.Count;

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
        }

        // Methods

        public void Reindex()
        {
            _sessionControl.Dispatcher.Invoke(() => {
                var i = 0;
                foreach (var r in Items)
                    r.Index = i++;
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
                r.Index = ItemCount;
                _sessionControl.AddRoundtrip(r, position);
            });
        }

        public RoundtripViewModel GetNextRoundtripViewModel(RoundtripViewModel r)
        {
            var i = r.Index;

            RoundtripViewModel result = null;
            _sessionControl.Dispatcher.Invoke(() => {
                if (i >= ItemCount - 1)
                    result = (ViewItems[ViewItems.Count - 1] as Views.Roundtrip).ViewModel;

                result = (ViewItems[i + 1] as Views.Roundtrip).ViewModel;
            });

            return result;
        }

        public void ScrollToBottom()
        {
            _sessionControl.Dispatcher.Invoke(() => {
                _sessionControl.ScrollViewer.ScrollToBottom();
            });
        }

        public void InsertRoundtrip(RoundtripViewModel after)
        {
            var index = after.Index + 1;
            _model.CreateNewRoundtrip(false, index);
            _sessionControl.Dispatcher.Invoke(() => {
                Reindex();
                ViewItems[index].Focus();
            });
        }

        public void RemoveRoundtrip(RoundtripViewModel r)
        {
            _sessionControl.Dispatcher.Invoke(() => {
                if (ViewItems.Count <= 1)
                    return;

                var index = r.Index;
                Debug.Assert(index == ViewItems.IndexOf(r.Control));
                ViewItems.RemoveAt(index);

                Reindex();

                if (index == ViewItems.Count)
                    ViewItems[ViewItems.Count - 1].Focus();
                else
                    ViewItems[index].Focus();
            });
        }

        public void MoveRoundtrip(RoundtripViewModel r, int newIndex)
        {
            _sessionControl.Dispatcher.Invoke(() => {
                var index = r.Index;
                if (newIndex < 0 || index == newIndex || newIndex >= ViewItems.Count)
                    return;

                var rr = ViewItems[newIndex] as Views.Roundtrip;

                ViewItems.RemoveAt(index);
                ViewItems.Insert(newIndex, r.Control);
                Reindex();

                r.Control.CommandLine.Focus();
            });
        }

        public void Clear()
        {
            _sessionControl.Dispatcher.Invoke(() => {
                ViewItems.Clear();
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
