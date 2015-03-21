using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Traktrix.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace Traktrix
{
    public sealed partial class SongDetails : SettingsFlyout
    {
        StorageFile temp_file = null;
        public SongDetails()
        {
            this.InitializeComponent();
            try
            {
                SetMusic();
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }
        }

        public SongDetails(StorageFile file, MusicProperties prop)
        {
            this.InitializeComponent();
            try
            {
                temp_file = file;
                MusicDetails(file, prop);
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }
        }

        #region MusicControls
        private void SetRating(MusicProperties prop)
        {
            try
            {
                if (prop.Rating == 0)
                {
                    Rating.Width = 2.5 * 24;
                }
                else
                {
                    Rating.Width = prop.Rating * 24;
                }
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }
        }

        private async void MusicDetails(StorageFile file, MusicProperties prop)
        {
            if (!prop.Title.Equals(""))
            {
                SongName.Text = prop.Title;
            }
            else
            {
                SongName.Text = "N/A";
            }

            if (!prop.Album.Equals(""))
            {
                AlbumName.Text = prop.Album;
            }
            else
            {
                AlbumName.Text = "N/A";
            }

            if (!prop.Artist.Equals(""))
            {
                ArtistName.Text = prop.Artist;
            }
            else
            {
                ArtistName.Text = "N/A";
            }

            if (prop.Genre.Count != 0)
            {
                Genre.Text = (String)prop.Genre[0];
            }
            else
            {
                Genre.Text = "N/A";
            }

            if (!prop.Year.Equals(""))
            {
                Year.Text = prop.Year.ToString();
            }
            else
            {
                Year.Text = "N/A";
            }

            if (!prop.Bitrate.Equals(""))
            {
                Bitrate.Text = prop.Bitrate.ToString();
            }
            else
            {
                Bitrate.Text = prop.Bitrate.ToString();
            }

            StorageFile file_;
            if (file == null)
            {
                file_ = AudioSingleton.Instance.currentSong;
            }
            else
            {
                file_ = file;
            }

            using (StorageItemThumbnail thumbnail = await file_.GetThumbnailAsync(ThumbnailMode.MusicView, 250))
            {
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(thumbnail);
                    CoverArt.Source = bitmapImage;
                }
                else
                {
                    var bitmapImage = new BitmapImage()
                    {
                        UriSource = new Uri("/Assets/DefaultMusicLogo.jpg")
                    };
                    CoverArt.Source = bitmapImage;
                }

            }


            SetRating(prop);
            DateTime dtime = DateTime.MinValue.Add(prop.Duration);
            Duration.Text = dtime.ToString(@"mm\:ss"); ;

        }

        private async void SetMusic()
        {
            try
            {
                var prop = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();
                MusicDetails(null, prop);
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }

        }
        #endregion

        #region Rating Handlers
        private async void CallRater(StorageFile file, uint Rating)
        {
            try
            {
                Star1.IsEnabled = false;
                Star2.IsEnabled = false;
                Star3.IsEnabled = false;
                Star4.IsEnabled = false;
                Star5.IsEnabled = false;

                var prop = await file.Properties.GetMusicPropertiesAsync();
                prop.Rating = Rating;
                await prop.SavePropertiesAsync();
                SetRating(prop);

                Star1.IsEnabled = true;
                Star2.IsEnabled = true;
                Star3.IsEnabled = true;
                Star4.IsEnabled = true;
                Star5.IsEnabled = true;
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }
        }

        private void Rate_1(object sender, RoutedEventArgs e)
        {
            if (temp_file != null)
            {
                CallRater(temp_file, 1);
            }
            else
            {
                CallRater(AudioSingleton.Instance.currentSong, 1);
            }
        }

        private void Rate_2(object sender, RoutedEventArgs e)
        {
            if (temp_file != null)
            {
                CallRater(temp_file, 2);
            }
            else
            {
                CallRater(AudioSingleton.Instance.currentSong, 2);
            }
        }

        private void Rate_3(object sender, RoutedEventArgs e)
        {
            if (temp_file != null)
            {
                CallRater(temp_file, 3);
            }
            else
            {
                CallRater(AudioSingleton.Instance.currentSong, 3);
            }
        }

        private void Rate_4(object sender, RoutedEventArgs e)
        {
            if (temp_file != null)
            {
                CallRater(temp_file, 4);
            }
            else
            {
                CallRater(AudioSingleton.Instance.currentSong, 4);
            }
        }

        private void Rate_5(object sender, RoutedEventArgs e)
        {
            if (temp_file != null)
            {
                CallRater(temp_file, 5);
            }
            else
            {
                CallRater(AudioSingleton.Instance.currentSong, 5);
            }
        }

        #endregion
    }
}
