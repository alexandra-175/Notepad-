using System.Collections.ObjectModel;

namespace notepad__.Models
{
    public class DirectoryItem
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }

        public ObservableCollection<DirectoryItem> Items { get; set; } = new ObservableCollection<DirectoryItem>();
    }
}