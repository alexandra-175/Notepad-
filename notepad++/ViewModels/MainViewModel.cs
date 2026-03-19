using Microsoft.Win32;
using notepad__.Commands;
using notepad__.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace notepad__.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<DirectoryItem> DirectoryTree { get; set; } = new ObservableCollection<DirectoryItem>();
        public ObservableCollection<FileModel> Tabs { get; set; } = new ObservableCollection<FileModel>();

        private FileModel? _selectedTab;
        public FileModel? SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(); }
        }

        public string? FolderToCopyPath { get; set; }

        public RelayCommand NewFileCommand { get; }
        public RelayCommand OpenFileCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand CloseTabCommand { get; }
        public RelayCommand CloseAllCommand { get; }
        public bool SearchInAllTabs { get; set; } = false;

        public MainViewModel()
        {
            NewFileCommand = new RelayCommand(_ => ExecuteNewFile());
            OpenFileCommand = new RelayCommand(_ => ExecuteOpenFile());
            SaveCommand = new RelayCommand(_ => ExecuteSave(SelectedTab));
            SaveAsCommand = new RelayCommand(_ => ExecuteSaveAs(SelectedTab));
            CloseTabCommand = new RelayCommand(o => ExecuteCloseTab(o as FileModel));
            CloseAllCommand = new RelayCommand(_ => ExecuteCloseAll());

            ExecuteNewFile();
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            LoadDirectory(desktop);
        }

        public void ExecuteNewFile()
        {
            var file = new FileModel { FileName = $"File {Tabs.Count + 1}", Content = string.Empty, IsDirty = false };
            Tabs.Add(file);
            SelectedTab = file;
        }

        public void ExecuteOpenFile()
        {
            var ofd = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (ofd.ShowDialog() == true) OpenSpecificFile(ofd.FileName);
        }

        public bool ExecuteSave(FileModel? file)
        {
            if (file == null) return false;
            if (string.IsNullOrEmpty(file.FilePath)) return ExecuteSaveAs(file);
            File.WriteAllText(file.FilePath, file.Content);
            file.IsDirty = false;
            return true;
        }

        public bool ExecuteSaveAs(FileModel? file)
        {
            if (file == null) return false;
            var sfd = new SaveFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, file.Content);
                file.FilePath = sfd.FileName;
                file.FileName = Path.GetFileName(sfd.FileName);
                file.IsDirty = false;
                return true;
            }
            return false;
        }

        public void ExecuteCloseTab(FileModel? file)
        {
            if (file == null) return;
            if (file.IsDirty)
            {
                var res = MessageBox.Show($"Salvați modificările pentru {file.FileName}?", "Notepad++", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Yes) { if (!ExecuteSave(file)) return; }
                else if (res == MessageBoxResult.Cancel) return;
            }
            Tabs.Remove(file);
        }

        private void ExecuteCloseAll()
        {
            foreach (var tab in Tabs.ToList()) ExecuteCloseTab(tab);
        }

        public void OpenSpecificFile(string path)
        {
            var existing = Tabs.FirstOrDefault(t => t.FilePath == path);
            if (existing != null) { SelectedTab = existing; return; }
            try
            {
                string content = File.ReadAllText(path);
                var file = new FileModel { FileName = Path.GetFileName(path), FilePath = path, Content = content, IsDirty = false };
                Tabs.Add(file);
                SelectedTab = file;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public void PerformSearchReplace(string searchText, string replaceText, bool allTabs, bool isReplaceAll)
        {
            var targets = allTabs ? Tabs.ToList() : new List<FileModel?> { SelectedTab };

            bool foundSomething = false;

            foreach (var tab in targets.Where(t => t != null))
            {
                var current = tab!;

                if (isReplaceAll)
                {
                    int count = 0;
                    int index = 0;

                    while ((index = current.Content.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
                    {
                        current.Content =
                            current.Content.Substring(0, index) +
                            replaceText +
                            current.Content.Substring(index + searchText.Length);

                        index += replaceText.Length;
                        count++;
                    }

                    if (count > 0)
                    {
                        foundSomething = true;
                        MessageBox.Show($"{count} apariții înlocuite în {current.FileName}");
                    }
                }
                else if (!string.IsNullOrEmpty(replaceText))
                {
                    int index = current.Content.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);

                    if (index >= 0)
                    {
                        current.Content =
                            current.Content.Substring(0, index) +
                            replaceText +
                            current.Content.Substring(index + searchText.Length);

                        foundSomething = true;
                        MessageBox.Show($"Text înlocuit în {current.FileName} la poziția {index}");
                    }
                }
                else
                {
                    int index = 0;
                    int count = 0;

                    while ((index = current.Content.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) != -1)
                    {
                        count++;
                        index += searchText.Length;
                    }

                    if (count > 0)
                    {
                        foundSomething = true;
                        MessageBox.Show($"{count} apariții găsite în {current.FileName}");
                    }
                }
            }

            if (!foundSomething)
            {
                MessageBox.Show("Textul nu a fost găsit.");
            }
        }

        public void LoadDirectory(string rootPath)
        {
            DirectoryTree.Clear();
            var rootItem = new DirectoryItem { Name = Path.GetFileName(rootPath), Path = rootPath, IsDirectory = true };
            rootItem.Items.Add(new DirectoryItem { Name = "Loading..." });
            DirectoryTree.Add(rootItem);
        }

        public void PopulateSubItem(DirectoryItem parent)
        {
            if (parent == null || string.IsNullOrEmpty(parent.Path)) return;

            parent.Items.Clear();
            try
            {
                foreach (var dir in Directory.GetDirectories(parent.Path))
                {
                    var item = new DirectoryItem { Name = Path.GetFileName(dir), Path = dir, IsDirectory = true };
                    item.Items.Add(new DirectoryItem { Name = "Loading..." });
                    parent.Items.Add(item);
                }
                foreach (var file in Directory.GetFiles(parent.Path))
                {
                    parent.Items.Add(new DirectoryItem { Name = Path.GetFileName(file), Path = file, IsDirectory = false });
                }
            }
            catch { }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}