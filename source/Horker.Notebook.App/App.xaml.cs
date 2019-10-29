using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Horker.Notebook
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length >= 1)
            {
                var i = 0;
                if (e.Args[i].ToLower() == "-run")
                {
                    Notebook.MainWindow.RunOnStartup = true;
                    ++i;
                }

                Notebook.MainWindow.FileToLoadOnStartup = e.Args[i];
            }
        }
    }
}
