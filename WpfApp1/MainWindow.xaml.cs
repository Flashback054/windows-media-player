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
        private void createDirectory()
        {
            string playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
            string source_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Source";
            string recent_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Recent";
            Directory.CreateDirectory(playlist_folder);
            Directory.CreateDirectory(source_folder);
            Directory.CreateDirectory(recent_folder);
        }


        public BindingList<Playlist> AllPlaylist
        {
            get;set;
        } = new BindingList<Playlist>();

        public BindingList<Media> SelectedPlaylist
        {
            get;set;
        } = new BindingList<Media>();
        
        public BindingList<Media> RecentFiles
        {
            get;set;
        } = new BindingList<Media>();


        int current_playlist_index = 0;

        public Visibility RecentVisibility { get; set; } = Visibility.Visible;
        public Visibility PlaylistVisibility { get; set; } = Visibility.Visible;
        public Visibility MediaVisibility { get; set; } = Visibility.Visible;

        private const string RecentMediaFileName = "recent_media.txt";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            createDirectory();

            Playlist.ItemsSource = AllPlaylist;
            Media_Files.ItemsSource = SelectedPlaylist;
            Recent_files.ItemsSource = RecentFiles;
            Read_Playlist();
            Read_Recent_Files();
            CheckVisibility();

        }

        public void CheckVisibility()
        {
            if(RecentFiles.Count != 0)
            {
                RecentVisibility = Visibility.Collapsed;
            }
            else
            {
                RecentVisibility = Visibility.Visible;
            }

            if(AllPlaylist.Count != 0)
            {
                PlaylistVisibility = Visibility.Collapsed;
            }
            else
            {
                PlaylistVisibility = Visibility.Visible;
            }

            if(SelectedPlaylist.Count != 0)
            {
                MediaVisibility = Visibility.Collapsed;
            }
            else
            {
                MediaVisibility = Visibility.Visible;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Add_Playlist(object sender, RoutedEventArgs e)
        {
            var screen = new AddPlaylist()
            {
                Owner = this
            };

            if (screen.ShowDialog() == true)
            {
                var new_playlist = (Playlist)screen.NewPlaylist ;
                

                string playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
                string file_name = new_playlist.PlaylistName + ".txt";
                string pathString = Path.Combine(playlist_folder, file_name);

                int count = 1;
                while (File.Exists(pathString))
                {
                    file_name = $"{new_playlist.PlaylistName}({count})";
                    pathString = Path.Combine(playlist_folder, file_name + ".txt");
                    count++;
                }

                File.Create(pathString);

                if(count != 1)
                {
                    new_playlist.PlaylistName = file_name;
                }

                AllPlaylist.Add(new_playlist);
            }
            else
            {
                return;
            }

            CheckVisibility();
        }

        private void Add_Media(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();

                fd.Filter = "MP3 Files (*.mp3)|*.mp3|MP4 File (*.mp4)|*.mp4|3GP File (*.3gp)|*.3gp|Audio File (*.wma)|*.wma|MOV File (*.mov)|*.mov|AVI File (*.avi)|*.avi|Flash Video(*.flv)|*.flv|Video File (*.wmv)|*.wmv|MPEG-2 File (*.mpeg)|*.mpeg|WebM Video (*.webm)|*.webm|All files (*.*)|*.*";
                fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                fd.Multiselect = true;
                fd.ShowDialog();

                string source_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Source";
              
                foreach (string filename in fd.FileNames)
                {
                   
                    if (filename != "")
                    {

                        string playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
                        DirectoryInfo directory = new DirectoryInfo(playlist_folder);
                        FileInfo[] files = directory.GetFiles("*.txt");

                        foreach (FileInfo file in files)
                        {
                            if (Path.GetFileNameWithoutExtension(file.Name).Equals(AllPlaylist[Playlist.SelectedIndex].PlaylistName))
                            {
                                File.Copy(filename, source_folder + "\\" + Path.GetFileName(filename), true);
                                StreamWriter sw = new StreamWriter(file.FullName, true);
                                sw.WriteLine(source_folder + "\\" + fd.SafeFileName);
                                //Close the file
                                sw.Close();

                                Media media = new Media(Path.GetFileName(filename), new Uri(source_folder + "\\" + Path.GetFileName(filename)));
                                AllPlaylist[Playlist.SelectedIndex].AddMediaFile(media);
                                AllPlaylist[Playlist.SelectedIndex].CountMedia = AllPlaylist[Playlist.SelectedIndex].CountPlaylistItems();
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
        private void Read_Playlist()
        {
            try
            {
                string playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
                DirectoryInfo directory = new DirectoryInfo(playlist_folder);
                string source_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Source\\";
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
                    foreach (FileInfo fi in files)
                    {
                        string[] lines = File.ReadAllLines(fi.FullName);
                        Playlist playlist = new Playlist()
                        {
                            PlaylistName = Path.GetFileNameWithoutExtension(fi.Name),
                        };

                        foreach (string name in lines)
                        {
                            string new_name = name.Replace(source_folder, "");
                            //media.create_Media(new_name, new Uri(source_folder + new_name));
                            Media media = new Media(new_name, new Uri(source_folder + new_name));

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

        private void Read_Recent_Files()
        {
            string recentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Recent";
            string recentMediaFilePath = Path.Combine(recentFolder, RecentMediaFileName);

            if (File.Exists(recentMediaFilePath))
            {
                try
                {
                    RecentFiles.Clear();
                    string[] lines = File.ReadAllLines(recentMediaFilePath);

                    for (int i = lines.Length - 1; i >= 0 && i >= lines.Length-10; i--)
                    {
                        string line = lines[i];
                        if (File.Exists(line))
                        {
                            Media media = new Media(Path.GetFileNameWithoutExtension(line), new Uri(line));
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
   
        private void Delete_Playlist(object sender, RoutedEventArgs e)
        {
            int i = Playlist.SelectedIndex;
            if (i < 0) return;

            string playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
            string file_name = AllPlaylist[i].PlaylistName + ".txt";
            AllPlaylist.RemoveAt(i);
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

        private void Playlist_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int i = Playlist.SelectedIndex;
            current_playlist_index = i;
            if (i < 0) return;
            int a = AllPlaylist[i].MediaList.Count;

            if (SelectedPlaylist.Count != 0)
                SelectedPlaylist.Clear();

            for (int j = 0; j < a; j++)
            {
                //media.create_Media();
                Media media = new Media(AllPlaylist[i].MediaList[j].Name, AllPlaylist[i].MediaList[j].Uri);

                SelectedPlaylist.Add(media);
            }

            CheckVisibility();

        }

        private void Media_Files_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Media_Files.SelectedIndex < 0)
                return;

            //var screen = new Media_Playing(Media_Files.SelectedIndex);
            string source_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Source";
            string media_name = SelectedPlaylist[Media_Files.SelectedIndex].Name;
            string pathString = Path.Combine(source_folder, media_name);

            AddToRecentMedia(pathString);
            //if (screen.ShowDialog() == true)
            //{

            //}
            //else
            //{

            //}

            Read_Recent_Files();
        }

        private void Delete_Media(object sender, RoutedEventArgs e)
        {
            int i = Media_Files.SelectedIndex;
            SelectedPlaylist.RemoveAt(i);

            try
            {
                string playlist_folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Playlist";
                DirectoryInfo directory = new DirectoryInfo(playlist_folder);
                FileInfo[] files = null;

                // First, process all the files directly under this folder
                try
                {
                    files = directory.GetFiles(AllPlaylist[current_playlist_index].PlaylistName + ".txt");
                }
                // This is thrown if even one of the files requires permissions greater
                // than the application provides.
                catch (UnauthorizedAccessException UA)
                {

                }

                if (files != null)
                {
                    foreach (FileInfo fi in files)
                    {

                        string[] lines = File.ReadAllLines(fi.FullName);
                        StreamWriter sw = new StreamWriter(fi.FullName, false);
                        foreach (string line in lines)
                        {
                            if (line == lines[i])
                            {
                                continue;
                            }
                            sw.WriteLine(line);
                        }
                        sw.Close();
                    }
                    AllPlaylist[current_playlist_index].MediaList.RemoveAt(i);
                    AllPlaylist[current_playlist_index].CountMedia = AllPlaylist[current_playlist_index].CountPlaylistItems();
                }
                else
                {
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