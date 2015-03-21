using Traktrix.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI.ApplicationSettings;
using Traktrix;
using Windows.UI.Popups;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http;
using System.Threading.Tasks;
using Traktrix.AudioPlayer;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Media.Transcoding;
using Windows.Media.MediaProperties;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Traktrix
{
    public sealed partial class PlayingPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public static MediaElement temp_media_element = new MediaElement();
        DispatcherTimer UpdateTimer;


        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        #region NavigationHelper registration
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public PlayingPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            Window.Current.SizeChanged += Current_SizeChanged;

        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }


        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            try
            {
                if ((bool)AudioSingleton.Instance.IsShuffleEnabled)
                {
                    ShuffleButton.IsChecked = true;
                }
                else
                {
                    ShuffleButton.IsChecked = false;
                }

                AudioSingleton.Instance.song_changer.Tick += song_changer_Tick;
                AudioSingleton.Instance.song_changer.Start();

                if ((bool)AudioSingleton.Instance.IsRepeatEnabled)
                {
                    RepeatButton.IsChecked = true;
                }
                else
                {
                    RepeatButton.IsChecked = false;
                }

                ShuffleButton_Checked(null, null);
                RepeatButton_Checked(null, null);
                VolumeSlider.Value = (double)AudioSingleton.Instance.LastVolumeValue * 100;

                InvalidateSize();
                SetVolumeImages();

                SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
                //this.DataContext = AudioPlayer.AudioPlayer.Instance;
                //PointCollection p = new PointCollection();
                //for (int i = 0; i < 1024; i++)
                //{
                //    p.Add(new Point(i*Visualizer.Width/(double)1024,20*Math.Sin((double)i/180*Math.PI)));
                //}
                //AudioPlayer.AudioPlayer.Instance.SoundWave = p;

                if (AudioSingleton.Instance.SongPlaying)
                {
                    StopButton.IsEnabled = true;
                    StopButton.Opacity = 0.7;
                }
                else
                {
                    StopButton.IsEnabled = false;
                    StopButton.Opacity = 0.3;
                }

                if (!AudioSingleton.Instance.LoadPlayingPage)
                {
                    ProgressGrid.Visibility = Visibility.Visible;
                    try
                    {
                        while (!AudioSingleton.Instance.AllSongsLoaded)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(100));
                        }
                        ProgressGrid.Visibility = Visibility.Collapsed;

                        //                await AudioSingleton.Instance.GeneratePlaylist();
                        Playlist.Items.Clear();
                        if (AudioSingleton.Instance.PlayList.Count != 0)
                        {

                            //foreach (StorageFile file in AudioSingleton.Instance.PlayList)
                            //{
                            //    var prop = await file.Properties.GetMusicPropertiesAsync();
                            //    try
                            //    {
                            //        await file.RenameAsync(prop.Title + file.FileType);
                            //    }
                            //    catch (Exception e)
                            //    {

                            //    }

                            //    if (prop.Title != "")
                            //        Playlist.Items.Add(file);
                            //}

                            foreach (StorageFile file in AudioSingleton.Instance.PlayList)
                            {
                                var prop = await file.Properties.GetMusicPropertiesAsync();
                                //try
                                //{
                                //    await file.RenameAsync(prop.Title + file.FileType);
                                //}
                                //catch (Exception e)
                                //{

                                //}

                                if (prop.Title != "")
                                    Playlist.Items.Add(file);
                            }


                            if (!AudioSingleton.Instance.ObjectSelected)
                            {
                                AudioSingleton.Instance.IndexValue = 0;
                                Playlist.SelectedIndex = 0;
                                AudioSingleton.Instance.currentSong = Playlist.SelectedItem as StorageFile;
                                Playlist.ScrollIntoView(Playlist.SelectedItem);
                                AudioSingleton.Instance.ObjectSelected = true;
                            }

                            AudioSingleton.Instance.AddToPreviousSongList(AudioSingleton.Instance.currentSong);

                            var prop1 = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();
                            SetMusic(AudioSingleton.Instance.currentSong, prop1);
                        }
                        else
                        {
                            MessageDialog temp_dialog = new MessageDialog("It appears to us that there are no songs in the default music library location of your machine. Kindly select 'Proceed' below and folllow the method listed in the help to fix the problem.", "Default location for music library is not set correctly");
                            temp_dialog.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(CommandHandler)));
                            await temp_dialog.ShowAsync();
                        }
                    }
                    catch (Exception e_)
                    {
                        System.Diagnostics.Debug.WriteLine(e_.StackTrace);
                    }


                }
                else
                {
                    ProgressGrid.Visibility = Visibility.Visible;
                    try
                    {
                        RestartSongLoading();
                        ProgressGrid.Visibility = Visibility.Collapsed;

                        Playlist.Items.Clear();
                        if (AudioSingleton.Instance.PlayList.Count != 0)
                        {
                            foreach (StorageFile file in AudioSingleton.Instance.PlayList)
                            {
                                var prop = await file.Properties.GetMusicPropertiesAsync();
                                if (prop.Title != "")
                                    Playlist.Items.Add(file);
                            }

                            AudioSingleton.Instance.IndexValue = Playlist.Items.ToList().FindIndex(x => x.Equals(AudioSingleton.Instance.currentSong));
                            Playlist.SelectedItem = Playlist.Items[AudioSingleton.Instance.IndexValue];
                            Playlist.ScrollIntoView(Playlist.SelectedItem);

                            //                        SetMusic();

                        }
                        else
                        {
                            MessageDialog temp_dialog = new MessageDialog("It appears to us that there are no songs in the default music library location of your machine. Kindly select 'Proceed' below and folllow the method listed in the help to fix the problem.", "Default location for music library is not set correctly");
                            temp_dialog.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(CommandHandler)));
                            await temp_dialog.ShowAsync();
                        }
                    }
                    catch (Exception e_)
                    {
                        System.Diagnostics.Debug.WriteLine(e_.StackTrace);
                    }
                }

                AudioSingleton.Instance.LoadPlayingPage = true;
                Window.Current.SizeChanged += Current_SizeChanged;

            }
            catch (Exception)
            {
            }
            navigationHelper.OnNavigatedTo(e);
        }
        
        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            InvalidateSize();
        }

        private void InvalidateSize()
        {
            var view_mode = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            if (view_mode.IsFullScreen)
            {
                FullScreen_Grid.Visibility = Visibility.Visible;
                HalfScreen_Grid.Visibility = Visibility.Collapsed;
                GalleryButton.Visibility = Visibility.Visible;
                ProgressGrid.Margin = new Thickness(0, 0, 0, 0);
                TitleName.FontSize = 70;
                CurrentTime.FontSize = 100;
                Advertisement.Visibility = Visibility.Visible;
            }
            else
            {
                Advertisement.Visibility = Visibility.Collapsed;
                SetMiniPlayerInfo();
                FullScreen_Grid.Visibility = Visibility.Collapsed;
                HalfScreen_Grid.Visibility = Visibility.Visible;
                GalleryButton.Visibility = Visibility.Collapsed;
                ProgressGrid.Margin = new Thickness(0, -120, 0, 0);
                TitleName.FontSize = 60;
                CurrentTime.FontSize = 90;

            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            try
            {
                AudioSingleton.Instance.song_changer.Stop();
                AudioSingleton.Instance.song_changer.Tick -= song_changer_Tick;

                try
                {
                    UpdateTimer.Stop();
                    UpdateTimer.Tick -= temp_timer_Tick;
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine(e1);

                }
                SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested;
            }
            catch (Exception)
            {
            }
            navigationHelper.OnNavigatedFrom(e);
        }

        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs e)
        {
            e.Request.ApplicationCommands.Clear();


            SettingsCommand song_details = new SettingsCommand("songdetails", "Song Details",
          (handler) =>
          {
              SongDetails sf = new SongDetails();
              sf.Show();
          });

            e.Request.ApplicationCommands.Add(song_details);

            SettingsCommand personalize_command = new SettingsCommand("personalize", "Personalize",
               (handler) =>
               {
                   ThemeChanger sf = new ThemeChanger();
                   sf.Show();
               });
            e.Request.ApplicationCommands.Add(personalize_command);


            SettingsCommand about_us_command = new SettingsCommand("aboutus", "About Us",
                (handler) =>
                {
                    AboutUs sf = new AboutUs();
                    sf.Show();
                });
            e.Request.ApplicationCommands.Add(about_us_command);

            SettingsCommand help_command = new SettingsCommand("aboutus", "Help",
                (handler) =>
                {
                    Help sf = new Help();
                    sf.Show();
                });
            e.Request.ApplicationCommands.Add(help_command);

        }

        #endregion

        #region MusicControls
        private async void setLyrics(MusicProperties prop)
        {
            LyricsPane.Text = "--- No Lyrics available ---";
            LyricsProgress.Visibility = Visibility.Visible;
            bool first_exception = false;
            String lyrics_default = "--- No Lyrics available ---";

            try
            {
                String url = "http://www.azlyrics.com/lyrics/" + prop.Artist.ToLower() + "/" + prop.Title.ToLower() + ".html";

                HttpClient temp = new HttpClient();
                String result = await temp.GetStringAsync(url);

                String temp_ = result.Substring(result.IndexOf("start of lyrics -->") + 19);

                temp_ = temp_.Substring(0, temp_.IndexOf("end of lyrics") - 5);
                temp_ = temp_.Replace("<br />", "");
                temp_ = HtmlRemoval.StripTagsRegex(temp_);
                lyrics_default = temp_;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
                lyrics_default = "--- No Lyrics available ---";
                first_exception = true;
            }

            if (first_exception)
            {
                try
                {
                    String url = "http://www.lyricsmode.com/lyrics/" + prop.Artist.ToLower().ToCharArray()[0] + "/" + prop.Artist.ToLower().Replace(" ", "_") + "/" + prop.Title.ToLower().Replace(" ", "_") + ".html";

                    HttpClient temp = new HttpClient();
                    String result = await temp.GetStringAsync(url);

                    String temp_ = result.Substring(result.IndexOf("<p id=\"lyrics_text\" class=\"ui-annotatable\">") + 43);
                    temp_ = temp_.Substring(0, temp_.IndexOf("<p id=") - 4);
                    temp_ = temp_.Replace("<br />", "");
                    temp_ = HtmlRemoval.StripTagsRegex(temp_);
                    lyrics_default = temp_;
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                    lyrics_default = "--- No Lyrics available ---";
                }
            }

            LyricsPane.Text = lyrics_default;
            LyricsProgress.Visibility = Visibility.Collapsed;

        }

        private async Task SetRating(MusicProperties prop)
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

        private async void SetMusic(StorageFile file, MusicProperties prop)
        {
            try
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

                using (StorageItemThumbnail thumbnail = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 250))
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
                            UriSource = new Uri("ms-appx:/Assets/DefaultMusicLogo.jpg", UriKind.Absolute)
                        };
                        CoverArt.Source = bitmapImage;
                    }
                }

                DateTime dtime = DateTime.MinValue.Add(prop.Duration);
                TotalTime.Text = dtime.ToString(@"mm\:ss"); ;
                AudioSingleton.Instance.SongTotalSeconds = prop.Duration.TotalSeconds;

                await SetRating(prop);
                setLyrics(prop);
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }

        }
        #endregion

        #region Button Handlers
        private async void ListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AudioSingleton.Instance.ObjectSelected = true;
            ProgressGrid.Visibility = Visibility.Visible;
            try
            {
                Playlist.ScrollIntoView(Playlist.SelectedItem);

                var prop = await ((StorageFile)Playlist.SelectedItem).Properties.GetMusicPropertiesAsync();
                SetMusic((StorageFile)Playlist.SelectedItem, prop);
            }
            catch (Exception e_)
            {
                System.Diagnostics.Debug.WriteLine(e_.StackTrace);
            }

            ProgressGrid.Visibility = Visibility.Collapsed;
        }

        private void CommandHandler(IUICommand command)
        {
            Help sf = new Help();
            sf.ShowIndependent();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            SongDetails sf = new SongDetails();
            sf.ShowIndependent();
        }
        #endregion

        #region Rating Handlers
        private async void CallRater(uint Rating)
        {
            bool exception = false;
            try
            {
                Star1.IsEnabled = false;
                Star2.IsEnabled = false;
                Star3.IsEnabled = false;
                Star4.IsEnabled = false;
                Star5.IsEnabled = false;

                MusicProperties prop;
                var view_mode = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
                if (view_mode.IsFullScreen)
                {
                    prop = await ((StorageFile)Playlist.SelectedItem).Properties.GetMusicPropertiesAsync();
                    prop.Rating = Rating;
                    await prop.SavePropertiesAsync();
                    await SetRating(prop);
                }
                else
                {
                    prop = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();
                    prop.Rating = Rating;
                    await prop.SavePropertiesAsync();
                    if (prop.Rating == 0)
                    {
                        Rating2.Width = 2.5 * 24;
                    }
                    else
                    {
                        Rating2.Width = prop.Rating * 24;
                    }
                }


                Star1.IsEnabled = true;
                Star2.IsEnabled = true;
                Star3.IsEnabled = true;
                Star4.IsEnabled = true;
                Star5.IsEnabled = true;
            }
            catch (Exception e1)
            {
                exception = true;
                Star1.IsEnabled = true;
                Star2.IsEnabled = true;
                Star3.IsEnabled = true;
                Star4.IsEnabled = true;
                Star5.IsEnabled = true;
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }


            if (exception)
            {
                MessageDialog temp_dialog = new MessageDialog("It appears that the file that you are trying to rate is write-protected.", "Unable to rate the song");
                await temp_dialog.ShowAsync();
            }
        }

        private void Rate_1(object sender, RoutedEventArgs e)
        {
            CallRater(1);
        }

        private void Rate_2(object sender, RoutedEventArgs e)
        {
            CallRater(2);
        }

        private void Rate_3(object sender, RoutedEventArgs e)
        {
            CallRater(3);
        }

        private void Rate_4(object sender, RoutedEventArgs e)
        {
            CallRater(4);
        }

        private void Rate_5(object sender, RoutedEventArgs e)
        {
            CallRater(5);
        }

        #endregion

        #region Music Playing Handlers
        private void SetVolumeImages()
        {
            if (VolumeSlider.Value < 20)
            {
                AudioSingleton.Instance.LastNormalVolume = "/Assets/Icons/Volume0_Normal.png";
                AudioSingleton.Instance.LastHoverVolumeImage = "/Assets/Icons/Volume0_Normal_" + ((TextBlock)App.Current.Resources["ColorName"]).Text + ".png";
                VolumeButton.NormalStateImageSource = new BitmapImage()
                {
                    UriSource = new Uri("ms-appx:/Assets/Icons/Volume0_Normal.png", UriKind.Absolute)
                };
                VolumeButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["Volume0IconHover"];
            }
            else if (VolumeSlider.Value < 40)
            {
                AudioSingleton.Instance.LastNormalVolume = "/Assets/Icons/Volume1_Normal.png";
                AudioSingleton.Instance.LastHoverVolumeImage = "/Assets/Icons/Volume1_Normal_" + ((TextBlock)App.Current.Resources["ColorName"]).Text + ".png";
                VolumeButton.NormalStateImageSource = new BitmapImage()
                {
                    UriSource = new Uri("ms-appx:/Assets/Icons/Volume1_Normal.png", UriKind.Absolute)
                };
                VolumeButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["Volume1IconHover"];
            }
            else if (VolumeSlider.Value < 60)
            {
                AudioSingleton.Instance.LastNormalVolume = "/Assets/Icons/Volume2_Normal.png";
                AudioSingleton.Instance.LastHoverVolumeImage = "/Assets/Icons/Volume2_Normal_" + ((TextBlock)App.Current.Resources["ColorName"]).Text + ".png";
                VolumeButton.NormalStateImageSource = new BitmapImage()
                {
                    UriSource = new Uri("ms-appx:/Assets/Icons/Volume2_Normal.png", UriKind.Absolute)
                };
                VolumeButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["Volume2IconHover"];
            }
            else
            {
                AudioSingleton.Instance.LastNormalVolume = "/Assets/Icons/Volume3_Normal.png";
                AudioSingleton.Instance.LastHoverVolumeImage = "/Assets/Icons/Volume3_Normal_" + ((TextBlock)App.Current.Resources["ColorName"]).Text + ".png";
                VolumeButton.NormalStateImageSource = new BitmapImage()
                {
                    UriSource = new Uri("ms-appx:/Assets/Icons/Volume3_Normal.png", UriKind.Absolute)
                };
                VolumeButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["Volume3IconHover"];
            }
        }

        public static async Task PlaySong(StorageFile file)
        {
            try
            {
                var uri = new Uri(file.Path);
                await ConvertFile(file);
                if (file != null)
                {
                    AudioPlayer.AudioPlayer.Instance.LoadSong(AudioSingleton.Instance.stream);
                    AudioPlayer.AudioPlayer.Instance.PlaySong();
                    AudioPlayer.AudioPlayer.Instance.setVolume((float)AudioSingleton.Instance.LastVolumeValue);
                }
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }
        }

        private async void SetMiniPlayerInfo()
        {
            try
            {
                var prop = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();

                if (!prop.Title.Equals(""))
                {
                    SongNameMini.Text = prop.Title;
                }
                else
                {
                    SongNameMini.Text = "N/A";
                }

                if (!prop.Album.Equals(""))
                {
                    AlbumNameMini.Text = prop.Album;
                }
                else
                {
                    AlbumNameMini.Text = "N/A";
                }

                if (!prop.Artist.Equals(""))
                {
                    ArtistNameMini.Text = prop.Artist;
                }
                else
                {
                    ArtistNameMini.Text = "N/A";
                }

                using (StorageItemThumbnail thumbnail = await AudioSingleton.Instance.currentSong.GetThumbnailAsync(ThumbnailMode.MusicView, 250))
                {
                    if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(thumbnail);
                        CoverArtMini.Source = bitmapImage;
                    }
                    else
                    {
                        var bitmapImage = new BitmapImage()
                        {
                            UriSource = new Uri("ms-appx:/Assets/DefaultMusicLogo.jpg", UriKind.Absolute)
                        };
                        CoverArtMini.Source = bitmapImage;
                    }
                }

                if (prop.Rating == 0)
                {
                    Rating2.Width = 2.5 * 24;
                }
                else
                {
                    Rating2.Width = prop.Rating * 24;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);

                SongNameMini.Text = "N/A";
                AlbumNameMini.Text = "N/A";
                ArtistNameMini.Text = "N/A";
                var bitmapImage = new BitmapImage()
                {
                    UriSource = new Uri("ms-appx:/Assets/DefaultMusicLogo.jpg", UriKind.Absolute)
                };
                CoverArtMini.Source = bitmapImage;
            }
        }

        private static async Task ConvertFile(StorageFile file)
        {
            try
            {
                var dst_file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("output.wav", Windows.Storage.CreationCollisionOption.ReplaceExisting);

                MediaTranscoder temp_transcoder = new MediaTranscoder();
                MediaEncodingProfile profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium);
                var preparedTranscodeResult = await temp_transcoder.PrepareFileTranscodeAsync(file, dst_file, profile);
                await preparedTranscodeResult.TranscodeAsync();

                //using (var dst_stream = await dst_file.OpenStreamForWriteAsync())
                //{
                //    await Mp3ToWav.Module1.TestFromWin8Async(uri, dst_stream);
                //}

                try
                {
                    if (AudioSingleton.Instance.stream != null)
                    {
                        AudioSingleton.Instance.stream.Dispose();
                    }
                }
                catch (Exception)
                {
                }

                AudioSingleton.Instance.stream = await dst_file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }

        }

        private void SongChange()
        {
            StopButton_Click(null, null);
            try
            {
                AudioSingleton.Instance.previous_textblock.Foreground = new SolidColorBrush()
                {
                    Color = Colors.White
                };
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1);
            }

            PlaySong_Change();
        }

        private void PlaySong_Change()
        {
            try
            {
                if (!AudioSingleton.Instance.SongPlaying)
                {
                    try
                    {
                        AudioSingleton.Instance.previous_textblock.Foreground = new SolidColorBrush()
                        {
                            Color = Colors.White
                        };
                    }
                    catch (Exception e1)
                    {
                        System.Diagnostics.Debug.WriteLine(e1);
                    }

                    List<TextBlock> temp = FindVisualChildren<TextBlock>(Playlist);
                    var block_required = temp.Find(x => x.Text == AudioSingleton.Instance.currentSong.DisplayName);
                    block_required.Foreground = new SolidColorBrush()
                    {
                        Color = Colors.Yellow
                    };
                    AudioSingleton.Instance.previous_textblock = block_required;
                    StopButton.IsEnabled = true;
                    StopButton.Opacity = 0.7;
                }
                else
                {
                    AudioPlayer.AudioPlayer.Instance.setFilterNo(0);
                    IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                               async (workItem) =>
                               {
                                   await PlaySong(AudioSingleton.Instance.currentSong);
                                   AudioSingleton.Instance.SongPlaying = true;

                                   await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                               CoreDispatcherPriority.High,
                               new DispatchedHandler(() =>
                               {
                                   EnableLocalTimer();
                                   //                    DispatchTimerEnable();

                                   PlayButton.NormalStateImageSource = (BitmapImage)App.Current.Resources["PauseIconNormal"];
                                   PlayButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["PauseIconPressed"];
                                   StopButton.IsEnabled = true;
                                   StopButton.Opacity = 0.7;
                               }));

                               });

                }
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }

        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Playlist.SelectedItem as StorageFile != null)
                {
                    PlayButton.IsEnabled = false;
                    PlayButton.Opacity = 0.3;
                    try
                    {
                        if (!AudioSingleton.Instance.SongPlaying)
                        {
                            AudioPlayer.AudioPlayer.Instance.setFilterNo(0);

                            if (AudioSingleton.Instance.IsPaused)
                            {
                                IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                                    (workItem) =>
                                    {
                                        AudioPlayer.AudioPlayer.Instance.PlaySong();
                                        AudioSingleton.Instance.IsPaused = false;
                                        AudioSingleton.Instance.SongPlaying = true;
                                    });
                                UpdateTimer.Tick += temp_timer_Tick;
                                AudioSingleton.Instance.temp_timer.Tick += AudioSingleton.UpdateTimer_Tick;
                                PlayButton.NormalStateImageSource = (BitmapImage)App.Current.Resources["PauseIconNormal"];
                                PlayButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["PauseIconPressed"];
                                PlayButton.IsEnabled = true;
                                PlayButton.Opacity = 0.7;
                                StopButton.IsEnabled = true;
                                StopButton.Opacity = 0.7;
                            }
                            else
                            {
                                AudioSingleton.Instance.currentSong = Playlist.SelectedItem as StorageFile;
                                IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                                    async (workItem) =>
                                    {
                                        await PlaySong(AudioSingleton.Instance.currentSong);
                                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                    CoreDispatcherPriority.High,
                                    new DispatchedHandler(() =>
                                    {
                                        DispatchTimerEnable();
                                        AudioSingleton.Instance.SongPlaying = true;
                                        PlayButton.NormalStateImageSource = (BitmapImage)App.Current.Resources["PauseIconNormal"];
                                        PlayButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["PauseIconPressed"];

                                        try
                                        {
                                            AudioSingleton.Instance.previous_textblock.Foreground = new SolidColorBrush()
                                            {
                                                Color = Colors.White
                                            };

                                            List<TextBlock> temp = FindVisualChildren<TextBlock>(Playlist);
                                            var block_required = temp.Find(x => x.Text == AudioSingleton.Instance.currentSong.DisplayName);
                                            block_required.Foreground = new SolidColorBrush()
                                            {
                                                Color = Colors.Yellow
                                            };

                                            AudioSingleton.Instance.previous_textblock = block_required;
                                        }
                                        catch (Exception e1)
                                        {
                                            System.Diagnostics.Debug.WriteLine(e1);
                                        }

                                        PlayButton.IsEnabled = true;
                                        PlayButton.Opacity = 0.7;
                                        StopButton.IsEnabled = true;
                                        StopButton.Opacity = 0.7;
                                    }));

                                    });
                            }
                        }
                        else
                        {
                            IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
            (workItem) =>
            {
                AudioPlayer.AudioPlayer.Instance.PauseSong();
                AudioSingleton.Instance.SongPlaying = false;
                AudioSingleton.Instance.IsPaused = true;
            });

                            PlayButton.NormalStateImageSource = (BitmapImage)App.Current.Resources["PlayIconNormal"];
                            PlayButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["PlayIconPressed"];

                            UpdateTimer.Tick -= temp_timer_Tick;
                            AudioSingleton.Instance.temp_timer.Tick -= AudioSingleton.UpdateTimer_Tick;
                            PlayButton.IsEnabled = true;
                            PlayButton.Opacity = 0.7;
                            StopButton.IsEnabled = true;
                            StopButton.Opacity = 0.7;
                        }
                    }
                    catch (Exception e1)
                    {
                        System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                StopButton.IsEnabled = false;
                StopButton.Opacity = 0.3;
                AudioPlayer.AudioPlayer.Instance.StopSong();

                if (sender != null)
                {
                    AudioSingleton.Instance.SongPlaying = false;
                    AudioSingleton.Instance.IsPaused = false;
                }

                PlayButton.NormalStateImageSource = (BitmapImage)App.Current.Resources["PlayIconNormal"];
                PlayButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["PlayIconPressed"];

                AudioSingleton.Instance.temp_timer.Stop();
                UpdateTimer.Stop();
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                CurrentTime.Text = "00:00";
                SongSlider.Value = 0;
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }
        }

        private async void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (AudioSingleton.Instance.IsMuted)
            {

                AudioPlayer.AudioPlayer.Instance.setVolume((float)AudioSingleton.Instance.LastVolumeValue);
                VolumeSlider.Value = (double)AudioSingleton.Instance.LastVolumeValue * 100;
                VolumeButton.NormalStateImageSource = new BitmapImage()
                {
                    UriSource = new Uri("ms-appx:" + AudioSingleton.Instance.LastNormalVolume, UriKind.Absolute)
                };
                VolumeButton.HoverStateImageSource = new BitmapImage()
                {
                    UriSource = new Uri("ms-appx:" + AudioSingleton.Instance.LastHoverVolumeImage, UriKind.Absolute)
                };
            }
            else
            {
                AudioSingleton.Instance.LastVolumeValue = AudioPlayer.AudioPlayer.Instance.getVolume();

                AudioPlayer.AudioPlayer.Instance.setVolume(0);
                VolumeSlider.Value = 0;
                VolumeButton.NormalStateImageSource = new BitmapImage()
                {
                    UriSource = new Uri("ms-appx:/Assets/Icons/Mute_Normal.png", UriKind.Absolute)
                };
                VolumeButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["MuteIconHover"];
            }

            AudioSingleton.Instance.IsMuted = !AudioSingleton.Instance.IsMuted;
            try
            {
                AudioSingleton.SaveObject<object>(AudioSingleton.Instance.LastVolumeValue.ToString(), "Volume");
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1);
            }
        }

        private async void ShuffleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ShuffleButton.IsChecked == true)
            {
                AudioSingleton.Instance.IsShuffleEnabled = true;
            }
            else
            {
                AudioSingleton.Instance.IsShuffleEnabled = false;
            }

            await AudioSingleton.SaveObject<object>(AudioSingleton.Instance.IsShuffleEnabled, "Shuffle");
        }

        private async void RepeatButton_Checked(object sender, RoutedEventArgs e)
        {
            if (RepeatButton.IsChecked == true)
            {
                AudioSingleton.Instance.IsRepeatEnabled = true;
            }
            else
            {
                AudioSingleton.Instance.IsRepeatEnabled = false;
            }

            await AudioSingleton.SaveObject<object>(AudioSingleton.Instance.IsRepeatEnabled, "Repeat");

        }

        private async void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ForwardButton.IsEnabled = false;
                ForwardButton.Opacity = 0.3;

                if (AudioSingleton.Instance.IsRepeatEnabled) { }
                else
                {
                    if (AudioSingleton.Instance.IsShuffleEnabled)
                    {
                        Random rnd = new Random();
                        int totalsize = Playlist.Items.Count;
                        int index = rnd.Next(0, totalsize - 1);
                        AudioSingleton.Instance.AddToPreviousSongList(AudioSingleton.Instance.currentSong);
                        AudioSingleton.Instance.IndexValue = index;
                    }
                    else
                    {
                        int currentIndex = AudioSingleton.Instance.IndexValue;
                        AudioSingleton.Instance.AddToPreviousSongList(AudioSingleton.Instance.currentSong);
                        if (currentIndex == Playlist.Items.Count - 1)
                        {
                            AudioSingleton.Instance.IndexValue = 0;
                        }
                        else
                        {
                            AudioSingleton.Instance.IndexValue = AudioSingleton.Instance.IndexValue + 1;
                        }
                    }
                }

                ProgressGrid.Visibility = Visibility.Visible;
                try
                {
                    Playlist.SelectedItem = Playlist.Items[AudioSingleton.Instance.IndexValue];
                    AudioSingleton.Instance.currentSong = Playlist.SelectedItem as StorageFile;
                    Playlist.ScrollIntoView(Playlist.SelectedItem);

                    var prop = await ((StorageFile)Playlist.SelectedItem).Properties.GetMusicPropertiesAsync();
                    SetMusic((StorageFile)Playlist.SelectedItem, prop);
                }
                catch (Exception e_)
                {
                    System.Diagnostics.Debug.WriteLine(e_.StackTrace);
                }

                ProgressGrid.Visibility = Visibility.Collapsed;
                SongChange();
                ForwardButton.IsEnabled = true;
                ForwardButton.Opacity = 0.7;
            }
            catch (Exception)
            {
            }
        }

        private async void RewindButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RewindButton.IsEnabled = false;
                RewindButton.Opacity = 0.3;
                var RetrieveSong = AudioSingleton.Instance.PreviousSongList.ElementAt(AudioSingleton.Instance.PreviousSongList.Count - 1);
                if (AudioSingleton.Instance.PreviousSongList.Count != 1)
                    AudioSingleton.Instance.PreviousSongList.Remove(RetrieveSong);

                ProgressGrid.Visibility = Visibility.Visible;
                try
                {
                    var temp_index = Playlist.Items.ToList().FindIndex(x => x.Equals(RetrieveSong));
                    AudioSingleton.Instance.IndexValue = temp_index;
                    Playlist.SelectedItem = Playlist.Items[temp_index];
                    Playlist.ScrollIntoView(Playlist.SelectedItem);

                    AudioSingleton.Instance.currentSong = RetrieveSong;

                    var prop = await ((StorageFile)Playlist.SelectedItem).Properties.GetMusicPropertiesAsync();
                    SetMusic((StorageFile)Playlist.SelectedItem, prop);
                }
                catch (Exception e_)
                {
                    System.Diagnostics.Debug.WriteLine(e_.StackTrace);
                }

                ProgressGrid.Visibility = Visibility.Collapsed;
                SongChange();
                RewindButton.IsEnabled = true;
                RewindButton.Opacity = 0.7;

            }
            catch (Exception)
            {
            }
        }

        private void DispatchTimerEnable()
        {
            try
            {
                AudioSingleton.Instance.temp_timer.Stop();
                AudioSingleton.Instance.temp_timer.Tick -= AudioSingleton.UpdateTimer_Tick;
            }
            catch (Exception)
            {
            }

            AudioSingleton.Instance.temp_timer = new DispatcherTimer();
            AudioSingleton.Instance.temp_timer.Interval = TimeSpan.FromSeconds(1);
            AudioSingleton.Instance.temp_timer.Tick += AudioSingleton.UpdateTimer_Tick;
            AudioSingleton.Instance.temp_timer.Start();
            AudioSingleton.Instance.ElapsedSeconds = 0;
            EnableLocalTimer();
        }

        private void EnableLocalTimer()
        {
            try
            {
                UpdateTimer.Stop();
                UpdateTimer.Tick -= temp_timer_Tick;
            }
            catch (Exception)
            {
            }

            SongSlider.Maximum = AudioSingleton.Instance.SongTotalSeconds;
            SongSlider.Minimum = 0;
            SongSlider.Value = 0;
            UpdateTimer = new DispatcherTimer();
            UpdateTimer.Interval = TimeSpan.FromMilliseconds(100);
            UpdateTimer.Tick += temp_timer_Tick;
            UpdateTimer.Start();
        }

        void temp_timer_Tick(object sender, object e)
        {
            if (AudioSingleton.Instance.ElapsedSeconds >= AudioSingleton.Instance.SongTotalSeconds)
            {
                (sender as DispatcherTimer).Stop();
                AudioSingleton.Instance.ElapsedSeconds = 0;
                StopButton_Click(null, null);
            }
            else
            {
                SongSlider.Value = AudioSingleton.Instance.ElapsedSeconds;

                DateTime dtime = DateTime.MinValue.Add(TimeSpan.FromSeconds((AudioSingleton.Instance.ElapsedSeconds)));
                CurrentTime.Text = dtime.ToString(@"mm\:ss"); ;
            }
        }

        private async void RestartSongLoading()
        {

            try
            {
                var prop = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();
                SetMusic((StorageFile)Playlist.SelectedItem, prop);

                try
                {
                    if (AudioSingleton.Instance.SongPlaying)
                    {
                        EnableLocalTimer();

                        PlayButton.NormalStateImageSource = (BitmapImage)App.Current.Resources["PauseIconNormal"];
                        PlayButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["PauseIconPressed"];

                        List<TextBlock> temp = FindVisualChildren<TextBlock>(Playlist);
                        try
                        {
                            var block_required = temp.Find(x => x.Text == AudioSingleton.Instance.currentSong.DisplayName);
                            block_required.Foreground = new SolidColorBrush()
                            {
                                Color = Colors.Yellow
                            };
                            AudioSingleton.Instance.previous_textblock = block_required;

                        }
                        catch (Exception e1)
                        {
                            System.Diagnostics.Debug.WriteLine(e1);
                        }
                        StopButton.IsEnabled = true;
                        StopButton.Opacity = 0.7;
                    }
                    else
                    {
                        SongSlider.Value = AudioSingleton.Instance.ElapsedSeconds;

                        DateTime dtime = DateTime.MinValue.Add(TimeSpan.FromSeconds((AudioSingleton.Instance.ElapsedSeconds)));
                        CurrentTime.Text = dtime.ToString(@"mm\:ss"); ;

                        PlayButton.NormalStateImageSource = (BitmapImage)App.Current.Resources["PlayIconNormal"];
                        PlayButton.HoverStateImageSource = (BitmapImage)App.Current.Resources["PlayIconPressed"];

                        StopButton.IsEnabled = false;
                        StopButton.Opacity = 0.3;
                    }
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                }

            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region Visual Children
        public static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }

        public static List<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            List<T> list = new List<T>();
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        list.Add((T)child);
                    }

                    List<T> childItems = FindVisualChildren<T>(child);
                    if (childItems != null && childItems.Count() > 0)
                    {
                        foreach (var item in childItems)
                        {
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }
        #endregion

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MusicGallery), e);
        }

        private void TrixterMode_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(TrixterMode), e);
        }

        void song_changer_Tick(object sender, object e)
        {
            try
            {
                if (AudioSingleton.Instance.currentSong != null)
                {
                    if (AudioSingleton.Instance.SongPlaying)
                    {
                        if (AudioSingleton.Instance.ElapsedSeconds >= AudioSingleton.Instance.SongTotalSeconds)
                        {
                            ForwardButton_Click(null, null);
                        }
                    }
                }
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1);
            }
        }

        private void AppBarButton_Click_3(object sender, RoutedEventArgs e)
        {
            IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                                async (workItem) =>
                                {
                                    var prop = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();
                                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                             CoreDispatcherPriority.High,
                             new DispatchedHandler(async () =>
                             {
                                 LyricsPane.Text = "--- No Lyrics available ---";
                                 LyricsProgress.Visibility = Visibility.Visible;
                             }));
                                    bool first_exception = false;
                                    String lyrics_default = "--- No Lyrics available ---";

                                    try
                                    {
                                        String url = "http://www.azlyrics.com/lyrics/" + prop.Artist.ToLower() + "/" + prop.Title.ToLower() + ".html";

                                        HttpClient temp = new HttpClient();
                                        String result = await temp.GetStringAsync(url);

                                        String temp_ = result.Substring(result.IndexOf("start of lyrics -->") + 19);

                                        temp_ = temp_.Substring(0, temp_.IndexOf("end of lyrics") - 5);
                                        temp_ = temp_.Replace("<br />", "");
                                        temp_ = HtmlRemoval.StripTagsRegex(temp_);
                                        lyrics_default = temp_;

                                    }
                                    catch (Exception e1)
                                    {
                                        System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                                        lyrics_default = "--- No Lyrics available ---";
                                        first_exception = true;
                                    }

                                    if (first_exception)
                                    {
                                        try
                                        {
                                            String url = "http://www.lyricsmode.com/lyrics/" + prop.Artist.ToLower().ToCharArray()[0] + "/" + prop.Artist.ToLower().Replace(" ", "_") + "/" + prop.Title.ToLower().Replace(" ", "_") + ".html";

                                            HttpClient temp = new HttpClient();
                                            String result = await temp.GetStringAsync(url);

                                            String temp_ = result.Substring(result.IndexOf("<p id=\"lyrics_text\" class=\"ui-annotatable\">") + 43);
                                            temp_ = temp_.Substring(0, temp_.IndexOf("<p id=") - 4);
                                            temp_ = temp_.Replace("<br />", "");
                                            temp_ = HtmlRemoval.StripTagsRegex(temp_);
                                            lyrics_default = temp_;
                                        }
                                        catch (Exception e1)
                                        {
                                            System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                                            lyrics_default = "--- No Lyrics available ---";
                                        }
                                    }

                                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                                                CoreDispatcherPriority.High,
                                                                new DispatchedHandler(() =>
                                                                {
                                                                    LyricsPane.Text = lyrics_default;
                                                                    LyricsProgress.Visibility = Visibility.Collapsed;
                                                                }));
                                });
        }

       
        private void VolumeSlider_LostFocus(object sender, RoutedEventArgs e)
        {
            AudioSingleton.Instance.LastVolumeValue = VolumeSlider.Value / 100;
            AudioPlayer.AudioPlayer.Instance.setVolume((float)VolumeSlider.Value / 100);
            SetVolumeImages();

            try
            {
                AudioSingleton.SaveObject<object>(AudioSingleton.Instance.LastVolumeValue.ToString(), "Volume");
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1);

            }
        }
    }
}
