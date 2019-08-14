using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
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
using Horker.Notebook.ViewModels;

namespace Horker.Notebook.Views
{
    public partial class Session : UserControl
    {
        public SessionViewModel ViewModel { get; set; }

        public Session()
        {
            InitializeComponent();
            DataContext = ViewModel = new SessionViewModel(this);
        }

        public Roundtrip GetActiveRoundtrip()
        {
            var rtb = Keyboard.FocusedElement as RichTextBox;
            var grid1 = rtb?.Parent as Grid;
            var grid2 = grid1?.Parent as Grid;
            var r = grid2?.Parent as Roundtrip;
            return r;
        }

        public void MoveToPreviousRoundtrip()
        {
            var r = GetActiveRoundtrip();
            if (r == null)
                return;

            var index = ViewModel.Items.IndexOf(r.ViewModel);
            if (index <= 0)
                return;

            ViewModel.Items[index - 1].Focus();
        }

        public void MoveToNextRoundtrip()
        {
            var r = GetActiveRoundtrip();
            if (r == null)
                return;

            var index = ViewModel.Items.IndexOf(r.ViewModel);
            if (index == -1 || index == ViewModel.Items.Count - 1)
                return;

            ViewModel.Items[index + 1].Focus();
        }

        public void ShowProgress()
        {
            ProgressBar.Visibility = Visibility.Visible;
        }

        public void HideProgress()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
        }
    }
}
