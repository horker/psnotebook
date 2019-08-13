using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static RoundtripViewModel Active { get; private set; }

        private static int nextIndex = 1;

        private Models.Roundtrip _model;
        private int _index;
        private Views.Roundtrip _control;

        public RoundtripViewModel(Models.Roundtrip r)
        {
            _index = nextIndex++;
            _model = r;
        }
        
        public Views.Roundtrip Control
        {
            get => _control;
            set => _control = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Models.Roundtrip Model => _model;

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        public string IndexString => _index.ToString("d4");

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

        protected void OnPropertyChanged(string info)
        {
            _control.Dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
            });
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

        public void ResolveNewline()
        {
            if (_newlinePending)
                _control.OutputControl.Document.Blocks.Add(new Paragraph());

            _newlinePending = false;
        }

        public void Write(string text, Brush foreground = null, Brush background = null)
        {
            _control.Dispatcher.Invoke(() => {
                _control.OutputControl.Visibility = Visibility.Visible;

                ResolveNewline();

                var run = GetRun(text, foreground, background);
                ((Paragraph)_control.OutputControl.Document.Blocks.LastBlock).Inlines.Add(run);

                _control.ScrollToBottom();
            });
        }

        public void WriteLine(string text, Brush foreground = null, Brush background = null)
        {
            _control.Dispatcher.Invoke(() => {
                _control.OutputControl.Visibility = Visibility.Visible;

                ResolveNewline();

                var run = GetRun(text, foreground, background);

                if (!string.IsNullOrEmpty(text))
                    ((Paragraph)_control.OutputControl.Document.Blocks.LastBlock).Inlines.Add(run);

                _newlinePending = true;

                _control.ScrollToBottom();
            });
        }

        public void WriteWholeLine(string text, Brush foreground = null, Brush background = null)
        {
            _control.Dispatcher.Invoke(() => {
                _control.OutputControl.Visibility = Visibility.Visible;

                ResolveNewline();

                var run = GetRun(text, foreground, background);

                var par = (Paragraph)_control.OutputControl.Document.Blocks.LastBlock;
                if (par.Inlines.Count > 0)
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

                ResolveNewline();

                var container = new InlineUIContainer(uiElement);
                ((Paragraph)_control.OutputControl.Document.Blocks.LastBlock).Inlines.Add(container);

                _newlinePending = true;

                _control.ScrollToBottom();
            });
        }

        public void Clear()
        {
            _control.Dispatcher.Invoke(() => {
                _control.OutputControl.Document.Blocks.Clear();
                _control.OutputControl.Document.Blocks.Add(new Paragraph());
            });
        }

        public void Hidden()
        {
            _control.Dispatcher.Invoke(() => {
                _control.OutputControl.Visibility = Visibility.Collapsed;
            });
        }

        public void NotifyExecute()
        {
            Active = this;
            Model.NotifyExecute();
        }

        public void Focus()
        {
            _control.Dispatcher.Invoke(() => {
                _control?.CommandLineControl?.Focus();
            });
        }
    }
}
