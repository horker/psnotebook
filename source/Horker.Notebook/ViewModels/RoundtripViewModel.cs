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
            Control?.Dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
            });
        }

        public RoundtripViewModel(Models.Roundtrip r)
        {
            _index = 0; // Index is set by Session
            Model = r;
            CreatedEvent = new ManualResetEvent(false);
        }

        public ManualResetEvent CreatedEvent { get; }

        public Views.Roundtrip Control { get; set; }

        public Models.Roundtrip Model { get; }

        private int _index;

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
                string text = null;
                Control.Dispatcher.Invoke(() => {
                    text = Control.CommandLine.Text;
                });
                return text;
            }
        }

        public string Output
        {
            get
            {
                return new TextRange(
                    Control.Output.Document.ContentStart,
                    Control.Output.Document.ContentEnd).Text;
            }
        }

        private bool _isEditorMode = false;

        public bool IsEditorMode
        {
            get => _isEditorMode;
            set
            {
                _isEditorMode = value;
                OnPropertyChanged(nameof(IsEditorMode));
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
            Control.Dispatcher.Invoke(() => {
                Control.CommandLine.Text = line;
            });
        }

        private void ResolveNewline()
        {
            if (_newlinePending)
                Control.Output.Document.Blocks.Add(new Paragraph());

            _newlinePending = false;
        }

        private void WriteInternal(string text, Brush foreground, Brush background, bool newLine)
        {
            Control.Dispatcher.Invoke(() => {
                Control.Output.Visibility = Visibility.Visible;

                ResolveNewline();

                var run = GetRun(text, foreground, background);

                var par = Control.Output.Document.Blocks.LastBlock as Paragraph;
                if (par == null)
                {
                    par = new Paragraph();
                    Control.Output.Document.Blocks.Add(par);
                }

                par.Inlines.Add(run);

                _newlinePending = newLine;

                Control.ScrollToBottom();
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
            Control.Dispatcher.Invoke(() => {
                Control.Output.Visibility = Visibility.Visible;

                ResolveNewline();

                var run = GetRun(text, foreground, background);

                var par = (Paragraph)Control.Output.Document.Blocks.LastBlock;
                if (par == null || par.Inlines.Count > 0)
                {
                    par = new Paragraph();
                    Control.Output.Document.Blocks.Add(par);
                }

                par.Inlines.Add(run);
                _newlinePending = true;

                Control.ScrollToBottom();
            });
        }

        public void WriteUIElement(UIElement uiElement)
        {
            Control.Dispatcher.Invoke(() => {
                Control.Output.Visibility = Visibility.Visible;

                // WPF objects that have no default size appears in (zero, zero) size.
                // Give sizes explicitly to such objects.

                uiElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (uiElement.DesiredSize.Width == 0 || uiElement.DesiredSize.Height == 0)
                {
                    var grid = new Grid()
                    {
                        Width = Models.Configuration.DefaultWpfElementWidth,
                        Height = Models.Configuration.DefaultWpfElementHeight
                    };
                    grid.Children.Add(uiElement);
                    uiElement = grid;
                }

                var container = new InlineUIContainer(uiElement);
                var par = new Paragraph(container);
                Control.Output.Document.Blocks.Add(par);

                _newlinePending = true;

                Control.ScrollToBottom();
            });
        }

        public void Clear()
        {
            Control.Dispatcher.Invoke(() => {
                Control.Output.Document.Blocks.Clear();
            });
        }

        public void Hidden()
        {
            Control.Dispatcher.Invoke(() => {
                Control.Output.Visibility = Visibility.Collapsed;
            });
        }

        public void NotifyExecute(bool moveToNext)
        {
            SessionViewModel.ActiveOutput = this;
            Model.NotifyExecute(moveToNext);
        }

        public void Focus()
        {
            Control?.Dispatcher.Invoke(() => {
                Control.CommandLine?.Focus();
            });
        }

        public bool IsOutputEmpty()
        {
            var result = true;

            Control?.Dispatcher.Invoke(() => {
                var doc = Control.Output.Document;
                result = doc.Blocks.Count == 0 || ((Paragraph)doc.Blocks.LastBlock).Inlines.Count == 0;
            });

            return result;
        }

        public void ShowExecutionWaiting()
        {
            Control?.Dispatcher.Invoke(() => {
                Control.Index.Background = Brushes.PowderBlue;
            });
        }

        public void ShowExecuting()
        {
            Control?.Dispatcher.Invoke(() => {
                Control.Index.Background = Brushes.Bisque;
            });
        }

        public void ShowEditing()
        {
            Control?.Dispatcher.Invoke(() => {
                Control.Index.Background = Brushes.Gainsboro;
            });
        }
    }
}
