using System;
using System.Collections.Generic;
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
using Horker.ViewModels;

namespace NotebookApp
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Session_Loaded(object sender, RoutedEventArgs e)
        {
            var sessionControl = (Horker.Views.Session)sender;

            var sessionViewModel = new SessionViewModel(sessionControl);

            var thread = new Thread(() => {
                var session = new Horker.Models.Session(sessionViewModel);
                var exitCode = session.StartExecutionLoop();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "Notebook GUI thread";
            thread.Start();
        }
    }
}
