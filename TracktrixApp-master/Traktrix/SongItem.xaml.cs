using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Traktrix.AudioPlayer;
using Traktrix.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Traktrix
{
    public sealed partial class SongItem : UserControl
    {
        public string title_ = "";
        public MusicProperties prop;
        public StorageFile file;

        public SongItem()
        {
            this.InitializeComponent();
        }

        public void SetSongItem(StorageFile temp_file, MusicProperties prop, String Title, String Artist, uint rating, BitmapImage img_)
        {
            this.file = temp_file;
            this.prop = prop;
            title_ = Title;
            this.SongName.Text = Title;
            this.ArtistName.Text = Artist;
            this.CoverArt.Source = img_;

            if (rating > 5)
            {
                rating = rating / 10;
            }

            if (rating == 0)
            {
                Rating.Width = 2.5 * 12;
            }
            else
            {
                Rating.Width = rating * 12;
            }

            this.RightTapped += new RightTappedEventHandler(AttachmentImage_RightTapped);
        }

        public void SetAlphabet(String value_)
        {
            AlphabetStackPanel.Visibility = Visibility.Visible;
            Alphabet.Text = value_;
            this.Height = 140;
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        private async void ResetSong(bool value)
        {
            AudioSingleton.Instance.currentSong = file;

            if (AudioSingleton.Instance.IsPaused || AudioSingleton.Instance.SongPlaying)
            {
                if (AudioSingleton.Instance.IsPaused)
                {
                    AudioSingleton.Instance.IsPaused = false;
                }

                if (!AudioSingleton.Instance.SongPlaying)
                {
                    AudioSingleton.Instance.SongPlaying = true;
                }

                try
                {
                    AudioPlayer.AudioPlayer.Instance.StopSong();
                    AudioSingleton.Instance.temp_timer.Stop();
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                }
                catch (Exception e1)
                {

                }
            }

            if (value)
            {
                AudioPlayer.AudioPlayer.Instance.setFilterNo(0);
                PlayingPage.PlaySong(AudioSingleton.Instance.currentSong);
                AudioSingleton.Instance.SongPlaying = true;

                AudioSingleton.Instance.temp_timer = new DispatcherTimer();
                AudioSingleton.Instance.temp_timer.Interval = TimeSpan.FromSeconds(1);
                AudioSingleton.Instance.temp_timer.Tick += AudioSingleton.UpdateTimer_Tick;
                AudioSingleton.Instance.temp_timer.Start();
                AudioSingleton.Instance.ElapsedSeconds = 0;
                if (!AudioSingleton.Instance.LoadPlayingPage)
                {
                    AudioSingleton.Instance.LoadPlayingPage = true;
                }
            }
            else
            {
                if (!AudioSingleton.Instance.LoadTrixterMode)
                {
                    AudioSingleton.Instance.LoadTrixterMode = true;
                }
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(TrixterMode), null);
            }
        }

        private void AttachmentImage_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout(MainStackPanel);
        }

        private void SongDetails_Flyout_Click(object sender, RoutedEventArgs e)
        {
            SongDetails temp = new SongDetails(file, prop);
            temp.ShowIndependent();
        }

        private void EditSong_Flyout_Click(object sender, RoutedEventArgs e)
        {
            ResetSong(false);

        }

        private void PlaySong_Flyout_Click(object sender, RoutedEventArgs e)
        {
            ResetSong(true);
        }
    }
}
