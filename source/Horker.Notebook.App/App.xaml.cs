using Horker.Notebook.Cmdlets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Horker.Notebook
{
    public partial class App : Application
    {
        public static Models.Session Session { get; private set; }

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

            var homePath = Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH");
            Directory.SetCurrentDirectory(homePath);

            Session = new Models.Session();
            OutNotebookInernal.Session = Session;

            var assem = Assembly.GetEntryAssembly();
            var path = Path.GetDirectoryName(assem.Location);
            var startupFile = Path.Combine(path, "NotebookApp_Startup.ps1");

            if (File.Exists(startupFile))
                Session.ExecuteConfigurationScript(startupFile, Session.Configuration);
        }
    }
}
