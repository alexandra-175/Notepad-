using System;
using System.Globalization;
using System.Windows.Data;

namespace notepad__
{
    public class DirToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Dacă IsDirectory este true, returnăm iconița de folder, altfel cea de fișier
            return (bool)value ? "📁" : "📄";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}