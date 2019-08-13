using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Horker.ViewModels;

namespace Horker.Views
{
    public partial class Roundtrip : UserControl
    {
        private RichTextBox _commandLineControl;
        private FlowDocumentScrollViewer _outputControl;

        public RichTextBox CommandLineControl => _commandLineControl;
        public FlowDocumentScrollViewer OutputControl => _outputControl;

        public RoundtripViewModel ViewModel
        {
            get => DataContext as RoundtripViewModel;
            set => DataContext = value;
        }

        public Roundtrip()
        {
            InitializeComponent();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.NotifyExecute();
        }

        private void RoundtripControl_Loaded(object sender, RoutedEventArgs e)
        {
            var control = sender as Roundtrip;

            _commandLineControl = control.FindName("CommandLine") as RichTextBox;
            _outputControl = control.FindName("Output") as FlowDocumentScrollViewer;
            ViewModel.Control = control;

            // Address RichTextBox's known limitation
            // (source: https://stackoverflow.com/questions/350863/wpf-richtextbox-with-no-width-set)

            _commandLineControl.Document.PageWidth = _commandLineControl.ActualWidth;
            _outputControl.Document.PageWidth = _commandLineControl.ActualWidth - 20;

            _commandLineControl.Focus();
        }
    }
}
