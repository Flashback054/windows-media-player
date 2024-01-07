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
        public string PlaylistName { get; set;}

        public BindingList<Media> MediaList { get; set;}

        public String CountMedia
        {
            get; set;
        } = "0 items";

        public Playlist()
        {
            PlaylistName = "";
            MediaList = new BindingList<Media>();
        }

        public void AddMediaFile(Media media)
        {
            MediaList.Add(media);
        }

        public string CountPlaylistItems()
        {
            return MediaList.Count.ToString() + " items";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
