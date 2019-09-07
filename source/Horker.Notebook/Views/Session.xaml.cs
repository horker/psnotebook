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
        }

        public Roundtrip GetActiveRoundtrip()
        {
            var rtb = Keyboard.FocusedElement as RichTextBox;
            var border = rtb?.Parent as Border;
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

        private void SaveCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (!ViewModel.HasFileName())
                SaveAsCommand_Execute(sender, e);
            else
                ViewModel.SaveSession();
        }

        private void SaveAsCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "PowerShell Notebook (*.nb.ps1)|*.nb.ps1",
                InitialDirectory = Directory.GetCurrentDirectory(),
                CheckFileExists = false
            };

            if (openFileDialog.ShowDialog() == true)
                ViewModel.SaveSession(openFileDialog.FileName);
        }

        private void LoadCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "PowerShell Notebook (*.nb.ps1)|*.nb.ps1",
                InitialDirectory = Directory.GetCurrentDirectory()
            };

            if (openFileDialog.ShowDialog() == true)
                ViewModel.EnqueueLoadSessionRequest(openFileDialog.FileName);
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
//            ViewModel.IsEditorModeByDefault = !ViewModel.IsEditorModeByDefault;
        }
    }
}
