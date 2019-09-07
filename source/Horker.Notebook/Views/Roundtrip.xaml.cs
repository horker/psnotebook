using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Horker.Notebook.ViewModels;

namespace Horker.Notebook.Views
{
    public partial class Roundtrip : UserControl
    {
        private ScrollViewer _outputScrollViewer;

        private ScrollViewer OutputScrollViewer
        {
            get
            {
                // This getter can return null.

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

            Container = container;
            DataContext = viewModel;
        }

        public void ScrollToBottom()
        {
            OutputScrollViewer?.ScrollToBottom();
        }

        // Commands

        private void EnterCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.IsEditorMode)
                EditingCommands.EnterParagraphBreak.Execute(null, CommandLine);
            else
                ViewModel.NotifyExecute(true);
        }

        private void ShiftEnterCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.IsEditorMode)
                ViewModel.NotifyExecute(true);
            else
                EditingCommands.EnterParagraphBreak.Execute(null, CommandLine);
        }

        private void CtrlEnterCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.IsEditorMode)
                ViewModel.NotifyExecute(false);
            else
                EditingCommands.EnterParagraphBreak.Execute(null, CommandLine);
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
            CommandLine.Selection.Text = "    ";
            CommandLine.Selection.Select(CommandLine.Selection.End, CommandLine.Selection.End);
        }

        private void UpCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (EditingCommands.MoveUpByLine.CanExecute(null, CommandLine))
            {
                var position = CommandLine.CaretPosition;
                EditingCommands.MoveUpByLine.Execute(null, CommandLine);
                if (position.CompareTo(CommandLine.CaretPosition) == 0)
                    Container.MoveToPreviousRoundtrip();
            }
            else
            {
                Container.MoveToPreviousRoundtrip();
            }
        }

        private void DownCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (EditingCommands.MoveDownByLine.CanExecute(null, CommandLine))
            {
                var position = CommandLine.CaretPosition;
                EditingCommands.MoveDownByLine.Execute(null, CommandLine);
                if (position.CompareTo(CommandLine.CaretPosition) == 0)
                    Container.MoveToNextRoundtrip();
            }
            else
            {
                Container.MoveToNextRoundtrip();
            }
        }

        private void CursorToTopCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var position = CommandLine.CaretPosition = CommandLine.Document.ContentStart;
        }

        private void CursorToBottomCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var position = CommandLine.CaretPosition = CommandLine.Document.ContentEnd;
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

        private void EditorMode_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsEditorMode)
                CommandLineBorderRectangle.StrokeDashArray = new DoubleCollection(new double[] { 6, 4 });
            else
                CommandLineBorderRectangle.StrokeDashArray = null;

            CommandLine.Focus();
        }

        // Event handlers

        private void RoundtripControl_Loaded(object sender, RoutedEventArgs e)
        {
            var control = sender as Roundtrip;

            ViewModel.Control = control;

            // Address RichTextBox's known limitation that its document's width is not stretched automatically.
            // (source: https://stackoverflow.com/questions/350863/wpf-richtextbox-with-no-width-set)

            CommandLine.Document.PageWidth = Models.Configuration.ConsoleWidth;
            Output.Document.PageWidth = Models.Configuration.ConsoleWidth;

            CommandLine.Focus();

            ViewModel.CreatedEvent.Set();
        }

        private void CommandLine_GotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            CommandLine.Background = Brushes.FloralWhite;
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

        // Helper methods
        // https://stackoverflow.com/questions/12787513/how-to-transfer-data-from-richtextbox-to-another-richtextbox-wpf-c-sharp
        // These methods are not used in the current version. Kept for future use.

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

        private void CommandLine_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Container != null && Container.ViewModel != null)
                Container.ViewModel.IsTextChanged = true;
        }
    }
}
