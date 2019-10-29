using System;
using System.Collections.Generic;
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

namespace Horker.Notebook
{
    public partial class MainWindow : Window
    {
        private int _exitCode;
        private bool _sessionEnded;

        public static string FileToLoadOnStartup { get; set; }
        public static bool RunOnStartup { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Session_Loaded(object sender, RoutedEventArgs e)
        {
            var homePath = Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH");
            Directory.SetCurrentDirectory(homePath);

            var sessionControl = (Views.Session)sender;
            var sessionViewModel = sessionControl.ViewModel;

            Models.CurrentState.Dispatcher = Dispatcher;

            var thread = new Thread(() => {
                var session = new Models.Session(sessionViewModel);

                if (!string.IsNullOrEmpty(FileToLoadOnStartup))
                {
                    session.EnqueueLoadSessionRequest(FileToLoadOnStartup, RunOnStartup);
                }

                _exitCode = session.StartExecutionLoop();
                _sessionEnded = true;

                Dispatcher.Invoke(() => {
                    Close();
                });
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "Notebook Execution loop thread";
            thread.Start();

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(_exitCode);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_sessionEnded && Session.ViewModel.IsTextChanged)
            {
                var result = MessageBox.Show("Session is changed. Are you sure to exit?", "PowrShell Notebook", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel)
                    e.Cancel = true;
            }
        }
    }
}
