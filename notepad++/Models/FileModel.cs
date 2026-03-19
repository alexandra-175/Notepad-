using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace notepad__.Models
{
    public class FileModel : INotifyPropertyChanged
    {
        private string _fileName = string.Empty;
        private string _content = string.Empty;
        private string _filePath = string.Empty;
        private bool _isDirty;

        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
        }

        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    IsDirty = true;
                    OnPropertyChanged();
                }
            }
        }

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set { _isDirty = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
        }

        public string DisplayName => IsDirty ? FileName + "*" : FileName;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}