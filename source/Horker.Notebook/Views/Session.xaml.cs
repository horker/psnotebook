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

        public ObservableCollection<RoundtripViewModel> Items { get; private set; }

        private Roundtrip GetActiveRoundtrip()
        {
            var rtb = Keyboard.FocusedElement as RichTextBox;
            var grid = rtb?.Parent as Grid;
            var r = grid?.Parent as Roundtrip;
            return r;
        }

        public Session()
        {
            InitializeComponent();
            DataContext = Items = new ObservableCollection<RoundtripViewModel>();
        }

        public void MoveToPreviousRoundtrip()
        {
            var r = GetActiveRoundtrip();
            if (r == null)
                return;

            var index = Items.IndexOf(r.ViewModel);
            if (index <= 0)
                return;

            Items[index - 1].Focus();
        }

        public void MoveToNextRoundtrip()
        {
            var r = GetActiveRoundtrip();
            if (r == null)
                return;

            var index = Items.IndexOf(r.ViewModel);
            if (index == -1 || index == Items.Count - 1)
                return;

            Items[index + 1].Focus();
        }
    }
}
