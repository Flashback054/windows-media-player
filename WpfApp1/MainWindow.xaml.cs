using MediaPlayer;
using MediaPlayer.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace MediaPlayer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public BindingList<Playlist> AllPlaylist
        {
            get; set;
        } = new BindingList<Playlist>();

        public BindingList<Media> SelectedPlaylist
        {
            get; set;
        } = new BindingList<Media>();

        public BindingList<Media> RecentFiles
        {
            get; set;
        } = new BindingList<Media>();


        private int currentPlaylistIndex = 0;

        public Visibility RecentVisibility { get; set; } = Visibility.Visible;
        public Visibility PlaylistVisibility { get; set; } = Visibility.Visible;
        public Visibility MediaVisibility { get; set; } = Visibility.Visible;

        private const string RecentMediaFileName = "recent_media.txt";

        private void createDirectory()
        {
            string playlistFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
            string recentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Recent";
            Directory.CreateDirectory(playlistFolder);
            Directory.CreateDirectory(recentFolder);
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            createDirectory();

            ReadPlaylist();
            ReadRecentFiles();
            CheckVisibility();

        }

        public void CheckVisibility()
        {
            if (RecentFiles.Count != 0)
            {
                RecentVisibility = Visibility.Collapsed;
            }
            else
            {
                RecentVisibility = Visibility.Visible;
            }

            if (AllPlaylist.Count != 0)
            {
                PlaylistVisibility = Visibility.Collapsed;
            }
            else
            {
                PlaylistVisibility = Visibility.Visible;
            }

            if (SelectedPlaylist.Count != 0)
            {
                MediaVisibility = Visibility.Collapsed;
            }
            else
            {
                MediaVisibility = Visibility.Visible;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void AddPlaylist(object sender, RoutedEventArgs e)
        {
            var screen = new AddPlaylist()
            {
                Owner = this
            };

            if (screen.ShowDialog() == true)
            {
                var newPlaylist = (Playlist)screen.NewPlaylist;


                var playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
                var file_name = newPlaylist.Name + ".txt";
                var pathString = Path.Combine(playlist_folder, file_name);

                int count = 1;
                while (File.Exists(pathString))
                {
                    file_name = $"{newPlaylist.Name}({count})";
                    pathString = Path.Combine(playlist_folder, file_name + ".txt");
                    count++;
                }

                File.Create(pathString);

                if (count != 1)
                {
                    newPlaylist.Name = file_name;
                }

                AllPlaylist.Add(newPlaylist);
            }
            else
            {
                return;
            }

            CheckVisibility();
        }

        private void AddMedia(object sender, RoutedEventArgs e)
        {
            try
            {

                var fd = new OpenFileDialog
                {
                    Filter = "MP4 File (*.mp4)|*.mp4|MP3 Files (*.mp3)|*.mp3|3GP File (*.3gp)|*.3gp|Audio File (*.wma)|*.wma|MOV File (*.mov)|*.mov|AVI File (*.avi)|*.avi|Flash Video(*.flv)|*.flv|Video File (*.wmv)|*.wmv|MPEG-2 File (*.mpeg)|*.mpeg|WebM Video (*.webm)|*.webm|All files (*.*)|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Multiselect = true
                };

                fd.ShowDialog();

                foreach (string filename in fd.FileNames)
                {

                    if (filename != "")
                    {

                        string playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
                        DirectoryInfo directory = new DirectoryInfo(playlist_folder);
                        FileInfo[] playlistTxts = directory.GetFiles("*.txt");

                        var selectedPlaylist = AllPlaylist[Playlist.SelectedIndex];

                        foreach (FileInfo playlist in playlistTxts)
                        {
                            if (Path.GetFileNameWithoutExtension(playlist.Name).Equals(selectedPlaylist.Name))
                            {
                                StreamWriter sw = new StreamWriter(playlist.FullName, true);
                                sw.WriteLine(filename);
                                //Close the file
                                sw.Close();

                                Media media = new Media(Path.GetFileName(filename), new Uri(filename), null, null);

                                selectedPlaylist.AddMediaFile(media);
                                selectedPlaylist.CountMedia = selectedPlaylist.CountPlaylistItems();


                                SelectedPlaylist.Add(media);
                                break;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (Exception e1)
            {
                System.Console.WriteLine("Error Text: " + e1.Message);
            }

            CheckVisibility();

        }

        private void ReadPlaylist()
        {
            try
            {
                string playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
                DirectoryInfo directory = new DirectoryInfo(playlist_folder);

                FileInfo[] files = null;

                try
                {
                    files = directory.GetFiles("*.txt");
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                }

                if (files != null)
                {
                    foreach (FileInfo file in files)
                    {
                        string[] lines = File.ReadAllLines(file.FullName);
                        Playlist playlist = new Playlist()
                        {
                            Name = Path.GetFileNameWithoutExtension(file.Name),
                        };

                        foreach (string line in lines)
                        {
                            Media media = new Media(Path.GetFileName(line), new Uri(line), null, null);

                            playlist.AddMediaFile(media);
                        }

                        AllPlaylist.Add(playlist);

                        playlist.CountMedia = playlist.CountPlaylistItems();
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }

            CheckVisibility();
        }

        private void AddToRecentMedia(string mediaPath)
        {
            string recentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Recent";
            string recentMediaFilePath = Path.Combine(recentFolder, RecentMediaFileName);

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

        private void ReadRecentFiles()
        {
            string recentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Recent";
            string recentMediaFilePath = Path.Combine(recentFolder, RecentMediaFileName);

            if (File.Exists(recentMediaFilePath))
            {
                try
                {
                    RecentFiles.Clear();
                    string[] lines = File.ReadAllLines(recentMediaFilePath);

                    for (int i = lines.Length - 1; i >= 0 && i >= lines.Length - 10; i--)
                    {
                        string line = lines[i];
                        if (File.Exists(line))
                        {
                            Media media = new Media(Path.GetFileNameWithoutExtension(line), new Uri(line), null, null);
                            RecentFiles.Add(media);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading recent media file: " + ex.Message);
                }
                CheckVisibility();

            }
            else
            {
                Console.WriteLine("Recent media file does not exist");
            }
        }

        private void DeletePlaylist(object sender, RoutedEventArgs e)
        {
            int selectedPlaylistIndex = Playlist.SelectedIndex;

            var selectedPlaylist = AllPlaylist[selectedPlaylistIndex];

            if (selectedPlaylistIndex < 0) return;

            string playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
            string file_name = selectedPlaylist.Name + ".txt";
            AllPlaylist.RemoveAt(selectedPlaylistIndex);

            string pathString = Path.Combine(playlist_folder, file_name);

            if (File.Exists(pathString))
            {
                try
                {
                    File.Delete(pathString);
                }
                catch (IOException k)
                {
                    Console.WriteLine(k.Message);
                    return;
                }
            }

            SelectedPlaylist.Clear();

            CheckVisibility();
        }

        private void PlaylistClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int i = Playlist.SelectedIndex;

            currentPlaylistIndex = i;

            var currentPlaylist = AllPlaylist[i];

            if (i < 0) return;

            int totalMedia = AllPlaylist[i].MediaList.Count;

            if (SelectedPlaylist.Count != 0)
                SelectedPlaylist.Clear();

            for (int j = 0; j < totalMedia; j++)
            {
                Media media = new Media(currentPlaylist.MediaList[j].Name, currentPlaylist.MediaList[j].Uri, null, null);
                SelectedPlaylist.Add(media);
            }

            CheckVisibility();

        }

        private void PlaylistDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            int i = Playlist.SelectedIndex;

            currentPlaylistIndex = i;

            var selectedPlaylist = AllPlaylist[currentPlaylistIndex];

            if (selectedPlaylist.MediaList.Count == 0)
                return;

            var mediaIndex = 0;


            //----------open media player window----------------
            //var screen = new Media_Playing(Media_Files.SelectedIndex);


       
        }

        private void MediaFilesDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var mediaIndex = Media_Files.SelectedIndex;

            var media = SelectedPlaylist[mediaIndex];

            var selectedPlaylist = AllPlaylist[currentPlaylistIndex];


            if (mediaIndex < 0)
                return;

            //----------open media player window----------------
            //var screen = new Media_Playing(Media_Files.SelectedIndex);

            string pathString = media.Uri.LocalPath;

            AddToRecentMedia(pathString);

            ReadRecentFiles();
        }

        private void DeleteMedia(object sender, RoutedEventArgs e)
        {
            var selectedMedia = Media_Files.SelectedIndex;
            SelectedPlaylist.RemoveAt(selectedMedia);

            try
            {
                string playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
                DirectoryInfo directory = new DirectoryInfo(playlist_folder);
                FileInfo[] files = null;

                try
                {
                    files = directory.GetFiles(AllPlaylist[currentPlaylistIndex].Name + ".txt");
                }
                catch (UnauthorizedAccessException UA)
                {
                    Console.WriteLine(UA.Message);
                }

                if (files != null)
                {
                    foreach (FileInfo file in files)
                    {

                        string[] lines = File.ReadAllLines(file.FullName);
                        StreamWriter sw = new StreamWriter(file.FullName, false);
                        foreach (string line in lines)
                        {
                            if (line == lines[selectedMedia])
                            {
                                continue;
                            }
                            sw.WriteLine(line);
                        }
                        sw.Close();
                    }
                    AllPlaylist[currentPlaylistIndex].MediaList.RemoveAt(selectedMedia);
                    AllPlaylist[currentPlaylistIndex].CountMedia = AllPlaylist[currentPlaylistIndex].CountPlaylistItems();
                }
                else
                {
                    return;
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Exception: " + E.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }

            CheckVisibility();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                var fixedScrollAmount = 50;
                if (e.Delta > 0)
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - fixedScrollAmount);
                else
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + fixedScrollAmount);

                e.Handled = true;
            }
        }

    }
}