using System;
using System.Collections.Generic;
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
    public class Startup
    {
        public static Dispatcher Dispatcher { get; set; }

        public static void Start()
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
                    Content = sessionControl
                };

                Dispatcher = window.Dispatcher;

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

            var sessionViewModel = new SessionViewModel(sessionControl);

            var session = new Models.Session(sessionViewModel);

            var exitCode = session.StartExecutionLoop();

            Environment.ExitCode = exitCode;

            window.Close();
        }
    }
}
