using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private RichTextBox _commandLineControl;
        private FlowDocumentScrollViewer _outputControl;

        public static readonly DependencyProperty ContainerProperty =
            DependencyProperty.Register("Container", typeof(Session), typeof(Roundtrip));

        public Session Container
        {
            get => (Session)GetValue(ContainerProperty);
            set => SetValue(ContainerProperty, value);
        }

        public RichTextBox CommandLineControl => _commandLineControl;
        public FlowDocumentScrollViewer OutputControl => _outputControl;

        private ScrollViewer _outputScrollViewer;

        public RoundtripViewModel ViewModel
        {
            get => DataContext as RoundtripViewModel;
            set => DataContext = value;
        }

        public Roundtrip()
        {
            InitializeComponent();
        }

        public void ScrollToBottom()
        {
            if (_outputScrollViewer != null)
            {
                _outputScrollViewer.ScrollToBottom();
                return;
            }

            _outputScrollViewer = _outputControl.Template.FindName("PART_ContentHost", _outputControl) as ScrollViewer;
            if (_outputScrollViewer != null)
                _outputScrollViewer.ScrollToBottom();
        }

        // Commands

        private void InvokeCommandLineCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.NotifyExecute(true);
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

        // Event handlers

        private void RoundtripControl_Loaded(object sender, RoutedEventArgs e)
        {
            var control = sender as Roundtrip;

            _commandLineControl = control.FindName("CommandLine") as RichTextBox;
            Debug.Assert(_commandLineControl != null);

            _outputControl = control.FindName("Output") as FlowDocumentScrollViewer;
            Debug.Assert(_outputControl != null);

            ViewModel.Control = control;

            // Address RichTextBox's known limitation that its document's width is not stretched automatically.
            // (source: https://stackoverflow.com/questions/350863/wpf-richtextbox-with-no-width-set)

            _commandLineControl.Document.PageWidth = Models.Configuration.ConsoleWidth;
            _outputControl.Document.PageWidth = Models.Configuration.ConsoleWidth;

            _commandLineControl.Focus();

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
    }
}
