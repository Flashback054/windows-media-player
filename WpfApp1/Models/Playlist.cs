using System.ComponentModel;

namespace MediaPlayer.Models
{
    public class Playlist : INotifyPropertyChanged
    {
        public string Name { get; set;}

        public BindingList<Media> MediaList { get; set;}

        public String CountMedia
        {
            get; set;
        } = "0 items";

        public Playlist()
        {
            Name = "";
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
