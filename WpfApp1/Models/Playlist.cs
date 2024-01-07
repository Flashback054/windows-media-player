using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer.Models
{
    public class Playlist : INotifyPropertyChanged
    {
        public string PlaylistName;

        private ObservableCollection<Media> MediaList;

        public Playlist()
        {
            PlaylistName = "";
            MediaList = new ObservableCollection<Media>();
        }

        public void AddMediaFile(Media media)
        {
            MediaList.Add(media);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
