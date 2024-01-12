using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MediaPlayer.Models
{
    public class Media(string? name, Uri? uri, string? thumbnail, TimeSpan? lastSeekPosition) : INotifyPropertyChanged
    {
        public string? Name { get; set; } = name;
        public Uri? Uri { get; set; } = uri;
        public BitmapImage? Thumbnail
        {
            get
            {
                TagLib.File f = TagLib.File.Create(Uri.UnescapeDataString(this.Uri.LocalPath));
                BitmapImage bitmap;

                if (f.Tag.Pictures != null && f.Tag.Pictures.Length >= 1)
                {
                    TagLib.IPicture pic = f.Tag.Pictures[0];

                    MemoryStream ms = new MemoryStream(pic.Data.Data);
                    ms.Seek(0, SeekOrigin.Begin);

                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                }
                else
                {
                    bitmap = new BitmapImage(new Uri("/Assets/Icons/default.png", UriKind.RelativeOrAbsolute));
                }

                return bitmap;
            }
        }
        public TimeSpan LastSeekPosition { get; set; } = lastSeekPosition ?? TimeSpan.Zero;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
