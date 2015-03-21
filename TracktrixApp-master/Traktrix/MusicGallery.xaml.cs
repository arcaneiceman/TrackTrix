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
using Windows.ApplicationModel.Search;
using Windows.Storage;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Traktrix
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MusicGallery : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

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


        public MusicGallery()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
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

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AudioSingleton.Instance.song_changer.Tick += song_changer_Tick;
            AudioSingleton.Instance.song_changer.Start();
            InvalidateSize();
            IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
    async (workItem) =>
    {
        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.High,
                    new DispatchedHandler(() =>
                    {
                        ProgressGrid.Visibility = Visibility.Visible;
                    }));

        while (!AudioSingleton.Instance.AllSongsLoaded)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
        CreateSongs();

        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            CoreDispatcherPriority.High,
            new DispatchedHandler(() =>
            {
                ProgressGrid.Visibility = Visibility.Collapsed;
            }));
    });
            SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
            Window.Current.SizeChanged += Current_SizeChanged;
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AudioSingleton.Instance.song_changer.Stop();
            AudioSingleton.Instance.song_changer.Tick -= song_changer_Tick;
            SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested;
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        private async Task<bool> AddSong(StorageFile value, MusicProperties prop, bool char_edit, string start_character)
        {
            SongItem temp_item = null;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                             CoreDispatcherPriority.High,
                             new DispatchedHandler(() =>
                             {
                                 temp_item = new SongItem();
                             }));

            using (StorageItemThumbnail thumbnail = await value.GetThumbnailAsync(ThumbnailMode.MusicView, 250))
            {
                if (thumbnail != null && thumbnail.Type == ThumbnailType.Image)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                     CoreDispatcherPriority.High,
                                     new DispatchedHandler(() =>
                                     {
                                         var bitmapImage = new BitmapImage();
                                         bitmapImage.SetSource(thumbnail);
                                         temp_item.SetSongItem(value, prop, prop.Title, prop.Artist, prop.Rating, bitmapImage);
                                     }));

                }
                else
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
             CoreDispatcherPriority.High,
             new DispatchedHandler(() =>
             {
                 var bitmapImage = new BitmapImage()
                 {
                     UriSource = new Uri("ms-appx:/Assets/DefaultMusicLogo.jpg", UriKind.Absolute)
                 };

                 temp_item.SetSongItem(value, prop, prop.Title, prop.Artist, prop.Rating, bitmapImage);
             }));

                }
            }

            if (char_edit)
            {
                char_edit = false;
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
         CoreDispatcherPriority.High,
         new DispatchedHandler(() =>
         {
             temp_item.SetAlphabet(start_character);
         }));
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.High,
                new DispatchedHandler(() =>
                {
                    AllSongsGridView.Items.Add(temp_item);
                    AllSongsGridView.InvalidateArrange();
                    AllSongsGridView.InvalidateMeasure();
                }));

            return char_edit;
        }

        private async Task CreateSongs()
        {
            string start_character = "";
            bool char_edit = false;

            foreach (StorageFile value in AudioSingleton.Instance.AllSongs)
            {
                try
                {
                    var prop = await value.Properties.GetMusicPropertiesAsync();
                    string start_char = prop.Title.Substring(0, 1).ToUpper();

                    if (!start_char.Equals(start_character))
                    {
                        start_character = start_char;
                        char_edit = true;
                    }

                    char_edit = await AddSong(value, prop, char_edit, start_character);
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine(e1);
                }
            }
        }

        #region SongSearches
        private void SearchBoxEventsSuggestionsRequested(SearchBox sender, SearchBoxSuggestionsRequestedEventArgs args)
        {

            string queryText = args.QueryText;
            if (!string.IsNullOrEmpty(queryText))
            {
                Windows.ApplicationModel.Search.SearchSuggestionCollection suggestionCollection = args.Request.SearchSuggestionCollection;
                foreach (string suggestion in AudioSingleton.Instance.Suggestions.ToArray())
                {
                    if (suggestion.StartsWith(queryText, StringComparison.CurrentCultureIgnoreCase))
                    {
                        suggestionCollection.AppendQuerySuggestion(suggestion);
                    }
                }
            }

        }

        private async void SongSearch(String QueryText)
        {
            if (QueryText == "")
            {
                IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(async (workItem) =>
                {
                    await CreateSongs();
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            CoreDispatcherPriority.High,
                            new DispatchedHandler(() =>
                            {
                                ProgressGrid.Visibility = Visibility.Collapsed;
                            }));
                });

            }
            else
            {


                foreach (string suggestion in AudioSingleton.Instance.Suggestions.ToArray())
                {
                    if (suggestion.StartsWith(QueryText, StringComparison.CurrentCultureIgnoreCase))
                    {
                        try
                        {
                            var response = AudioSingleton.Instance.AllSongs.Find(e => e.Name.Contains(suggestion));
                            var prop = await response.Properties.GetMusicPropertiesAsync();
                            await AddSong(response, prop, false, "");

                        }
                        catch (Exception e1)
                        {
                            System.Diagnostics.Debug.WriteLine(e1);
                        }
                    }
                    ProgressGrid.Visibility = Visibility.Collapsed;

                }

            }
        }

        private void SearchSong_QueryChanged(SearchBox sender, SearchBoxQueryChangedEventArgs args)
        {
            ProgressGrid.Visibility = Visibility.Visible;
            AllSongsGridView.Items.Clear();
            AllSongsGridView.InvalidateArrange();
            AllSongsGridView.InvalidateMeasure();
            SongSearch(args.QueryText);
        }

        private void SearchSong_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            ProgressGrid.Visibility = Visibility.Visible;
            AllSongsGridView.Items.Clear();
            AllSongsGridView.InvalidateArrange();
            AllSongsGridView.InvalidateMeasure();
            SongSearch(args.QueryText);
        }

        private void SearchSong_LostFocus(object sender, RoutedEventArgs e)
        {
            ProgressGrid.Visibility = Visibility.Visible;
            AllSongsGridView.Items.Clear();
            AllSongsGridView.InvalidateArrange();
            AllSongsGridView.InvalidateMeasure();
            CreateSongs();
            ProgressGrid.Visibility = Visibility.Collapsed;

        }
        #endregion
        private void InvalidateSize()
        {
            var view_mode = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            if (view_mode.IsFullScreen)
            {
                Advertisement.Visibility = Visibility.Visible;
                PageName.Text = "MUSIC GALLERY";
                SearchSong.Width = 300;
            }
            else
            {
                Advertisement.Visibility = Visibility.Collapsed;
                PageName.FontSize = 30;
                PageName.Text = "GALLERY";
                SearchSong.Width = 200;
            }
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            InvalidateSize();
        }

        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs e)
        {
            e.Request.ApplicationCommands.Clear();


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


            SettingsCommand privacy_policy = new SettingsCommand("privacypolicy", "Privacy Policy",
            (handler) =>
            {
                PrivacyPolicy sf = new PrivacyPolicy();
                sf.Show();
            });

            e.Request.ApplicationCommands.Add(privacy_policy);
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
                            AudioPlayer.AudioPlayer.Instance.StopSong();
                            AudioSingleton.Instance.SongPlaying = false;
                            AudioSingleton.Instance.IsPaused = false;
                            AudioSingleton.Instance.temp_timer.Stop();

                            if (AudioSingleton.Instance.IsRepeatEnabled) { }
                            else
                            {
                                if (AudioSingleton.Instance.IsShuffleEnabled)
                                {
                                    Random rnd = new Random();
                                    int totalsize = AudioSingleton.Instance.PlayList.Count;
                                    int index = rnd.Next(0, totalsize - 1);
                                    AudioSingleton.Instance.AddToPreviousSongList(AudioSingleton.Instance.currentSong);
                                    AudioSingleton.Instance.IndexValue = index;

                                }
                                else
                                {
                                    int currentIndex = AudioSingleton.Instance.IndexValue;
                                    AudioSingleton.Instance.AddToPreviousSongList(AudioSingleton.Instance.currentSong);
                                    if (currentIndex == AudioSingleton.Instance.PlayList.Count - 1)
                                    {
                                        AudioSingleton.Instance.IndexValue = 0;
                                    }
                                    else
                                    {
                                        AudioSingleton.Instance.IndexValue = AudioSingleton.Instance.IndexValue + 1;
                                    }
                                }
                            }

                            try
                            {
                                AudioSingleton.Instance.currentSong = AudioSingleton.Instance.PlayList[AudioSingleton.Instance.IndexValue];
                            }
                            catch (Exception e_)
                            {
                                System.Diagnostics.Debug.WriteLine(e_.StackTrace);
                            }

                            try
                            {

                                AudioPlayer.AudioPlayer.Instance.setFilterNo(0);
                                IAsyncAction asyncAction = Windows.System.Threading.ThreadPool.RunAsync(
                                           async (workItem) =>
                                           {
                                               await PlayingPage.PlaySong(AudioSingleton.Instance.currentSong);
                                               AudioSingleton.Instance.SongPlaying = true;
                                           });
                            }
                            catch (Exception e1)
                            {
                                System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                            }
                        }
                    }
                }
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1);

            }
        }

    }
}
