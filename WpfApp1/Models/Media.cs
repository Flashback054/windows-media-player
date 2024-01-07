using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Models
{
    public class Media : INotifyPropertyChanged
    {
        public string? Name { get; set; }
        public string? Uri { get; set; }
        public string? Thumbnail { get; set; }
        public TimeSpan LastSeekPosition { get; set; }

        public Media(string? name, string? uri)
        {
            Name = name;
            Uri = uri;
            Thumbnail = "/Assets/Icons/default.png";
            LastSeekPosition = TimeSpan.Zero;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
