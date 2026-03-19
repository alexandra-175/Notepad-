using notepad__.Models;
using notepad__.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace notepad__
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void TreeView_Expanded(object sender, RoutedEventArgs e)
        {
            if ((e.OriginalSource as TreeViewItem)?.Header is DirectoryItem item)
            {
                if (item.IsDirectory && item.Items.Count == 1 && item.Items[0].Name == "Loading...")
                    (DataContext as MainViewModel)?.PopulateSubItem(item);
            }
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as TreeViewItem)?.Header is DirectoryItem item && !item.IsDirectory)
            {
                (DataContext as MainViewModel)?.OpenSpecificFile(item.Path);
                e.Handled = true;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Window aboutWindow = new Window
            {
                Title = "About",
                Height = 200,
                Width = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            TextBlock tb = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10)
            };

            // Creezi un hyperlink
            tb.Inlines.Add("Student: Neagu Alexandra\nGrupa: 10LF342\nEmail: ");
            Hyperlink link = new Hyperlink()
            {
                NavigateUri = new Uri("mailto:alexandra-i.neagu@student.unitbv.ro")
            };
            link.Inlines.Add("alexandra-i.neagu@student.unitbv.ro");
            link.RequestNavigate += (s, args) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(args.Uri.AbsoluteUri) { UseShellExecute = true });

            tb.Inlines.Add(link);

            aboutWindow.Content = tb;
            aboutWindow.ShowDialog();
        }

        private void OpenSearch_Click(object sender, RoutedEventArgs e)
        {
            string search = Microsoft.VisualBasic.Interaction.InputBox("Introduceți textul căutat:", "Find", "");
            if (!string.IsNullOrEmpty(search) && DataContext is MainViewModel vm)
                vm.PerformSearchReplace(search, "", vm.SearchInAllTabs, false);
        }

        private void OpenReplace_Click(object sender, RoutedEventArgs e)
        {
            string find = Microsoft.VisualBasic.Interaction.InputBox("Find:", "Replace", "");
            string replace = Microsoft.VisualBasic.Interaction.InputBox("Replace with:", "Replace", "");
            if (!string.IsNullOrEmpty(find) && DataContext is MainViewModel vm)
                vm.PerformSearchReplace(find, replace, vm.SearchInAllTabs, false);
        }

        private void OpenReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            string find = Microsoft.VisualBasic.Interaction.InputBox("Find:", "Replace All", "");
            string replace = Microsoft.VisualBasic.Interaction.InputBox("Replace with:", "Replace All", "");
            if (!string.IsNullOrEmpty(find) && DataContext is MainViewModel vm)
                vm.PerformSearchReplace(find, replace, vm.SearchInAllTabs, true);
        }

        private void NewFileFromTree_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is DirectoryItem item && item.IsDirectory)
            {
                string path = Path.Combine(item.Path, "NewFile.txt");
                File.WriteAllText(path, "");
                (DataContext as MainViewModel)?.PopulateSubItem(item);
            }
        }

        private void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is DirectoryItem item)
                Clipboard.SetText(item.Path);
        }

        private void CopyFolder_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is DirectoryItem item && item.IsDirectory)
                (DataContext as MainViewModel).FolderToCopyPath = item.Path;
        }

        private void PasteFolder_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if ((sender as MenuItem)?.DataContext is DirectoryItem target && !string.IsNullOrEmpty(vm?.FolderToCopyPath))
            {
                string source = vm.FolderToCopyPath;
                string dest = Path.Combine(target.Path, Path.GetFileName(source));
                DirectoryCopy(source, dest);
                vm.PopulateSubItem(target);
            }
        }

        private static void DirectoryCopy(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);

            foreach (var dir in Directory.GetDirectories(sourceDir))
                DirectoryCopy(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }

        private void SelectedTab_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm) vm.SearchInAllTabs = false;
            SelectedTabMenu.IsChecked = true;
            AllTabsMenu.IsChecked = false;
        }

        private void AllTabs_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm) vm.SearchInAllTabs = true;
            AllTabsMenu.IsChecked = true;
            SelectedTabMenu.IsChecked = false;
        }

        private void StandardView_Click(object sender, RoutedEventArgs e)
        {
            ViewExplorer.IsChecked = true;
            ExplorerColumn.Width = new GridLength(250);
        }

        private void ViewExplorer_Click(object sender, RoutedEventArgs e)
        {
            ExplorerColumn.Width = ViewExplorer.IsChecked ? new GridLength(250) : new GridLength(0);
        }
    }
}