using MediaPlayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        bool isDragginSlider = false;
        int selectedIndex = 0;

        BindingList<Media> playlist = new BindingList<Media>()
        {
            new Media("71", new Uri("D:\\DownloadedCourses\\71.mp4"), null, null),
            new Media("Whistling", new Uri("D:\\DownloadedCourses\\whistling.mp3"), null, null),
            new Media("1", new Uri("D:\\DownloadedCourses\\1.mp3"), null, null),
            new Media("2", new Uri("D:\\DownloadedCourses\\2.mp3"), null, null),
            new Media("3", new Uri("D:\\DownloadedCourses\\3.mp3"), null, null),
            new Media("4", new Uri("D:\\DownloadedCourses\\4.mp3"), null, null),
            new Media("5", new Uri("D:\\DownloadedCourses\\5.mp3"), null, null),
            new Media("6", new Uri("D:\\DownloadedCourses\\6.mp3"), null, null)
        };
        List<int> randomizedIndexes;
        int currentRandomIndex;

        public PlayerWindow()
        {
            InitializeComponent();

            // Setup timer
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            // Setup the global hotkey system
            HotkeysManager.SetupSystemHook();
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.Space, PlayOrPause));
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.Left, PlayPrevious));
            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.Right, PlayNext));

            Closing += Window_Closing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Play();
        }

        private void Play()
        {

            this.DataContext = playlist[selectedIndex];
            Player.Source = playlist[selectedIndex].Uri;

            Player.Play();
            isPlaying = true;

            if ((Player.Source != null) && (Player.NaturalDuration.HasTimeSpan))
            {
                timerProgressCurrent.Text = TotalSecondsToFormattedTimeConverter.Convert(Player.Position.TotalSeconds);
                timerProgressMax.Text = TotalSecondsToFormattedTimeConverter.Convert(Player.NaturalDuration.TimeSpan.TotalSeconds);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((Player.Source != null) && (Player.NaturalDuration.HasTimeSpan) && isPlaying)
            {
                timerSlider.Minimum = 0;
                timerSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
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
            isDragginSlider = true;
        }

        private void timerSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Player.Position = TimeSpan.FromSeconds(timerSlider.Value);
            Player.Play();
            isPlaying = true;
            isDragginSlider = false;
        }

        private void timerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timerProgressCurrent.Text = TotalSecondsToFormattedTimeConverter.Convert(Player.Position.TotalSeconds);
            timerProgressMax.Text = TotalSecondsToFormattedTimeConverter.Convert(Player.NaturalDuration.TimeSpan.TotalSeconds);

            playlist[selectedIndex].LastSeekPosition = Player.Position;

            if (isDragginSlider)
            {
                // Modify the slider's thumb tooltip to show the current time
                var track = timerSlider.Template.FindName("PART_Track", timerSlider) as Track;
                var thumb = track?.Thumb;
                if (thumb != null)
                {
                    var toolTip = thumb.ToolTip as ToolTip;
                    if (toolTip != null)
                    {
                        toolTip.Content = TotalSecondsToFormattedTimeConverter.Convert(Player.Position.TotalSeconds);
                    }
                }

            }
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
    }
}
