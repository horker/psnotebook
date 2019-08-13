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
using Horker.ViewModels;

namespace Horker.Views
{
    /// <summary>
    /// Notebook.xaml の相互作用ロジック
    /// </summary>
    public partial class Session : UserControl
    {
        public ObservableCollection<RoundtripViewModel> Items { get; private set; }

        public Session()
        {
            InitializeComponent();
            DataContext = Items = new ObservableCollection<RoundtripViewModel>();
        }
    }
}
