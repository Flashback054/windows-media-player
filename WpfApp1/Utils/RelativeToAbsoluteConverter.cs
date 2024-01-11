using System;
using System.Globalization;
using System.Windows.Data;

namespace MediaPlayer
{
    public class RelativeToAbsolutePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var relativePath = (string)value;
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            var absolutePath = $"{folder}{relativePath}";

            return absolutePath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}