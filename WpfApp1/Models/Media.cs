using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Models
{
    public class Media(string? name, Uri? uri, string? thumbnail, TimeSpan? lastSeekPosition) : INotifyPropertyChanged
    {
        public string? Name { get; set; } = name;
        public Uri? Uri { get; set; } = uri;
        public string? Thumbnail { get; set; } = thumbnail ?? "/Assets/Icons/default.png";
        public TimeSpan LastSeekPosition { get; set; } = lastSeekPosition ?? TimeSpan.Zero;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
