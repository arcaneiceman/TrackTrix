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
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using System.Net.Http;
using Windows.Storage;
using DemoApp.ViewModels;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.Storage.Streams;
using Traktrix.Filters;
using Traktrix.Audio;
using Windows.Media.Transcoding;
using Windows.Media.MediaProperties;
using DemoApp.Services;
using DemoApp.CoreAudio.Common;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Traktrix
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class TrixterMode : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        DispatcherTimer UpdateTimer;
        //DispatcherTimer GuitarDispatcher;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public RecordViewModel ViewModel
        {
            get
            {
                return DataContext as RecordViewModel;
            }
        }

        public TrixterMode()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            DataContext = new RecordViewModel();
            FiltersGrid.Loaded += FiltersGrid_Loaded;
        }

        void FiltersGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var number = AudioPlayer.AudioPlayer.Instance.getFilterStatus();
            if (number == 0)
            {
                FiltersGrid.SelectedItem = NoFilterGridViewItem;
            }
            else if (number == 1)
            {
                FiltersGrid.SelectedItem = CenterChannelGridViewItem;
            }
            else if (number == 2)
            {
                FiltersGrid.SelectedItem = HighPassGridViewItem;
            }
            else if (number == 3)
            {
                FiltersGrid.SelectedItem = LowPassGridViewItem;
            }
            else if (number == 4)
            {
                FiltersGrid.SelectedItem = HighShelfGridViewItem;
            }
            else if (number == 5)
            {
                FiltersGrid.SelectedItem = LowShelfGridViewItem;
            }
            else if (number == 6)
            {
                FiltersGrid.SelectedItem = BandPassGridViewItem;
            }
            else if (number == 7)
            {
                FiltersGrid.SelectedItem = BandStopGridViewItem;
            }
            else if (number == 8)
            {
                FiltersGrid.SelectedItem = CenterCutGridViewItem;
            }
            else if (number == 9)
            {
                FiltersGrid.SelectedItem = NotchGridViewItem;
            }
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

        #region Music UI Setters
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
                using (var stream = await file.OpenReadAsync())
                {
                    DrawLine(stream.AsStream(), true);
                }
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }

        }

        private void InvalidateSize()
        {
            var view_mode = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            if (view_mode.IsFullScreen)
            {
                Advertisement.Visibility = Visibility.Visible;
                EditedSongPane.Visibility = Visibility.Visible;
                GalleryAppBarButton.Visibility = Visibility.Visible;
                TrixterButton.Visibility = Visibility.Visible;
                FilterAppBarButton.Visibility = Visibility.Visible;
                TitleName.FontSize = 60;
                OriginalSongPane.Width = 600;
                OriginalSong.Width = 600;
                FilterStack.Visibility = Visibility.Visible;
                LyricsStack.Visibility = Visibility.Visible;
                RefreshLyricsButton.Visibility = Visibility.Visible;
                ExtraButtonPane.Visibility = Visibility.Visible;
                VolumeTextBlock.Visibility = Visibility.Visible;
                GuitarTunerTextBlock.Visibility = Visibility.Visible;
                SongName.FontSize = 45;
                KaroakeTextBlock.Visibility = Visibility.Visible;
                AlbumName.FontSize = 35;
                ArtistName.FontSize = 35;
                TotalTime.FontSize = 30;
                CurrentTime.FontSize = 40;
            }
            else
            {
                KaroakeTextBlock.Visibility = Visibility.Collapsed;
                TotalTime.FontSize = 20;
                CurrentTime.FontSize = 30;
                SongName.FontSize = 30;
                AlbumName.FontSize = 20;
                ArtistName.FontSize = 20;
                GuitarTunerTextBlock.Visibility = Visibility.Collapsed;
                VolumeTextBlock.Visibility = Visibility.Collapsed;
                RefreshLyricsButton.Visibility = Visibility.Collapsed;
                Advertisement.Visibility = Visibility.Collapsed;
                EditedSongPane.Visibility = Visibility.Collapsed;
                GalleryAppBarButton.Visibility = Visibility.Collapsed;
                TrixterButton.Visibility = Visibility.Collapsed;
                FilterAppBarButton.Visibility = Visibility.Collapsed;
                TitleName.FontSize = 40;
                OriginalSong.Width = 430;
                FilterStack.Visibility = Visibility.Collapsed;
                LyricsStack.Visibility = Visibility.Collapsed;
                OriginalSongPane.Width = 430;
                ExtraButtonPane.Visibility = Visibility.Collapsed;
            }
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

                if (AudioSingleton.Instance.MixedSongPlaying)
                {
                    AudioSingleton.Instance.MixedSongPlaying = false;
                    SeekEdited.Margin = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    SeekOriginal.Margin = new Thickness(0, 0, 0, 0);
                }
            }
            else
            {
                DateTime dtime = DateTime.MinValue.Add(TimeSpan.FromSeconds((AudioSingleton.Instance.ElapsedSeconds)));
                CurrentTime.Text = dtime.ToString(@"mm\:ss"); ;

                double margin_value = ((double)AudioSingleton.Instance.ElapsedSeconds / AudioSingleton.Instance.SongTotalSeconds) * 600d;
                if (AudioSingleton.Instance.MixedSongPlaying)
                {
                    SeekEdited.Margin = new Thickness(margin_value, 0, 0, 0);
                }
                else
                {
                    SeekOriginal.Margin = new Thickness(margin_value, 0, 0, 0);
                }

            }
        }

        private async void RestartSongLoading()
        {
            try
            {
                var prop = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();
                SetMusic(AudioSingleton.Instance.currentSong, prop);

                try
                {
                    if (AudioSingleton.Instance.SongPlaying)
                    {
                        EnableLocalTimer();

                        PlayButton.NormalStateImageSource = new BitmapImage()
                        {
                            UriSource = new Uri("ms-appx:/Assets/Icons/PauseIcon_Normal.png", UriKind.Absolute)
                        };
                        PlayButton.PressedStateImageSource = new BitmapImage()
                        {
                            UriSource = new Uri("ms-appx:/Assets/Icons/PauseIcon_Normal.png", UriKind.Absolute)
                        };
                        PlayButton.HoverStateImageUriSource = ((BitmapImage)App.Current.Resources["PauseTrixterIconNormal"]).UriSource;

                        StopButton.IsEnabled = true;
                        StopButton.Opacity = 0.7;
                        ListenButton.IsEnabled = false;
                        ListenButton.Opacity = 0.3;
                    }
                    else
                    {
                        DateTime dtime = DateTime.MinValue.Add(TimeSpan.FromSeconds((AudioSingleton.Instance.ElapsedSeconds)));
                        CurrentTime.Text = dtime.ToString(@"mm\:ss"); ;

                        PlayButton.NormalStateImageSource = new BitmapImage()
                        {
                            UriSource = new Uri("ms-appx:/Assets/Icons/PlayIcon_Normal.png", UriKind.Absolute)
                        };
                        PlayButton.PressedStateImageSource = new BitmapImage()
                        {
                            UriSource = new Uri("ms-appx:/Assets/Icons/PlayIcon_Normal.png", UriKind.Absolute)
                        };
                        PlayButton.HoverStateImageUriSource = ((BitmapImage)App.Current.Resources["PlayTrixterIconNormal"]).UriSource;

                        StopButton.IsEnabled = false;
                        StopButton.Opacity = 0.3;
                        ListenButton.IsEnabled = true;
                        StopButton.Opacity = 0.7;
                    }
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                }

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }

        }
        #endregion

        #region Rating Handlers

        private async Task SetRating()
        {

            bool exception = false;
            try
            {
                Star1.IsEnabled = false;
                Star2.IsEnabled = false;
                Star3.IsEnabled = false;
                Star4.IsEnabled = false;
                Star5.IsEnabled = false;

                var prop = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();
                if (prop.Rating == 0)
                {
                    Rating.Width = 2.5 * 24;
                }
                else
                {
                    Rating.Width = prop.Rating * 24;
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

        private async void CallRater(uint Rating)
        {
            try
            {
                Star1.IsEnabled = false;
                Star2.IsEnabled = false;
                Star3.IsEnabled = false;
                Star4.IsEnabled = false;
                Star5.IsEnabled = false;

                var prop = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();
                prop.Rating = Rating;
                await prop.SavePropertiesAsync();
                await SetRating();

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

        #region NavigationHelper registration

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
                InvalidateSize();
                AudioSingleton.Instance.copySong = AudioSingleton.Instance.currentSong;

                try
                {
                    FromBandPass.Text = Filter.BandPassLowerCutOff.ToString();
                    ToBandPass.Text = Filter.BandPassHigherCutOff.ToString();
                    FromBandStop.Text = Filter.BandStopLowerCutOff.ToString();
                    ToBandStop.Text = Filter.BandStopHigherCutOff.ToString();
                    HighPassCutoff.Text = Filter.HighPassCutOff.ToString();
                    LowPassCutoff.Text = Filter.LowPassCutOff.ToString();
                    HighShelfCutoff.Text = Filter.HighShelfCutoff.ToString();
                    HighShelfGain.Text = Filter.HighShelfGain.ToString();
                    LowShelfCutoff.Text = Filter.LowShelfCutoff.ToString();
                    LowShelfGain.Text = Filter.LowShelfGain.ToString();
                    NotchCutoff.Text = Filter.NotchCutoff.ToString();

                    AudioSingleton.Instance.song_changer.Tick += song_changer_Tick;
                    AudioSingleton.Instance.song_changer.Start();
                    RestartSongLoading();

                    if (AudioSingleton.Instance.PlayList.Count == 0)
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

                VolumeSlider.Value = (double)AudioSingleton.Instance.LastVolumeValue * 100;
                SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
                Window.Current.SizeChanged += Current_SizeChanged;
            }
            catch (Exception)
            {
            }
            navigationHelper.OnNavigatedTo(e);
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

        private void CommandHandler(IUICommand command)
        {
            Help sf = new Help();
            sf.ShowIndependent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            try
            {
                AudioSingleton.Instance.GuitarModeEnabled = false;

                try
                {
                    AudioSingleton.Instance.song_changer.Stop();
                    AudioSingleton.Instance.song_changer.Tick -= song_changer_Tick;
                }
                catch (Exception)
                {
                }

                try
                {
                    UpdateTimer.Stop();
                    UpdateTimer.Tick -= temp_timer_Tick;
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine(e1);
                }

                try
                {
                    StopButton_Click(null, null);
                    RecordButton_Click(null, null);
                    NotListenButton_Click(null, null);
                    FilePickerButton.IsEnabled = true;
                }
                catch (Exception e2)
                {
                    System.Diagnostics.Debug.WriteLine(e2);
                }

                SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested;
                Window.Current.SizeChanged -= Current_SizeChanged;
            }
            catch (Exception)
            {
            }
            navigationHelper.OnNavigatedFrom(e);
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            InvalidateSize();
        }
        #endregion

        private void TrixterMode_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(PlayingPage), e);
        }

        private void AppBarButton_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(MusicGallery), e);
            }
            catch (Exception)
            {
            }
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int filter_no = 0;
                var item = FiltersGrid.SelectedItem as GridViewItem;
                if (item.Name.Equals("NoFilterGridViewItem"))
                {
                    filter_no = 0;
                }
                else if (item.Name.Equals("CenterChannelGridViewItem"))
                {
                    filter_no = 1;
                }
                else if (item.Name.Equals("CenterCutGridViewItem"))
                {
                    filter_no = 8;
                }
                else if (item.Name.Equals("BandPassGridViewItem"))
                {
                    filter_no = 6;
                }
                else if (item.Name.Equals("BandStopGridViewItem"))
                {
                    filter_no = 7;
                }
                else if (item.Name.Equals("HighPassGridViewItem"))
                {
                    filter_no = 2;
                }
                else if (item.Name.Equals("HighShelfGridViewItem"))
                {
                    filter_no = 4;
                }
                else if (item.Name.Equals("LowPassGridViewItem"))
                {
                    filter_no = 3;
                }
                else if (item.Name.Equals("LowShelfGridViewItem"))
                {
                    filter_no = 5;
                }
                else
                {
                    filter_no = 9;
                }

                IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                                    (workItem) =>
                                    {
                                        AudioPlayer.AudioPlayer.Instance.setFilterNo(filter_no);
                                    });
            }
            catch (Exception)
            {
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FilterHelp sf = new FilterHelp();
                sf.ShowIndependent();
            }
            catch (Exception)
            {
            }
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AudioSingleton.Instance.isRecording)
                {
                    AudioPlayer.AudioPlayer.Instance.StopRecording();
                    StopButton_Click(new Object(), null);
                    RecordButton.IsEnabled = false;
                    RecordButton.Opacity = 0.3;

                    RenderButton.IsEnabled = true;
                    RenderButton.Opacity = 0.7;
                    StopButton.IsEnabled = false;
                    StopButton.Opacity = 0.3;
                    ListenButton.IsEnabled = true;
                    ListenButton.Opacity = 0.7;
                    PlayButton.IsEnabled = false;
                    PlayButton.Opacity = 0.3;
                }
                else
                {
                    if (AudioSingleton.Instance.SongPlaying)
                    {
                        StopButton_Click(1, null);
                    }

                    FiltersGrid.SelectedItem = CenterChannelGridViewItem;
                    FiltersGrid.ScrollIntoView(CenterChannelGridViewItem);
                    FiltersGrid.IsEnabled = false;

                    AudioPlayer.AudioPlayer.Instance.CreateRecorder();

                    IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                                     async (workItem) =>
                                     {
                                         //CHANGE HERE _____________----------------------_______________-------------
                                         //await PlayingPage.PlaySong(AudioSingleton.Instance.currentSong);
                                         //AudioPlayer.AudioPlayer.Instance.StartRecording();
                                         await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                     CoreDispatcherPriority.High,
                                     new DispatchedHandler(async () =>
                                     {
                                         DispatchTimerEnable();
                                         AudioSingleton.Instance.SongPlaying = true;
                                         PlayButton.NormalStateImageSource = new BitmapImage()
                                         {
                                             UriSource = new Uri("ms-appx:/Assets/Icons/PauseIcon_Normal.png", UriKind.Absolute)
                                         };
                                         PlayButton.HoverStateImageUriSource = ((BitmapImage)App.Current.Resources["PauseTrixterIconNormal"]).UriSource;

                                         await AudioPlayer.AudioPlayer.Instance.StartRecording();

                                         RenderButton.IsEnabled = false;
                                         RenderButton.Opacity = 0.3;
                                         StopButton.IsEnabled = false;
                                         StopButton.Opacity = 0.3;
                                         PlayButton.IsEnabled = false;
                                         PlayButton.Opacity = 0.3;
                                         FilePickerButton.IsEnabled = false;
                                         ListenButton.IsEnabled = false;
                                         ListenButton.Opacity = 0.3;

                                         if (SaveButton.IsEnabled == true)
                                         {
                                             SaveButton.IsEnabled = false;
                                             SaveButton.Opacity = 0.3;
                                         }
                                         if (AudioSingleton.Instance.currentSong == null)
                                         {
                                             RecordButton.IsEnabled = false;
                                             RecordButton.Opacity = 0.3;
                                         }
                                         else
                                         {
                                             try
                                             {
                                                 DispatchTimerEnable();
                                                 AudioSingleton.Instance.SongPlaying = true;
                                                 PlayButton.NormalStateImageSource = new BitmapImage()
                                                 {
                                                     UriSource = new Uri("ms-appx:/Assets/Icons/PauseIcon_Normal.png", UriKind.Absolute)
                                                 };
                                                 PlayButton.HoverStateImageUriSource = ((BitmapImage)App.Current.Resources["PauseTrixterIconNormal"]).UriSource;
                                                 RecordButton.IsEnabled = true;
                                                 RecordButton.Opacity = 0.7;
                                                 ListenButton.IsEnabled = false;
                                                 ListenButton.Opacity = 0.3;
                                             }
                                             catch (Exception e1)
                                             {
                                                 RecordButton.IsEnabled = false;
                                                 RecordButton.Opacity = 0.3;
                                                 System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                                             }
                                         }
                                     }));
                                     });


                }

                AudioSingleton.Instance.isRecording = !AudioSingleton.Instance.isRecording;
            }
            catch (Exception)
            {
            }
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SaveButton.IsEnabled == true)
                {
                    SaveButton.IsEnabled = false;
                    SaveButton.Opacity = 0.3;
                }

                StopButton.IsEnabled = false;
                StopButton.Opacity = 0.3;
                ListenButton.IsEnabled = true;
                ListenButton.Opacity = 0.7;

                AudioPlayer.AudioPlayer.Instance.StopSong();

                if (sender != null)
                {
                    AudioSingleton.Instance.SongPlaying = false;
                    AudioSingleton.Instance.IsPaused = false;
                }

                PlayButton.NormalStateImageSource = new BitmapImage()
                {
                    UriSource = new Uri("ms-appx:/Assets/Icons/PlayIcon_Normal.png", UriKind.Absolute)
                };
                PlayButton.HoverStateImageUriSource = ((BitmapImage)App.Current.Resources["PlayTrixterIconNormal"]).UriSource;

                AudioSingleton.Instance.temp_timer.Stop();
                UpdateTimer.Stop();
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                CurrentTime.Text = "00:00";

                if (AudioSingleton.Instance.MixedSongPlaying)
                {
                    AudioSingleton.Instance.MixedSongPlaying = false;
                    SeekEdited.Margin = new Thickness(0, 0, 0, 0);
                }
                else
                {
                    SeekOriginal.Margin = new Thickness(0, 0, 0, 0);
                }

                FilePickerButton.IsEnabled = true;

            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }
        }

        private void PlayButton_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SaveButton.IsEnabled == true)
                {
                    SaveButton.IsEnabled = false;
                    SaveButton.Opacity = 0.3;
                }
                if (AudioSingleton.Instance.currentSong == null)
                {
                    RecordButton.IsEnabled = false;
                    RecordButton.Opacity = 0.3;
                }
                else
                {
                    FilePickerButton.IsEnabled = false;
                    PlayButton.IsEnabled = false;
                    PlayButton.Opacity = 0.3;
                    try
                    {
                        if (!AudioSingleton.Instance.SongPlaying)
                        {
                            AudioPlayer.AudioPlayer.Instance.setFilterNo(0);
                            FiltersGrid.SelectedItem = NoFilterGridViewItem;
                            FiltersGrid.ScrollIntoView(NoFilterGridViewItem);
                            FiltersGrid.IsEnabled = true;

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
                                PlayButton.IsEnabled = true;
                                PlayButton.Opacity = 0.7;
                                RecordButton.IsEnabled = true;
                                RecordButton.Opacity = 0.7;
                                StopButton.IsEnabled = true;
                                StopButton.Opacity = 0.7;
                                ListenButton.IsEnabled = false;
                                ListenButton.Opacity = 0.3;
                                RenderButton.IsEnabled = false;
                                RenderButton.Opacity = 0.3;
                                PlayButton.NormalStateImageSource = new BitmapImage()
                                {
                                    UriSource = new Uri("ms-appx:/Assets/Icons/PauseIcon_Normal.png", UriKind.Absolute)
                                };
                                PlayButton.HoverStateImageUriSource = ((BitmapImage)App.Current.Resources["PauseTrixterIconNormal"]).UriSource;
                            }
                            else
                            {
                                IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                                    async (workItem) =>
                                    {
                                        await PlayingPage.PlaySong(AudioSingleton.Instance.currentSong);
                                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                    CoreDispatcherPriority.High,
                                    new DispatchedHandler(() =>
                                    {
                                        DispatchTimerEnable();
                                        AudioSingleton.Instance.SongPlaying = true;
                                        PlayButton.NormalStateImageSource = new BitmapImage()
                                        {
                                            UriSource = new Uri("ms-appx:/Assets/Icons/PauseIcon_Normal.png", UriKind.Absolute)
                                        };
                                        PlayButton.HoverStateImageUriSource = ((BitmapImage)App.Current.Resources["PauseTrixterIconNormal"]).UriSource;
                                        PlayButton.IsEnabled = true;
                                        PlayButton.Opacity = 0.7;
                                        RecordButton.IsEnabled = true;
                                        RecordButton.Opacity = 0.7;
                                        StopButton.IsEnabled = true;
                                        StopButton.Opacity = 0.7;
                                        ListenButton.IsEnabled = false;
                                        ListenButton.Opacity = 0.3;
                                        RenderButton.IsEnabled = false;
                                        RenderButton.Opacity = 0.3;

                                    }));

                                    });
                            }

                        }
                        else
                        {
                            IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
            (workItem) =>
            {
                AudioSingleton.Instance.SongPlaying = false;
                AudioSingleton.Instance.IsPaused = true;
                AudioPlayer.AudioPlayer.Instance.PauseSong();
            });

                            PlayButton.NormalStateImageSource = new BitmapImage()
                            {
                                UriSource = new Uri("ms-appx:/Assets/Icons/PlayIcon_Normal.png", UriKind.Absolute)
                            };
                            PlayButton.HoverStateImageUriSource = ((BitmapImage)App.Current.Resources["PlayTrixterIconNormal"]).UriSource;
                            UpdateTimer.Tick -= temp_timer_Tick;
                            AudioSingleton.Instance.temp_timer.Tick -= AudioSingleton.UpdateTimer_Tick;

                            RecordButton.IsEnabled = false;
                            RecordButton.Opacity = 0.3;

                            PlayButton.IsEnabled = true;
                            PlayButton.Opacity = 0.7;
                            StopButton.IsEnabled = true;
                            StopButton.Opacity = 0.7;
                            ListenButton.IsEnabled = false;
                            ListenButton.Opacity = 0.3;

                        }
                    }
                    catch (Exception e1)
                    {
                        RecordButton.IsEnabled = false;
                        RecordButton.Opacity = 0.3;
                        System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                    }
                }
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

        #region Volume
        private void VolumeButton_Click(object sender, RoutedEventArgs e)
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
        #endregion


        private void RenderButton_Click_1(object sender, RoutedEventArgs e)
        {

            try
            {
                ProgressGrid.Visibility = Visibility.Visible;

                IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
        async (workItem) =>
        {
            // CHANGE HERE>>>>>>>> THE BOTTOM LINE WAS CAUSEING IT TO CRASH< <<< NOW ITS NOT A TASK! its just a fucnction
            await AudioPlayer.AudioPlayer.Instance.RenderAudio();
            AudioSingleton.Instance.SongPlaying = true;
            AudioSingleton.Instance.MixedSongPlaying = true;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                               CoreDispatcherPriority.High,
                               new DispatchedHandler(async () =>
                               {
                                   DrawLine(AudioSingleton.Instance.mixed_song_stream.AsStream(), false);
                                   DispatchTimerEnable();
                                   ProgressGrid.Visibility = Visibility.Collapsed;
                                   RenderButton.IsEnabled = false;
                                   RenderButton.Opacity = 0.3;
                                   StopButton.IsEnabled = true;
                                   StopButton.Opacity = 0.7;
                                   PlayButton.IsEnabled = true;
                                   PlayButton.Opacity = 0.7;
                                   SaveButton.IsEnabled = true;
                                   SaveButton.Opacity = 0.7;
                                   ListenButton.IsEnabled = false;
                                   ListenButton.Opacity = 0.3;

                                   MessageDialog temp_dialog = new MessageDialog("The song has successfully rendered and will play automatically. Click the 'Save' button to save the song to Music Library.", "Rendering Successful");
                                   await temp_dialog.ShowAsync();
                               }));

        });

                try
                {
                    AudioPlayer.AudioPlayer.Instance.setFilterNo(0);
                    FiltersGrid.SelectedItem = NoFilterGridViewItem;
                    FiltersGrid.ScrollIntoView(NoFilterGridViewItem);
                    FiltersGrid.IsEnabled = true;

                    //IAsyncAction asyncAction2 = Windows.System.Threading.ThreadPool.RunAsync(
                    //                async (workItem) =>
                    //                {
                    //                    await PlayingPage.PlaySong(AudioSingleton.Instance.mixed_song_file);
                    //                    AudioSingleton.Instance.SongPlaying = true;
                    //                });

                    // EnableLocalTimer();
                    //                    DispatchTimerEnable();

                    PlayButton.NormalStateImageSource = new BitmapImage()
                    {
                        UriSource = new Uri("ms-appx:/Assets/Icons/PauseIcon_Normal.png", UriKind.Absolute)
                    };
                    PlayButton.HoverStateImageUriSource = ((BitmapImage)App.Current.Resources["PauseTrixterIconNormal"]).UriSource;

                    StopButton.IsEnabled = true;
                    StopButton.Opacity = 0.7;
                    ListenButton.IsEnabled = false;
                    ListenButton.Opacity = 0.3;

                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                }

            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1);
                ProgressGrid.Visibility = Visibility.Collapsed;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog temp_dialog = new MessageDialog("Do you wish to save the song?");
            temp_dialog.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(CommandHandler_Save), 1));
            temp_dialog.Commands.Add(new UICommand("No", new UICommandInvokedHandler(CommandHandler_Save), 2));
            await temp_dialog.ShowAsync();
        }

        private async void CommandHandler_Save(IUICommand command)
        {
            try
            {
                if (command.Id.Equals(1))
                {
                    bool exception = false;
                    try
                    {
                        var prop1 = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();
                        IRandomAccessStream lol = AudioSingleton.Instance.mixed_song_stream;
                        lol.AsStream();
                        var tempWriter = new Audio.MyWavWriter();
                        await tempWriter.Begin(lol.AsStream(), prop1.Title, (int)lol.AsStream().Length, 2, 44100);
                        AudioSingleton.Instance.mixed_song_stream.AsStream().Position = 0;

                        //StorageFile _file = await KnownFolders.MusicLibrary.CreateFileAsync(prop1.Title + "_tractrix.mp3", CreationCollisionOption.GenerateUniqueName);

                        //MediaTranscoder temp_transcoder = new MediaTranscoder();
                        //MediaEncodingProfile profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Medium);
                        //var preparedTranscodeResult = await temp_transcoder.PrepareFileTranscodeAsync(AudioSingleton.Instance.mixed_song_file, _file, profile);
                        //await preparedTranscodeResult.TranscodeAsync();

                        lol.Dispose();

                        AudioSingleton.Instance.PlayList.Add(AudioSingleton.Instance.mixed_song_file);
                        SaveButton.Opacity = 0.3;
                        SaveButton.IsEnabled = false;

                        MessageDialog temp_dialog = new MessageDialog("You have successfully saved the song and it has been added to your playlist.");
                        await temp_dialog.ShowAsync();
                    }
                    catch (Exception e1)
                    {
                        System.Diagnostics.Debug.WriteLine(e1);
                        exception = true;
                    }

                    if (exception)
                    {
                        MessageDialog temp_dialog = new MessageDialog("Song could not be saved due to some error. Please try mixing the song again.");
                        await temp_dialog.ShowAsync();
                    }
                }
            }
            catch (Exception)
            {
            }
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
                            StopButton_Click(1, null);
                        }
                    }
                }
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1);
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            var item = sender as TextBox;
            try
            {
                if (item.Name.Equals("FromBandPass"))
                {
                    Filter.BandPassLowerCutOff = Int32.Parse(FromBandPass.Text);
                }
                else if (item.Name.Equals("ToBandPass"))
                {
                    Filter.BandPassHigherCutOff = Int32.Parse(ToBandPass.Text);
                }
                else if (item.Name.Equals("FromBandStop"))
                {
                    Filter.BandStopLowerCutOff = Int32.Parse(FromBandStop.Text);
                }
                else if (item.Name.Equals("ToBandStop"))
                {
                    Filter.BandStopHigherCutOff = Int32.Parse(ToBandStop.Text);
                }
                else if (item.Name.Equals("HighPassCutoff"))
                {
                    Filter.HighPassCutOff = Int32.Parse(HighPassCutoff.Text);
                }
                else if (item.Name.Equals("LowPassCutoff"))
                {
                    Filter.LowPassCutOff = Int32.Parse(LowPassCutoff.Text);
                }
                else if (item.Name.Equals("HighShelfCutoff"))
                {
                    Filter.HighShelfCutoff = Int32.Parse(HighShelfCutoff.Text);
                }
                else if (item.Name.Equals("HighShelfGain"))
                {
                    Filter.HighShelfGain = Int32.Parse(HighShelfGain.Text);
                }
                else if (item.Name.Equals("LowShelfCutoff"))
                {
                    Filter.LowShelfCutoff = Int32.Parse(LowShelfCutoff.Text);
                }
                else if (item.Name.Equals("LowShelfGain"))
                {
                    Filter.LowShelfGain = Int32.Parse(LowShelfGain.Text);
                }
                else if (item.Name.Equals("NotchCutoff"))
                {
                    Filter.NotchCutoff = Int32.Parse(NotchCutoff.Text);
                }
            }
            catch (Exception)
            {
                item.Text = "0";
            }
        }

        private async void AppBarButton_Click_2(object sender, RoutedEventArgs e)
        {
            bool exception = false;
            try
            {
                if (AudioSingleton.Instance.SongPlaying)
                {
                    StopButton_Click(1, null);
                }

                Windows.Storage.Pickers.FileOpenPicker picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
                picker.FileTypeFilter.Add(".mp3");
                picker.FileTypeFilter.Add(".wav");
                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    AudioSingleton.Instance.currentSong = file;

                    PlayButton_Click_1(null, null);
                    var prop = await AudioSingleton.Instance.currentSong.Properties.GetMusicPropertiesAsync();
                    SetMusic(AudioSingleton.Instance.currentSong, prop);

                    //IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                    //            async (workItem) =>
                    //            {
                    //                //await PlayingPage.PlaySong(AudioSingleton.Instance.currentSong);
                    //                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    //            CoreDispatcherPriority.High,
                    //            new DispatchedHandler(() =>
                    //            {

                    //                //DispatchTimerEnable();
                    //                AudioSingleton.Instance.SongPlaying = true;
                    //                PlayButton.NormalStateImageSource = new BitmapImage()
                    //                {
                    //                    UriSource = new Uri("ms-appx:/Assets/Icons/PauseIcon_Normal.png", UriKind.Absolute)
                    //                };
                    //                PlayButton.HoverStateImageUriSource = ((BitmapImage)App.Current.Resources["PauseTrixterIconNormal"]).UriSource;
                    //                PlayButton.IsEnabled = true;
                    //                PlayButton.Opacity = 0.7;
                    //                RecordButton.IsEnabled = true;
                    //                RecordButton.Opacity = 0.7;
                    //                StopButton.IsEnabled = true;
                    //                StopButton.Opacity = 0.7;
                    //                ListenButton.IsEnabled = false;
                    //                ListenButton.Opacity = 0.3;

                    //            }));

                    //            });
                }
            }
            catch (Exception e1)
            {
                exception = true;
                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
            }

            if (exception)
            {
                MessageDialog temp_dialog = new MessageDialog("The file wasn't selected properly. Please ensure the song you are trying to select is in the Music Library or refer 'Help' for further information.");
                await temp_dialog.ShowAsync();
            }
        }

        bool stopbutton_ = false;
        double stopbutton_value = 0.3;
        bool playbutton_ = true;
        double playbutton_value = 0.7;
        bool recordbutton_ = false;
        double recordbutton_value = 0.3;
        bool renderbutton_ = false;
        double renderbutton_value = 0.3;
        bool savebutton_ = false;
        double savebutton_value = 0.3;
        bool filtersgrid_ = true;

        private void ListenButton_Click(object sender, RoutedEventArgs e)
        {
            AudioSingleton.Instance.GuitarModeEnabled = true;
            AudioSingleton.Instance._isListening = true;
            //GuitarDispatcher = new DispatcherTimer();
            //GuitarDispatcher.Interval = TimeSpan.FromMilliseconds(200);
            //GuitarDispatcher.Tick += GuitarDispatcher_Tick;
            //GuitarDispatcher.Start();
            AudioPlayer.AudioPlayer.Instance.GTListen();
            NotListenButton.IsEnabled = true;
            NotListenButton.Opacity = 0.7;
            ListenButton.IsEnabled = false;
            ListenButton.Opacity = 0.3;

            stopbutton_ = StopButton.IsEnabled;
            StopButton.IsEnabled = false;
            stopbutton_value = StopButton.Opacity;
            StopButton.Opacity = 0.3;

            playbutton_ = PlayButton.IsEnabled;
            PlayButton.IsEnabled = false;
            playbutton_value = PlayButton.Opacity;
            PlayButton.Opacity = 0.3;

            recordbutton_ = RecordButton.IsEnabled;
            RecordButton.IsEnabled = false;
            recordbutton_value = RecordButton.Opacity;
            RecordButton.Opacity = 0.3;

            filtersgrid_ = FiltersGrid.IsEnabled;
            FiltersGrid.IsEnabled = false;

            renderbutton_ = RenderButton.IsEnabled;
            RenderButton.IsEnabled = false;
            renderbutton_value = RenderButton.Opacity;
            RenderButton.Opacity = 0.3;

            savebutton_ = SaveButton.IsEnabled;
            SaveButton.IsEnabled = false;
            savebutton_value = SaveButton.Opacity;
            SaveButton.Opacity = 0.3;
        }

        void GuitarDispatcher_Tick(object sender, object e)
        {

            try
            {
                TunerSlider.Value = Double.Parse(GuitarTuner.closestFrequency);
                NoteAlphabet.Text = GuitarTuner.noteName;
                NoteValue.Text = GuitarTuner.closestFrequency;
            }
            catch (Exception)
            {

            }
        }

        private async void NotListenButton_Click(object sender, RoutedEventArgs e)
        {
            AudioSingleton.Instance._isListening = false;
            StopButton.IsEnabled = stopbutton_;
            StopButton.Opacity = stopbutton_value;

            PlayButton.IsEnabled = playbutton_;
            PlayButton.Opacity = playbutton_value;

            RecordButton.IsEnabled = recordbutton_;
            RecordButton.Opacity = recordbutton_value;

            FiltersGrid.IsEnabled = filtersgrid_;

            RenderButton.IsEnabled = renderbutton_;
            RenderButton.Opacity = renderbutton_value;

            SaveButton.IsEnabled = savebutton_;
            SaveButton.Opacity = savebutton_value;

            //try
            //{
            //    GuitarDispatcher.Stop();
            //    GuitarDispatcher.Tick -= GuitarDispatcher_Tick;
            //}
            //catch (Exception e1)
            //{
            //    System.Diagnostics.Debug.WriteLine(e1);
            //}

            await AudioPlayer.AudioPlayer.Instance.GTStop();
            ListenButton.IsEnabled = true;
            ListenButton.Opacity = 0.7;
            NotListenButton.IsEnabled = false;
            NotListenButton.Opacity = 0.3;
            AudioSingleton.Instance.GuitarModeEnabled = false;
            try
            {
                TunerSlider.Value = Double.Parse(GuitarTuner.closestFrequency);
                NoteAlphabet.Text = GuitarTuner.noteName;
                NoteValue.Text = GuitarTuner.closestFrequency;
            }
            catch (Exception)
            {
            }
        }

        public void DrawLine(Stream sstream, bool OriginalSongBool)
        {
            sstream.Position = 0;
            if (OriginalSongBool)
            {
                OriginalSong.Points.Clear();
            }
            else
            {
                EditedSong.Points.Clear();
            }

            sstream.Position = 0;
            byte[] temparray = new byte[sstream.Length];
            sstream.Read(temparray, 0, (int)sstream.Length);
            double[] values = Filters.Filter.BytesToDoubles(temparray);
            int XScaleFactor = (int)values.Length / 600;
            double biggest = 0;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > biggest)
                {
                    biggest = values[i];
                }
            }

            int yvalue;
            int xvalue = 0;
            for (int x = 0; x < values.Length; x = x + XScaleFactor)
            {
                //lower y value will always be the same 
                // x value will be x 
                yvalue = (int)((80d * values[x]) / biggest);
                if (OriginalSongBool)
                {
                    OriginalSong.Points.Add(new Point(xvalue, yvalue + 75));
                }
                else
                {
                    EditedSong.Points.Add(new Point(xvalue, yvalue + 75));
                }
                xvalue++;
            }
            sstream.Position = 0;

            if (OriginalSongBool)
            {
                if (OriginalSong.Points.Count > 600)
                {
                    while (OriginalSong.Points.Count != 600)
                    {
                        OriginalSong.Points.RemoveAt(OriginalSong.Points.Count - 1);
                    }
                }
            }
            else
            {
                if (EditedSong.Points.Count > 600)
                {
                    while (EditedSong.Points.Count != 600)
                    {
                        EditedSong.Points.RemoveAt(EditedSong.Points.Count - 1);
                    }
                }
            }
        }

        private void AppBarButton_Click_3(object sender, RoutedEventArgs e)
        {
            IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                                async (workItem) =>
                                {
                                    try
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
                                    }
                                    catch (Exception)
                                    {
                                    }
                                });
        }

        private void VolumeSlider_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                AudioSingleton.Instance.LastVolumeValue = VolumeSlider.Value / 100;
                AudioPlayer.AudioPlayer.Instance.setVolume((float)VolumeSlider.Value / 100);
                SetVolumeImages();

                AudioSingleton.SaveObject<object>(AudioSingleton.Instance.LastVolumeValue.ToString(), "Volume");
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1);
            }
        }
    }
}
