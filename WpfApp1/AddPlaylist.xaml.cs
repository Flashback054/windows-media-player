using MediaPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

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
                PlaylistName = PlaylistNameTextBox.Text,
            };

            DialogResult = true;
        }
    }
}
