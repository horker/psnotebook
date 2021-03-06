using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Horker.Notebook.Models;
using Horker.Notebook.ViewModels;

namespace Horker.Notebook.Models
{
    public class ApplicationInstance
    {
        public static readonly string Version = "0.4.0";

        public static Session Session { get; set; }
        public static SessionViewModel SessionViewModel { get; set; }
        public static Dispatcher Dispatcher { get; set; }

/*
        public static int Start()
        {
            Window window = null;
            Views.Session sessionControl = null;

            var e = new ManualResetEvent(false);

            var thread = new Thread(() => {
                sessionControl = new Views.Session();
                e.Set();

                window = new Window()
                {
                    Title = "PowerShell Notebook",
                    Content = sessionControl,
                    Width = Configuration.WindowWidth
                };

                CurrentState.Dispatcher = window.Dispatcher;

                try
                {
                    window.ShowDialog();
                }
                catch (OperationCanceledException)
                {
                    // pass
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "Notebook GUI thread";
            thread.Start();

            e.WaitOne();

            var sessionViewModel = sessionControl.ViewModel;

            var session = new Models.Session();
            session.InitializeViewModel(sessionViewModel);

            var exitCode = session.StartExecutionLoop();

            window.Dispatcher.Invoke(() => {
                window.Close();
            });

            return exitCode;
        }
*/
        public static void StartNotebookProcess(string fileName, bool run)
        {
            var executablePath = typeof(ApplicationInstance).Assembly.Location;
            executablePath = executablePath.Substring(0, executablePath.LastIndexOf("\\"));
            executablePath = Path.Combine(executablePath, "NotebookApp.exe");

            var arguments = fileName;
            if (run)
                arguments = "-Run " + fileName;

            Process.Start(executablePath, arguments);
        }
    }
}
