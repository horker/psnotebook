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
            _control?.Dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
            });
        }

        // References to related objects

        private Views.Session _control;

        public Models.Session Model { get; set; }

        public static RoundtripViewModel ActiveOutput { get; set; }

        // View properties

        public UIElementCollection ViewItems => _control.StackPanel.Children;

        public IEnumerable<RoundtripViewModel> Items
        {
            get
            {
                foreach (var r in _control.StackPanel.Children)
                    yield return (r as Views.Roundtrip).ViewModel;
            }
        }

        public RoundtripViewModel this[int index]
        {
            get
            {
                RoundtripViewModel result = null;
                _control.Dispatcher.Invoke(() => {
                    result = (ViewItems[index] as Views.Roundtrip).ViewModel;
                });
                return result;
            }
        }

        public int ItemCount => _control.StackPanel.Children.Count;

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

        private bool _isTextChanged;

        public bool IsTextChanged
        {
            get => _isTextChanged;
            set
            {
                _isTextChanged = value;
                OnPropertyChanged(nameof(IsTextChanged));
                OnPropertyChanged(nameof(TitleString));
            }
        }

        private bool _isEditorModeByDefault = true;

        public bool IsEditorModeByDefault
        {
            get => _isEditorModeByDefault;
            set
            {
                _isEditorModeByDefault = value;
                OnPropertyChanged(nameof(IsEditorModeByDefault));
            }
        }

        public string TitleString
        {
            get
            {
                string f = null;
                if (string.IsNullOrEmpty(_fileName))
                    f = "Untitled";
                else
                    f = Regex.Replace(_fileName, "(.+[/\\\\])?(.+)$", "$2");

                string changed = _isTextChanged ? "*" : "";
                return f + changed + " - PowerShlell Notebook";
            }
        }

        // Constructor

        public SessionViewModel(Views.Session sessionControl)
        {
            _control = sessionControl;

            _isEditorModeByDefault = true;
        }

        // Methods

        public void Reindex()
        {
            _control.Dispatcher.Invoke(() => {
                var i = 0;
                foreach (var r in Items)
                    r.Index = i++;
            });
        }

        public RoundtripViewModel GetLastItem()
        {
            RoundtripViewModel r = null;
            _control.Dispatcher.Invoke(() => {
                r = Items.Last();
            });
            return r;
        }

        public bool IsLastItem(RoundtripViewModel r)
        {
            bool result = false;
            _control.Dispatcher.Invoke(() => {
                result = r == Items.Last();
            });
            return result;
        }

        public void AddRoundtripViewModel(RoundtripViewModel r, int position = -1)
        {
            _control.Dispatcher.Invoke(() => {
                r.Index = ItemCount;
                r.IsEditorMode = _isEditorModeByDefault;
                _control.AddRoundtrip(r, position);
                IsTextChanged = true;
            });
        }

        public RoundtripViewModel GetNextRoundtripViewModel(RoundtripViewModel r)
        {
            var i = r.Index;

            RoundtripViewModel result = null;
            _control.Dispatcher.Invoke(() => {
                if (i >= ItemCount - 1)
                    result = (ViewItems[ViewItems.Count - 1] as Views.Roundtrip).ViewModel;

                result = (ViewItems[i + 1] as Views.Roundtrip).ViewModel;
            });

            return result;
        }

        public void ScrollToBottom()
        {
            _control.Dispatcher.Invoke(() => {
                _control.ScrollViewer.ScrollToBottom();
            });
        }

        public void InsertRoundtrip(RoundtripViewModel after)
        {
            var index = after.Index + 1;
            Model.CreateNewRoundtrip(false, index);
            _control.Dispatcher.Invoke(() => {
                Reindex();
                ViewItems[index].Focus();
                IsTextChanged = true;
            });
        }

        public void RemoveRoundtrip(RoundtripViewModel r)
        {
            _control.Dispatcher.Invoke(() => {
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

                IsTextChanged = true;
            });
        }

        public void RemoveRoundtripAt(int index)
        {
            _control.Dispatcher.Invoke(() => {
                if (ViewItems.Count <= 1)
                    return;

                ViewItems.RemoveAt(index);

                Reindex();

                if (index == ViewItems.Count)
                    ViewItems[ViewItems.Count - 1].Focus();
                else
                    ViewItems[index].Focus();

                IsTextChanged = true;
            });
        }

        public void MoveRoundtrip(RoundtripViewModel r, int newIndex)
        {
            _control.Dispatcher.Invoke(() => {
                var index = r.Index;
                if (newIndex < 0 || index == newIndex || newIndex >= ViewItems.Count)
                    return;

                var rr = ViewItems[newIndex] as Views.Roundtrip;

                ViewItems.RemoveAt(index);
                ViewItems.Insert(newIndex, r.Control);
                Reindex();

                r.Control.CommandLine.Focus();
                IsTextChanged = true;
            });
        }

        public void Clear()
        {
            _control.Dispatcher.Invoke(() => {
                ViewItems.Clear();
                IsTextChanged = true;
            });
        }

        public void ShowProgress()
        {
            _control.Dispatcher.Invoke(() => {
                _control.ShowProgress();
            });
        }

        public void HideProgress()
        {
            _control.Dispatcher.Invoke(() => {
                _control.HideProgress();
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
            Model.NotifyCancel();
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
                Model.SaveSession();
            }
            catch (Exception ex)
            {
                _control.Dispatcher.Invoke(() => {
                    MessageBox.Show(ex.Message, "Error Occurred on Saving");
                });
            }

            _control.Dispatcher.Invoke(() => {
                IsTextChanged = false;
            });
        }

        public void EnqueueLoadSessionRequest(string fileName)
        {
            Model.EnqueueLoadSessionRequest(fileName);
        }

        public void ShowMessageBox(string message, string caption)
        {
            _control.Dispatcher.Invoke(() => {
                MessageBox.Show(message, caption);
            });
        }
    }
}
