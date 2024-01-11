using MediaPlayer.Models;
using System.Windows;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for AddPlaylist.xaml
    /// </summary>
    public partial class AddPlaylist : Window
    {

        public AddPlaylist()
        {
            InitializeComponent();
        }

        public Playlist NewPlaylist { get; set; }

        private void ClearTextBox_Click(object sender, RoutedEventArgs e)
        {
            PlaylistNameTextBox.Text = "";
        }

        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if(PlaylistNameTextBox.Text == "")
            {
                MessageBox.Show("Please enter a playlist name.");
                return;
            }

            NewPlaylist = new Playlist()
            {
                Name = PlaylistNameTextBox.Text,
            };

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
