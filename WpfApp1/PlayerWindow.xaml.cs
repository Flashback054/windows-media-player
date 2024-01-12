using MediaPlayer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for PlayerWindow.xaml
    /// </summary>
    public partial class PlayerWindow : Window
    {
        bool isPlaying = false;
        bool isShuffling = false;
        int selectedIndex = 0;
        public int selectedPlaylistIndex = (int)((MainWindow)Application.Current.MainWindow).currentPlaylistIndex;

        private DispatcherTimer idleTimer;

        BindingList<Media> playlist
        {
            get
            {
                return ((MainWindow)Application.Current.MainWindow).AllPlaylist[selectedPlaylistIndex].MediaList;
            }
        }

        string RecentMediaFileName = "recent_media.txt";
        List<int> randomizedIndexes;
        int currentRandomIndex;

        public PlayerWindow(int selectedIndex)
        {
            InitializeComponent();

            this.selectedIndex = selectedIndex;

            // #region Setup slider timer
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
            // #endregion

            // #region Setup idle timer
            // Initialize the timer
            idleTimer = new DispatcherTimer();
            idleTimer.Interval = TimeSpan.FromSeconds(3);
            idleTimer.Tick += IdleTimer_Tick;
            PreviewMouseMove += MainWindow_PreviewMouseMove;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            // Start the timer
            idleTimer.Start();

            // Setup the global hotkey system
            HotkeysManager.SetupSystemHook();
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.Space, PlayOrPause));
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.Left, PlayPrevious));
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.Right, PlayNext));

            Closing += Window_Closing;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            idleTimer.Stop();
            idleTimer.Start();

            playlistNameElement.Visibility = Visibility.Visible;
            playerControlElement.Visibility = Visibility.Visible;
        }

        private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            idleTimer.Stop();
            idleTimer.Start();

            playlistNameElement.Visibility = Visibility.Visible;
            playerControlElement.Visibility = Visibility.Visible;
        }

        private void IdleTimer_Tick(object? sender, EventArgs e)
        {
            playlistNameElement.Visibility = Visibility.Hidden;
            playerControlElement.Visibility = Visibility.Hidden;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Play();

            // Setup the playlist
            playlistNameTextBlock.Text = ((MainWindow)Application.Current.MainWindow).AllPlaylist[selectedPlaylistIndex].Name ?? "";
        }

        private void AddToRecentMedia(string mediaPath)
        {
            string recentFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Recent";
            string recentMediaFilePath = System.IO.Path.Combine(recentFolder, RecentMediaFileName);

            try
            {
                using (StreamWriter sw = new StreamWriter(recentMediaFilePath, true))
                {
                    sw.WriteLine(mediaPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while writing to recent media file: " + ex.Message);
            }
        }

        private void Play()
        {
            this.DataContext = playlist[selectedIndex];
            Player.Source = playlist[selectedIndex].Uri;

            Player.Play();
            isPlaying = true;
            playButtonIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;

            if ((Player.Source != null) && (Player.NaturalDuration.HasTimeSpan))
            {
                timerProgressCurrent.Text = TotalSecondsToFormattedTimeConverter.Convert(Player.Position.TotalSeconds);
                timerProgressMax.Text = TotalSecondsToFormattedTimeConverter.Convert(Player.NaturalDuration.TimeSpan.TotalSeconds);
            }

            string path = playlist[selectedIndex].Uri.LocalPath;
            AddToRecentMedia(path);

            ((MainWindow)Application.Current.MainWindow).ReadRecentFiles();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((Player.Source != null) && (Player.NaturalDuration.HasTimeSpan) && isPlaying)
            {
                timerSlider.Value = Player.Position.TotalSeconds;
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            PlayOrPause();
        }

        private void PlayOrPause()
        {
            if (!isPlaying)
            {
                Player.Play();
                playButtonIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
                isPlaying = true;
            }
            else
            {
                Player.Pause();
                playButtonIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
                isPlaying = false;
            }
        }
        private void PlayPrevious()
        {
            if (!isShuffling)
            {
                if (selectedIndex == 0)
                {
                    selectedIndex = playlist.Count - 1;
                }
                else
                {
                    selectedIndex--;
                }
            }
            else
            {
                if (currentRandomIndex == 0)
                {
                    currentRandomIndex = randomizedIndexes.Count - 1;
                }
                else
                {
                    currentRandomIndex--;
                }

                selectedIndex = randomizedIndexes[currentRandomIndex];
            }

            this.Play();
        }
        private void PlayNext()
        {
            if (!isShuffling)
            {
                if (selectedIndex == playlist.Count - 1)
                {
                    selectedIndex = 0;
                }
                else
                {
                    selectedIndex++;
                }
            }
            else
            {
                if (currentRandomIndex == randomizedIndexes.Count - 1)
                {
                    currentRandomIndex = 0;
                }
                else
                {
                    currentRandomIndex++;
                }

                selectedIndex = randomizedIndexes[currentRandomIndex];
            }

            this.Play();
        }

        private void Shuffle()
        {
            if (!isShuffling)
            {
                isShuffling = true;
                shuffleButtonIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ShuffleVariant;

                // Randomize the indexes using Fisher-Yates shuffle algorithm
                // Setup backup playlist indexes
                randomizedIndexes = new List<int>();
                currentRandomIndex = selectedIndex;
                for (int i = 0; i < playlist.Count; i++)
                {
                    randomizedIndexes.Add(i);
                }

                Random random = new Random();
                for (int i = playlist.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);


                    if (i != selectedIndex && j != selectedIndex)
                    {
                        int temp = randomizedIndexes[i];
                        randomizedIndexes[i] = randomizedIndexes[j];
                        randomizedIndexes[j] = temp;
                    }
                }
            }
            else
            {
                isShuffling = false;
                shuffleButtonIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.ShuffleDisabled;

                randomizedIndexes = new List<int>();
                for (int i = 0; i < playlist.Count; i++)
                {
                    randomizedIndexes.Add(i);
                }
            }
        }

        private void timerSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            Player.Pause();
            isPlaying = false;
        }

        private void timerSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Player.Position = TimeSpan.FromSeconds(timerSlider.Value);
            Player.Play();
            isPlaying = true;
        }

        private void timerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Position = TimeSpan.FromSeconds(timerSlider.Value);
            timerProgressCurrent.Text = TotalSecondsToFormattedTimeConverter.Convert(Player.Position.TotalSeconds);

            playlist[selectedIndex].LastSeekPosition = Player.Position;
        }

        private void skipPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            PlayPrevious();
        }

        private void skipNextButton_Click(object sender, RoutedEventArgs e)
        {
            PlayNext();
        }

        private void shuffleButton_Click(object sender, RoutedEventArgs e)
        {
            Shuffle();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            HotkeysManager.ShutdownSystemHook();
        }

        private void volumeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Player.IsMuted == false)
            {
                Player.IsMuted = true;
                volumeSlider.Value = 0;
            }
            else
            {
                Player.IsMuted = false;
                volumeSlider.Value = 50;
            }
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (volumeSlider.Value == 0)
            {
                Player.IsMuted = true;
                volumeBtnIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeOff;
            }
            else
            {
                Player.IsMuted = false;
                Player.Volume = (double)volumeSlider.Value / 100.0;
                volumeBtnIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeHigh;
            }
        }

        private void button_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Player_MediaOpened(object sender, RoutedEventArgs e)
        {
            timerSlider.Minimum = 0;
            timerSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
            timerProgressMax.Text = TotalSecondsToFormattedTimeConverter.Convert(Player.NaturalDuration.TimeSpan.TotalSeconds);

            if (Player.NaturalVideoHeight > 0 && Player.NaturalVideoWidth > 0)
            {
                CoverArt.Source = null;
                CoverArt.Visibility = Visibility.Hidden;
                if (playlist[selectedIndex].LastSeekPosition != TimeSpan.Zero)
                {
                    Player.Position = playlist[selectedIndex].LastSeekPosition; ;
                }
            }
            else
            {
                CoverArt.Source = playlist[selectedIndex].Thumbnail;
                CoverArt.Visibility = Visibility.Visible;
                Player.Position = TimeSpan.Zero;
            }
        }

        private void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // Show error message
            MessageBox.Show("Cannot play media file: " + e.ErrorException.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Skip to next media file
            PlayNext();
        }
    }
}
