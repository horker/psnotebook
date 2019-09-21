using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Horker.Notebook.ViewModels;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Horker.Notebook.Views
{
    public partial class Roundtrip : UserControl
    {
        private ScrollViewer _outputScrollViewer;

        private ScrollViewer OutputScrollViewer
        {
            get
            {
                // It is possible that this getter will return null.

                if (_outputScrollViewer == null)
                    _outputScrollViewer = Output.Template.FindName("PART_ContentHost", Output) as ScrollViewer;

                return _outputScrollViewer;
            }
        }

        public Session Container { get; set; }

        public RoundtripViewModel ViewModel
        {
            get => (RoundtripViewModel)DataContext;
            set => DataContext = value;
        }

        public Roundtrip(Session container, RoundtripViewModel viewModel)
        {
            InitializeComponent();

            InitializeCommandBindings();
            InitializeKeyBindings();
            InitializeCodeCompletion();

            Container = container;
            DataContext = viewModel;
            viewModel.Control = this;

            UpdateEditorMode();

            CommandLine.Document.TextChanged += CommandLine_Document_TextChanged;

            CommandLine.TextArea.Caret.PositionChanged += (object s, EventArgs ee) => {
                CommandLine.TextArea.Caret.BringCaretToView();
            };
        }

        public void ScrollToBottom()
        {
            OutputScrollViewer?.ScrollToBottom();
        }

        // Commands

        public static readonly RoutedCommand EnterCommand = new RoutedCommand("EnterCommand", typeof(Roundtrip));
        public static readonly RoutedCommand ShiftEnterCommand = new RoutedCommand("ShiftEnterCommand", typeof(Roundtrip));
        public static readonly RoutedCommand CtrlEnterCommand = new RoutedCommand("CtrlEnterCommand", typeof(Roundtrip));
        public static readonly RoutedCommand PreviousRoundtripCommand = new RoutedCommand("PreviousRoundtripCommnad", typeof(Roundtrip));
        public static readonly RoutedCommand NextRoundtripCommand = new RoutedCommand("NextRoundtripCommand", typeof(Roundtrip));
        public static readonly RoutedCommand InsertTabCommand = new RoutedCommand("InsertTabCommand", typeof(Roundtrip));
        public static readonly RoutedCommand UpCommand = new RoutedCommand("UpCommand", typeof(Roundtrip));
        public static readonly RoutedCommand DownCommand = new RoutedCommand("DownCommand", typeof(Roundtrip));
        public static readonly RoutedCommand CursorToTopCommand = new RoutedCommand("CursorToTopCommand", typeof(Roundtrip));
        public static readonly RoutedCommand CursorToBottomCommand = new RoutedCommand("CursorToBottomCommand", typeof(Roundtrip));

        private void InitializeCommandBindings()
        {
            CommandBindings.Add(new CommandBinding(EnterCommand, EnterCommand_Execute));
            CommandBindings.Add(new CommandBinding(ShiftEnterCommand, ShiftEnterCommand_Execute));
            CommandBindings.Add(new CommandBinding(CtrlEnterCommand, CtrlEnterCommand_Execute));
            CommandBindings.Add(new CommandBinding(PreviousRoundtripCommand, PreviousRoundtripCommand_Execute));
            CommandBindings.Add(new CommandBinding(NextRoundtripCommand, NextRoundtripCommand_Execute));
            CommandBindings.Add(new CommandBinding(InsertTabCommand, InsertTabCommand_Execute));
            CommandBindings.Add(new CommandBinding(UpCommand, UpCommand_Execute));
            CommandBindings.Add(new CommandBinding(DownCommand, DownCommand_Execute));
            CommandBindings.Add(new CommandBinding(CursorToTopCommand, CursorToTopCommand_Execute));
            CommandBindings.Add(new CommandBinding(CursorToBottomCommand, CursorToBottomCommand_Execute));
        }

        private void InitializeKeyBindings()
        {
            DefineKeyBinding(new KeyBinding(EnterCommand, new KeyGesture(Key.Enter)));
            DefineKeyBinding(new KeyBinding(ShiftEnterCommand, new KeyGesture(Key.Enter, ModifierKeys.Shift)));
            DefineKeyBinding(new KeyBinding(CtrlEnterCommand, new KeyGesture(Key.Enter, ModifierKeys.Control)));
            DefineKeyBinding(new KeyBinding(PreviousRoundtripCommand, new KeyGesture(Key.Up, ModifierKeys.Control)));
            DefineKeyBinding(new KeyBinding(NextRoundtripCommand, new KeyGesture(Key.Down, ModifierKeys.Control)));
            DefineKeyBinding(new KeyBinding(InsertTabCommand, new KeyGesture(Key.Tab)));
            DefineKeyBinding(new KeyBinding(UpCommand, new KeyGesture(Key.Up)));
            DefineKeyBinding(new KeyBinding(DownCommand, new KeyGesture(Key.Down)));
            DefineKeyBinding(new KeyBinding(CursorToTopCommand, new KeyGesture(Key.Left, ModifierKeys.Control)));
            DefineKeyBinding(new KeyBinding(CursorToBottomCommand, new KeyGesture(Key.Right, ModifierKeys.Control)));
        }

        private void DefineKeyBinding(KeyBinding kb)
        {
            var editingKeyBindings = CommandLine.TextArea.DefaultInputHandler.Editing.InputBindings.OfType<KeyBinding>();

            var existing = editingKeyBindings.FirstOrDefault(b => b.Key == kb.Key && b.Modifiers == kb.Modifiers);

            if (existing != null)
                CommandLine.TextArea.DefaultInputHandler.Editing.InputBindings.Remove(existing);

            CommandLine.TextArea.DefaultInputHandler.Editing.InputBindings.Add(kb);
        }

        private void EnterCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.IsEditorMode)
                CommandLine.Document.Insert(CommandLine.CaretOffset, "\n");
            else
                ViewModel.NotifyExecute(true);
        }

        private void ShiftEnterCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.IsEditorMode)
                ViewModel.NotifyExecute(true);
            else
                CommandLine.Document.Insert(CommandLine.CaretOffset, "\n");
        }

        private void CtrlEnterCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.IsEditorMode)
                ViewModel.NotifyExecute(false);
            else
                CommandLine.Document.Insert(CommandLine.CaretOffset, "\n");
        }

        private void PreviousRoundtripCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Container.MoveToPreviousRoundtrip();
        }

        private void NextRoundtripCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Container.MoveToNextRoundtrip();
        }

        private void InsertTabCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var line = CommandLine.Document.GetLineByOffset(CommandLine.CaretOffset);
            var bol = CommandLine.Document.GetText(line.Offset, CommandLine.CaretOffset - line.Offset);

            var allWhitespace = true;
            foreach (var ch in bol)
            {
                if (!char.IsWhiteSpace(ch))
                {
                    allWhitespace = false;
                    break;
                }
            }

            if (allWhitespace)
                CommandLine.Document.Insert(CommandLine.CaretOffset, "    ");
            else
            {
                if (ViewModel.RequestCodeCompletion(CommandLine.Text, CommandLine.CaretOffset) == false)
                    OpenCompletionWindow(null, true);

                else
                {
                    var completion = ViewModel.WaitForCodeCompletion();
                    OpenCompletionWindow(completion, true);
                }
            }
        }

        private void UpCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var position = CommandLine.TextArea.Caret.Position;
            --CommandLine.TextArea.Caret.Line;
            if (position.CompareTo(CommandLine.TextArea.Caret.Position) == 0)
                Container.MoveToPreviousRoundtrip();
        }

        private void DownCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var position = CommandLine.TextArea.Caret.Position;
            ++CommandLine.TextArea.Caret.Line;
            if (position.CompareTo(CommandLine.TextArea.Caret.Position) == 0)
                Container.MoveToNextRoundtrip();
        }

        private void CursorToTopCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            CommandLine.TextArea.Caret.Offset = 0;
        }

        private void CursorToBottomCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            CommandLine.TextArea.Caret.Offset = CommandLine.TextArea.Document.TextLength;
        }

        // Menu items

        private void InsertNewRoundtrip_Click(object sender, RoutedEventArgs e)
        {
            Container.ViewModel.InsertRoundtrip(ViewModel);
        }

        private void DeleteRoundtrip_Click(object sender, RoutedEventArgs e)
        {
            Container.ViewModel.RemoveRoundtrip(ViewModel);
        }

        private void ClearOutput_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Clear();
            ViewModel.Hidden();
            CommandLine.Focus();
        }

        private void MoveUpRoundtrip_Click(object sender, RoutedEventArgs e)
        {
            Container.MoveRoundtrip(this, ViewModel.Index - 1);
        }

        private void MoveDownRoundtrip_Click(object sender, RoutedEventArgs e)
        {
            Container.MoveRoundtrip(this, ViewModel.Index + 1);
        }

        private void MoveToTopRoundtrip_Click(object sender, RoutedEventArgs e)
        {
            Container.MoveRoundtrip(this, 0);
        }

        private void MoveToBottomRoundtrip_Click(object sender, RoutedEventArgs e)
        {
            Container.MoveRoundtrip(this, Container.StackPanel.Children.Count - 1);
        }

        private void UpdateEditorMode()
        {
        }

        private void EditorMode_Click(object sender, RoutedEventArgs e)
        {
            UpdateEditorMode();
            CommandLine.Focus();
        }

        // Event handlers

        private void RoundtripControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Address RichTextBox's known limitation that its document's width is not stretched automatically.
            // (source: https://stackoverflow.com/questions/350863/wpf-richtextbox-with-no-width-set)
            Output.Document.PageWidth = Models.Configuration.ConsoleWidth;

            CommandLine.Focus();
            CommandLine.TextArea.Caret.BringCaretToView();

            ViewModel.CreatedEvent.Set();
        }

        private void CommandLine_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsEditorMode)
                CommandLine.Background = Brushes.FloralWhite;
            else
                CommandLine.Background = Brushes.Beige;
        }

        private void CommandLine_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            CommandLine.Background = Brushes.WhiteSmoke;
        }

        private void Roundtrip_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (OutputScrollViewer?.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            {
                e.Handled = true;
                var args = new MouseWheelEventArgs((MouseDevice)e.Device, e.Timestamp, e.Delta);
                args.RoutedEvent = MouseWheelEvent;
                RaiseEvent(args);
            }
        }

        private void CommandLine_Document_TextChanged(object sender, EventArgs e)
        {
            if (Container != null && Container.ViewModel != null)
                Container.ViewModel.IsTextChanged = true;
        }

        // Code completion

        private CompletionWindow _completionWindow;
        private bool _inlineCompletion;
        private int _completionStartOffset;
        private int _completionEndOffset;

        void InitializeCodeCompletion()
        {
            CommandLine.TextArea.TextEntering += CommandLine_TextEntering;
            _inlineCompletion = Models.Configuration.InlineCompletion;
        }

        void OpenCompletionWindow(CommandCompletion completion, bool byUserAction)
        {
            if (!byUserAction && (completion == null || completion.CompletionMatches.Count == 0))
                return;

            _completionWindow = new CompletionWindow(CommandLine.TextArea)
            {
                FontFamily = Models.Configuration.FontFamily,
                SizeToContent = SizeToContent.WidthAndHeight,
                MinWidth = 75,
                MaxWidth = 500
            };

            _completionWindow.PreviewKeyDown += (object sender, KeyEventArgs e) => {
                if (e.Key == Key.Tab)
                {
                    var listBox = _completionWindow?.CompletionList?.ListBox;
                    if (listBox == null)
                        return;

                    if (listBox.SelectedIndex == listBox.Items.Count - 1)
                        listBox.SelectedIndex = 0;
                    else
                        listBox.SelectIndex(listBox.SelectedIndex + 1);

                    e.Handled = true;
                }
                else if (_inlineCompletion && (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right))
                    _completionWindow.Close();
            };

            if (_inlineCompletion)
            {
                _completionWindow.CompletionList.ListBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) => {
                    if (e.AddedItems.Count > 0)
                    {
                        var item = e.AddedItems[0] as CompletionData;
                        if (item.NoCompletionFound)
                            return;

                        CommandLine.TextArea.Document.Replace(_completionStartOffset, CommandLine.CaretOffset - _completionStartOffset, item.Text);
                    }
                };

                _completionWindow.CompletionList.IsFiltering = false;
            }

            IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;

            if (completion == null || completion.CompletionMatches.Count == 0)
            {
                data.Add(new CompletionData());
            }
            else
            {
                _completionStartOffset = completion.ReplacementIndex;
                _completionEndOffset = CommandLine.CaretOffset;

                foreach (var result in completion.CompletionMatches)
                    data.Add(new CompletionData(result, _completionStartOffset, _completionEndOffset));

                if (_inlineCompletion)
                {
                    var offset = completion.ReplacementIndex;
                    var length = CommandLine.CaretOffset - offset;
                    data.Add(new CompletionData(CommandLine.Document.GetText(offset, length), "<pre-completion text>", _completionStartOffset, _completionEndOffset));

                    _completionWindow.StartOffset = _completionStartOffset;
                }

                if (byUserAction)
                    _completionWindow.CompletionList.SelectedItem = data[0];
            }

            _completionWindow.Closed += (object sender, EventArgs e) => {
                _completionWindow = null;
            };

            _completionWindow.Show();
        }

        void CommandLine_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (_inlineCompletion)
                {
                    _completionWindow.Close();
                }
                else
                {
                    if (!char.IsLetterOrDigit(e.Text[0]))
                    {
                        // Whenever a non-letter is typed while the completion window is open,
                        // insert the currently selected element.
                        _completionWindow.CompletionList.RequestInsertion(e);
                    }
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        // Helper methods
        // https://stackoverflow.com/questions/12787513/how-to-transfer-data-from-richtextbox-to-another-richtextbox-wpf-c-sharp
        // These methods are not used in the current version. Kept for future use.
        /*
                private static string GetRtfStringFromFlowDocument(FlowDocument doc)
                {
                    TextRange textRange = new TextRange(doc.ContentStart, doc.ContentEnd);
                    MemoryStream ms = new MemoryStream();
                    textRange.Save(ms, DataFormats.Rtf);

                    return Encoding.Default.GetString(ms.ToArray());
                }

                private static FlowDocument CreateFlowDocument(string richTextString)
                {
                    FlowDocument fd = new FlowDocument();
                    MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(richTextString));
                    TextRange textRange = new TextRange(fd.ContentStart, fd.ContentEnd);
                    textRange.Load(ms, DataFormats.Rtf);

                    return fd;
                }

                public static FlowDocument CopyFlowDocument(FlowDocument doc)
                {
                    var s = GetRtfStringFromFlowDocument(doc);
                    return CreateFlowDocument(s);
                }

                public static void SwapControlContent(Roundtrip r1, Roundtrip r2)
                {
                    var outputDoc1 = CopyFlowDocument(r1.Output.Document);
                    var outputDoc2 = CopyFlowDocument(r2.Output.Document);
                    r1.Output.Document = outputDoc2;
                    r2.Output.Document = outputDoc1;

                    var commandLineDoc1 = CopyFlowDocument(r1.CommandLine.Document);
                    var commandLineDoc2 = CopyFlowDocument(r2.CommandLine.Document);
                    r1.CommandLine.Document = commandLineDoc2;
                    r2.CommandLine.Document = commandLineDoc1;
                }
                */
    }
}
