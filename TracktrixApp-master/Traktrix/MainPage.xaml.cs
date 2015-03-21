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
using Windows.UI.Xaml.Media.Animation;
using Traktrix;
using Windows.UI.ApplicationSettings;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.Storage.Search;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Traktrix
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
 
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            var shuffle = await AudioSingleton.LoadObject<object>("Shuffle");
            var repeat = await AudioSingleton.LoadObject<object>("Repeat");
            var volume = await AudioSingleton.LoadObject<object>("Volume");

            if (shuffle == null || repeat == null || volume == null)
            {
                await AudioSingleton.SaveObject<object>(AudioSingleton.Instance.IsShuffleEnabled, "Shuffle");
                await AudioSingleton.SaveObject<object>(AudioSingleton.Instance.IsShuffleEnabled, "Repeat");
                await AudioSingleton.SaveObject<object>("1", "Volume");
                AudioSingleton.Instance.LastVolumeValue = 1;
            }
            else
            {
                AudioSingleton.Instance.LastVolumeValue = Double.Parse((string)volume);
                AudioSingleton.Instance.IsShuffleEnabled = (bool)shuffle;
                AudioSingleton.Instance.IsRepeatEnabled = (bool)repeat;
            }

            var policy_display = await AudioSingleton.LoadObject<bool>("PolicyDisplay");
            if (!policy_display)
            {
                TermsAndConditions.Visibility = Visibility.Visible;
                Help.Visibility = Visibility.Collapsed;
                Important.Visibility = Visibility.Collapsed;
                Help1.Visibility = Visibility.Collapsed;
                Important1.Visibility = Visibility.Collapsed;

                Help.Click -= HelpButton_Click;
                Important.Click -= AppBarButton_Click;
                Help1.Click -= HelpButton_Click;
                Important1.Click -= AppBarButton_Click;
                Grid_Play1.PointerEntered -= Grid_PointerEntered1;
                Grid_Play1.PointerExited -= Grid_PointerExited1;
                Grid_Trixter1.PointerEntered -= Grid_PointerEntered1_1;
                Grid_Trixter1.PointerExited -= Grid_PointerExited1_1;
                Grid_Browse1.PointerEntered -= Grid_PointerEntered1_2;
                Grid_Browse1.PointerExited -= Grid_PointerExited1_2;
                GridPlay.PointerEntered -= Grid_PointerEntered;
                GridPlay.PointerExited -= Grid_PointerExited;
                GridTrixter.PointerEntered -= Grid_PointerEntered_1;
                GridTrixter.PointerExited -= Grid_PointerExited_1;
                GridBrowse.PointerEntered -= Grid_PointerEntered_2;
                GridBrowse.PointerExited -= Grid_PointerExited_2;

            }

            var temp = AudioPlayer.AudioPlayer.Instance;
            InvalidateSize();
//            await AudioSingleton.Instance.GeneratePlaylist();

            if (AudioSingleton.Instance.PlayList.Count == 0)
            {
                MessageDialog temp_dialog = new MessageDialog("It appears to us that there are no songs in the default music library location of your machine. Kindly select 'Proceed' below and folllow the method listed in the help to fix the problem.", "Default location for music library is not set correctly");
                temp_dialog.Commands.Add(new UICommand("Close", new UICommandInvokedHandler(CommandHandler)));
                await temp_dialog.ShowAsync();
            }

            SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;
            Window.Current.SizeChanged += Current_SizeChanged;
            base.OnNavigatedTo(e);
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            InvalidateSize();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested -= onCommandsRequested;
            base.OnNavigatedFrom(e);
        }

        #region Settings
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
        #endregion

        #region Buttons_Handlers
        private void CommandHandler(IUICommand command)
        {
            Help sf = new Help();
            sf.ShowIndependent();
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PlayGridStoryHoverStart.Begin();
            PlayGo.IsEnabled = true;
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            PlayGridStoryHoverStop.Begin();
            PlayGo.IsEnabled = false;
        }

        private void Grid_PointerEntered_1(object sender, PointerRoutedEventArgs e)
        {
            TrixerGridStoryHoverStart.Begin();
            TrixerGo.IsEnabled = true;
        }

        private void Grid_PointerExited_1(object sender, PointerRoutedEventArgs e)
        {
            TrixerGridStoryHoverStop.Begin();
            TrixerGo.IsEnabled = false;
        }

        private void Grid_PointerEntered_2(object sender, PointerRoutedEventArgs e)
        {
            BrowseGridStoryHoverStart.Begin();
            BrowseGo.IsEnabled = true;
        }

        private void Grid_PointerExited_2(object sender, PointerRoutedEventArgs e)
        {
            BrowseGridStoryHoverStop.Begin();
            BrowseGo.IsEnabled = false;
        }

        private void Grid_PointerEntered1(object sender, PointerRoutedEventArgs e)
        {
            PlayGridStoryHoverStart1.Begin();
            PlayGo.IsEnabled = true;
        }

        private void Grid_PointerExited1(object sender, PointerRoutedEventArgs e)
        {
            PlayGridStoryHoverStop1.Begin();
            PlayGo.IsEnabled = false;
        }

        private void Grid_PointerEntered1_1(object sender, PointerRoutedEventArgs e)
        {
            TrixerGridStoryHoverStart1.Begin();
            TrixerGo1.IsEnabled = true;
        }

        private void Grid_PointerExited1_1(object sender, PointerRoutedEventArgs e)
        {
            TrixerGridStoryHoverStop1.Begin();
            TrixerGo1.IsEnabled = false;
        }

        private void Grid_PointerEntered1_2(object sender, PointerRoutedEventArgs e)
        {
            BrowseGridStoryHoverStart1.Begin();
            BrowseGo1.IsEnabled = true;
        }

        private void Grid_PointerExited1_2(object sender, PointerRoutedEventArgs e)
        {
            BrowseGridStoryHoverStop1.Begin();
            BrowseGo1.IsEnabled = false;
        }

        private void PlayGo_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(PlayingPage), e);
        }

        private void TrixerGo_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(TrixterMode), e);
        }

        private void BrowseGo_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MusicGallery), e);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                AboutUs sf = new AboutUs();
                sf.ShowIndependent();
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                Help sf = new Help();
                sf.ShowIndependent();
            }
        }

        #endregion

        private void InvalidateSize()
        {
            var view_mode = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            if (view_mode.IsFullScreen)
            {
                FullScreenAppBar.Visibility = Visibility.Visible;
                FullScreenGrid.Visibility = Visibility.Visible;
                HalfScreenAppBar.Visibility = Visibility.Collapsed;
                HalfScreenGrid.Visibility = Visibility.Collapsed;
                //Advertisement.Visibility = Visibility.Visible;
            }
            else
            {
                FullScreenAppBar.Visibility = Visibility.Collapsed;
                FullScreenGrid.Visibility = Visibility.Collapsed;
                HalfScreenAppBar.Visibility = Visibility.Visible;
                HalfScreenGrid.Visibility = Visibility.Visible;
                //Advertisement.Visibility = Visibility.Collapsed;
            }
        }

        private void PrivacyPolicy_CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private async void PrivacyPolicy_AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            await AudioSingleton.SaveObject<bool>(true, "PolicyDisplay");

            TermsAndConditions.Visibility = Visibility.Collapsed;
            Help.Visibility = Visibility.Visible;
            Important.Visibility = Visibility.Visible;
            Help1.Visibility = Visibility.Visible;
            Important1.Visibility = Visibility.Visible;

            Help.Click += HelpButton_Click;
            Important.Click += AppBarButton_Click;
            Help1.Click += HelpButton_Click;
            Important1.Click += AppBarButton_Click;

            Grid_Play1.PointerEntered += Grid_PointerEntered1;
            Grid_Play1.PointerExited += Grid_PointerExited1;
            Grid_Trixter1.PointerEntered += Grid_PointerEntered1_1;
            Grid_Trixter1.PointerExited += Grid_PointerExited1_1;
            Grid_Browse1.PointerEntered += Grid_PointerEntered1_2;
            Grid_Browse1.PointerExited += Grid_PointerExited1_2;

            GridPlay.PointerEntered += Grid_PointerEntered;
            GridPlay.PointerExited += Grid_PointerExited;
            GridTrixter.PointerEntered += Grid_PointerEntered_1;
            GridTrixter.PointerExited += Grid_PointerExited_1;
            GridBrowse.PointerEntered += Grid_PointerEntered_2;
            GridBrowse.PointerExited += Grid_PointerExited_2;
        }

        private async void ExamplesLink_Click(object sender, RoutedEventArgs e)
        {
            Uri privacyPolicyUrl = new Uri("http://myapppolicy.com/app/tractrix");
            var result = await Windows.System.Launcher.LaunchUriAsync(privacyPolicyUrl);
        }
    }
}
