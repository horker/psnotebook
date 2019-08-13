using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Horker.ViewModels;

namespace Horker
{
    public class TestMain
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Window window = null;
            Views.Session sessionControl = null;

            var e = new ManualResetEvent(false);

            var thread = new Thread(() => {
                sessionControl = new Views.Session();
                e.Set();

                window = new Window();
                window.Title = "PowerShell Notebook";
                window.Content = sessionControl;
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

//            Console.WriteLine("PUsh enter key to exit");
//            Console.ReadLine();
        }
    }
}
