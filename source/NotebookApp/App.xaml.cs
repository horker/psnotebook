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
                Notebook.MainWindow.FileToLoadOnStartup = e.Args[0];
        }
    }
}
