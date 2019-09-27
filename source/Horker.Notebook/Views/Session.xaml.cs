using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
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
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using Microsoft.Win32;

namespace Horker.Notebook.Views
{
    public partial class Session : UserControl
    {
        public SessionViewModel ViewModel { get; set; }

        public Session()
        {
            InitializeComponent();
            DataContext = ViewModel = new SessionViewModel(this);

            RecentFileList.UseXmlPersister();
            RecentFileList.MaxNumberOfFiles = 5;
            RecentFileList.MenuClick += (s, e) => {
                ViewModel.EnqueueLoadSessionRequest(e.Filepath, false);
            };
        }

        public Roundtrip GetActiveRoundtrip()
        {
            var textArea = Keyboard.FocusedElement as TextArea;
            var scrollView = textArea?.Parent as ScrollViewer;
            var textEditor = scrollView?.TemplatedParent as TextEditor;
            var border = textEditor?.Parent as Border;
            var grid1 = border?.Parent as Grid;
            var grid2 = grid1?.Parent as Grid;
            var r = grid2?.Parent as Roundtrip;

            return r;
        }

        // Add/Remove roundtrips

        public void AddRoundtrip(RoundtripViewModel r, int position)
        {
            if (position == -1)
                StackPanel.Children.Add(new Roundtrip(this, r));
            else
                StackPanel.Children.Insert(position, new Roundtrip(this, r));
        }

        public void MoveToPreviousRoundtrip()
        {
            var r = GetActiveRoundtrip();
            if (r == null)
                return;

            var index = StackPanel.Children.IndexOf(r);
            if (index <= 0)
                return;

            (StackPanel.Children[index - 1] as Roundtrip).CommandLine.Focus();
        }

        public void MoveToNextRoundtrip()
        {
            var r = GetActiveRoundtrip();
            if (r == null)
                return;

            var index = StackPanel.Children.IndexOf(r);

            if (index == -1)
                return;

            if (index == StackPanel.Children.Count - 1)
            {
                ViewModel.InsertRoundtrip(r.ViewModel);
                return;
            }

            (StackPanel.Children[index + 1] as Roundtrip).CommandLine.Focus();
        }

        public void MoveRoundtrip(Roundtrip r, int newIndex)
        {
            ViewModel.MoveRoundtrip(r.ViewModel, newIndex);
        }

        public void ShowProgress()
        {
            ProgressBar.Visibility = Visibility.Visible;
        }

        public void HideProgress()
        {
            ProgressBar.Visibility = Visibility.Collapsed;
        }

        // Commands

        private bool Confirm(string message, string title)
        {
            if (ViewModel.IsTextChanged)
            {
                var ok = MessageBox.Show(message, title, MessageBoxButton.YesNo);
                if (ok == MessageBoxResult.No)
                    return false;
            }

            return true;
        }

        private void NewCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Confirm("Seesion is changed and not saved yet.\nAre you sure to continue?", "New session"))
                return;

            ViewModel.Clear();
            ViewModel.IsTextChanged = false;
        }

        private void SaveCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ViewModel.HasFileName())
                SaveAsCommand_Execute(sender, e);
            else
                ViewModel.SaveSession();
        }

        private static readonly string _filterString = "PowerShell Notebook script (*.nb.ps1)|*.nb.ps1|PowerShell script (*.ps1)|*.ps1|All files (*.*)|*.*";

        private void SaveAsCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Title = "Save session",
                Filter = _filterString,
                InitialDirectory = Directory.GetCurrentDirectory(),
                CheckFileExists = false
            };

            if (openFileDialog.ShowDialog() == true)
                ViewModel.SaveSession(openFileDialog.FileName);
        }

        private void LoadCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Title = "Load session",
                Filter = _filterString,
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (openFileDialog.ShowDialog() == true)
                ViewModel.EnqueueLoadSessionRequest(openFileDialog.FileName, false);
        }

        private void ReloadCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Confirm("Seesion is changed and not saved yet.\nAre you sure to continue?", "Reload session"))
                return;

            ViewModel.NotifyRestart(ViewModel.FileName, false);
        }

        private void ReloadAndRunCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Confirm("Seesion is changed and not saved yet.\nAre you sure to continue?", "Reload and run session"))
                return;

            ViewModel.NotifyRestart(ViewModel.FileName, true);
        }

        private void RestartCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Confirm("Seesion is changed and not saved yet.\nAre you sure to continue?", "Restart"))
                return;

            ViewModel.NotifyRestart(null, false);
        }

        private void ExitCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (!Confirm("Seesion is changed and not saved yet.\nAre you sure to continue?", "Exit"))
                return;

            ViewModel.NotifyExit();
        }

        private void RunCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var r = GetActiveRoundtrip();
            r.ViewModel.NotifyExecute(false);
        }

        private void RunAllCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.NotifyExecuteAll();
        }

        private void CancelCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.NotifyCancel();
        }

        private void EditorModeByDefaultCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            // Nothing to do
        }

        private void AboutCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("PowerShell Notebook v0.2.2\r\n\r\nIcons made by Smashicons from https://www.flaticon.com/", "About", MessageBoxButton.OK);
        }
    }
}
