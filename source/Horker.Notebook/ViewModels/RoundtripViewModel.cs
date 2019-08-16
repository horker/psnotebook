using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Horker.Notebook.ViewModels
{
    public class RoundtripViewModel : INotifyPropertyChanged
    {
        // INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string info)
        {
            _control?.Dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
            });
        }

        private Models.Roundtrip _model;
        private int _index;
        private Views.Roundtrip _control;

        private ManualResetEvent _createdEvent;

        public ManualResetEvent CreatedEvent => _createdEvent;

        public RoundtripViewModel(Models.Roundtrip r)
        {
            _index = 0; // Index is set by Session
            _model = r;
            _createdEvent = new ManualResetEvent(false);
        }
        
        public Views.Roundtrip Control
        {
            get => _control;
            set => _control = value;
        }

        public Models.Roundtrip Model => _model;

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged(nameof(Index));
                OnPropertyChanged(nameof(IndexString));
            }
        }

        public string IndexString => (_index + 1).ToString("d4");

        public string CommandLine
        {
            get
            {
                return new TextRange(
                    _control.CommandLineControl.Document.ContentStart,
                    _control.CommandLineControl.Document.ContentEnd).Text;
            }
        }

        public string Output
        {
            get
            {
                return new TextRange(
                    _control.OutputControl.Document.ContentStart,
                    _control.OutputControl.Document.ContentEnd).Text;
            }
        }

        private Run GetRun(string text, Brush foreground = null, Brush background = null)
        {
            var run = new Run(text);

            if (foreground != null)
                run.Foreground = foreground;

            if (background != null)
                run.Background = background;

            return run;
        }

        private bool _newlinePending;

        public void WriteCommandLine(string line)
        {
            _control.Dispatcher.Invoke(() => {
                _control.CommandLineControl.Document.Blocks.Add(new Paragraph(new Run(line)));
            });
        }

        private void ResolveNewline()
        {
            if (_newlinePending)
                _control.OutputControl.Document.Blocks.Add(new Paragraph());

            _newlinePending = false;
        }

        private void WriteInternal(string text, Brush foreground, Brush background, bool newLine)
        {
            _control.Dispatcher.Invoke(() => {
                _control.OutputControl.Visibility = Visibility.Visible;

                ResolveNewline();

                var run = GetRun(text, foreground, background);

                var par = _control.OutputControl.Document.Blocks.LastBlock as Paragraph;
                if (par == null)
                {
                    par = new Paragraph();
                    _control.OutputControl.Document.Blocks.Add(par);
                }

                par.Inlines.Add(run);

                _newlinePending = newLine;

                _control.ScrollToBottom();
            });
        }

        public void Write(string text, Brush foreground = null, Brush background = null)
        {
            WriteInternal(text, foreground, background, false);
        }

        public void WriteLine(string text, Brush foreground = null, Brush background = null)
        {
            WriteInternal(text, foreground, background, true);
        }

        public void WriteWholeLine(string text, Brush foreground = null, Brush background = null)
        {
            _control.Dispatcher.Invoke(() => {
                _control.OutputControl.Visibility = Visibility.Visible;

                ResolveNewline();

                var run = GetRun(text, foreground, background);

                var par = (Paragraph)_control.OutputControl.Document.Blocks.LastBlock;
                if (par == null || par.Inlines.Count > 0)
                {
                    par = new Paragraph();
                    _control.OutputControl.Document.Blocks.Add(par);
                }

                par.Inlines.Add(run);
                _newlinePending = true;

                _control.ScrollToBottom();
            });
        }

        public void WriteUIElement(UIElement uiElement)
        {
            _control.Dispatcher.Invoke(() => {
                _control.OutputControl.Visibility = Visibility.Visible;

                var container = new InlineUIContainer(uiElement);
                var par = new Paragraph(container);
                _control.OutputControl.Document.Blocks.Add(par);

                _newlinePending = true;

                _control.ScrollToBottom();
            });
        }

        public void Clear()
        {
            _control.Dispatcher.Invoke(() => {
                _control.OutputControl.Document.Blocks.Clear();
            });
        }

        public void Hidden()
        {
            _control.Dispatcher.Invoke(() => {
                _control.OutputControl.Visibility = Visibility.Collapsed;
            });
        }

        public void NotifyExecute(bool moveToNext)
        {
            SessionViewModel.ActiveOutput = this;
            Model.NotifyExecute(moveToNext);
        }

        public void Focus()
        {
            _control?.Dispatcher.Invoke(() => {
                _control.CommandLineControl?.Focus();
            });
        }

        public bool IsOutputEmpty()
        {
            var result = true;

            _control?.Dispatcher.Invoke(() => {
                var doc = _control.OutputControl.Document;
                result = doc.Blocks.Count == 0 || ((Paragraph)doc.Blocks.LastBlock).Inlines.Count == 0;
            });

            return result;
        }
    }
}
