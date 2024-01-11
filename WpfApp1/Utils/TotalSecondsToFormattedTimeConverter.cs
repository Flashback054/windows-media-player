using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MediaPlayer
{
    public class TotalSecondsToFormattedTimeConverter
    {
        public static string Convert(object value)
        {
            // Convert MediaElement.Position.TotalSeconds to a formatted time string
            TimeSpan time = TimeSpan.FromSeconds((double)value);
            if (time.Hours > 0)
            {
                return time.ToString(@"hh\:mm\:ss");
            }
            else
            {
                return time.ToString(@"mm\:ss");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
