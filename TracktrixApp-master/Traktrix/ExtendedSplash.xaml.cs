using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Traktrix.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Traktrix
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExtendedSplash : Page
    {
        public ExtendedSplash()
        {
            this.InitializeComponent();
            this.ProgressGrid.Loaded += ProgressGrid_Loaded;

        }

       async void ProgressGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var color_name = await AudioSingleton.LoadObject<object>("ColorName");
            var background_name = await AudioSingleton.LoadObject<object>("BackgroundName");

            if (color_name == null || background_name == null)
            {
                await AudioSingleton.SaveObject<object>("Orange", "ColorName");
                await AudioSingleton.SaveObject<object>("Eight", "BackgroundName");
            }
            else
            {
                ThemeSet((string)color_name, (string)background_name);
            }

            var temp = AudioPlayer.AudioPlayer.Instance;
            ColorChangeStart.Begin();
            List<StorageFile> temp_songs = await AudioSingleton.LoadList<List<StorageFile>>("AllSongs");
            List<StorageFile> temp_playlist = await AudioSingleton.LoadList<List<StorageFile>>("Playlist");
            
            await AudioSingleton.Instance.GeneratePlaylist();

            //if (temp_songs == default(List<StorageFile>) || temp_playlist == default(List<StorageFile>))
            //{
            //    await AudioSingleton.Instance.GeneratePlaylist();
            //}
            //else
            //{
            //    int counter = 0;
            //    StorageFolder musicFolder = KnownFolders.MusicLibrary;
            //    StorageFolderQueryResult queryresult = musicFolder.CreateFolderQuery(CommonFolderQuery.DefaultQuery);

            //    IReadOnlyList<StorageFolder> folderList = await queryresult.GetFoldersAsync();

            //    foreach (StorageFolder folder in folderList)
            //    {
            //        IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();

            //        foreach (StorageFile file in fileList)
            //        {
            //            if (file.FileType.Equals(".mp3"))
            //            {
            //                counter++;
            //            }
            //        }
            //    }

            //    IReadOnlyList<StorageFile> rootList = await KnownFolders.MusicLibrary.GetFilesAsync();

            //    foreach (StorageFile file in rootList)
            //    {
            //        if (file.FileType.Equals(".mp3"))
            //        {
            //            counter++;
            //        }
            //    }


            //    if (counter == temp_songs.Count)
            //    {
            //        AudioSingleton.Instance.PlayList = temp_playlist;
            //        AudioSingleton.Instance.AllSongs = temp_songs;
            //    }
            //    else
            //    {
            //        await AudioSingleton.Instance.GeneratePlaylist();
            //    }
            //}

           // await Task.Delay(3000);
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage), e);
        }

       private void ThemeSet(String colorname, String BackgroundName)
       {
           Windows.UI.Color color;
           Windows.UI.Color secondary_color;
           Windows.UI.Color tertiary_color;

           if (colorname.Equals("Blue"))
           {
               color = Windows.UI.Color.FromArgb(255, 27, 161, 226);
               secondary_color = Windows.UI.Color.FromArgb(153, 27, 161, 226);
               tertiary_color = Windows.UI.Color.FromArgb(102, 27, 161, 226);
           }
           else if (colorname.Equals("Red"))
           {
               color = Windows.UI.Color.FromArgb(255, 229, 20, 0);
               secondary_color = Windows.UI.Color.FromArgb(153, 229, 20, 0);
               tertiary_color = Windows.UI.Color.FromArgb(102, 229, 20, 0);
           }
           else if (colorname.Equals("Green"))
           {
               color = Windows.UI.Color.FromArgb(255, 164, 196, 0);
               secondary_color = Windows.UI.Color.FromArgb(153, 164, 196, 0);
               tertiary_color = Windows.UI.Color.FromArgb(102, 164, 196, 0);
           }
           else if (colorname.Equals("Gray"))
           {
               color = Windows.UI.Color.FromArgb(255, 157, 157, 157);
               secondary_color = Windows.UI.Color.FromArgb(153, 157, 157, 157);
               tertiary_color = Windows.UI.Color.FromArgb(102, 157, 157, 157);
           }
           else if (colorname.Equals("Orange"))
           {
               color = Windows.UI.Color.FromArgb(255, 250, 104, 0);
               secondary_color = Windows.UI.Color.FromArgb(153, 250, 104, 0);
               tertiary_color = Windows.UI.Color.FromArgb(102, 250, 104, 0);
           }
           else
           {
               color = Windows.UI.Color.FromArgb(255, 240, 163, 10);
               secondary_color = Windows.UI.Color.FromArgb(153, 240, 163, 10);
               tertiary_color = Windows.UI.Color.FromArgb(102, 240, 163, 10);
           }

           ((TextBlock)App.Current.Resources["ColorName"]).Text = colorname;
           ((SolidColorBrush)App.Current.Resources["OrangeColor"]).Color = color;
           ((SolidColorBrush)App.Current.Resources["SearchBoxHitHighlightSelectedForegroundThemeBrush"]).Color = secondary_color;
           ((SolidColorBrush)App.Current.Resources["SearchBoxBorderThemeBrush"]).Color = tertiary_color;

           ((BitmapImage)App.Current.Resources["MainBackgroundImage"]).UriSource = new Uri("ms-appx:/Assets/Wallpapers/" + BackgroundName + ".jpg", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["Logo"]).UriSource = new Uri("ms-appx:/Assets/Logo_" + colorname + ".jpg", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["BackgroundImage"]).UriSource = new Uri("ms-appx:/Assets/Background_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["StopIconHover"]).UriSource = new Uri("ms-appx:/Assets/Icons/Stop_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["ShuffleIconHover"]).UriSource = new Uri("ms-appx:/Assets/Icons/Shuffle_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["RepeatIconHover"]).UriSource = new Uri("ms-appx:/Assets/Icons/Repeat_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["MuteIconHover"]).UriSource = new Uri("ms-appx:/Assets/Icons/Mute_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["Volume0IconHover"]).UriSource = new Uri("ms-appx:/Assets/Icons/Volume0_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["Volume1IconHover"]).UriSource = new Uri("ms-appx:/Assets/Icons/Volume1_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["Volume2IconHover"]).UriSource = new Uri("ms-appx:/Assets/Icons/Volume2_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["Volume3IconHover"]).UriSource = new Uri("ms-appx:/Assets/Icons/Volume3_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["PlayIconNormal"]).UriSource = new Uri("ms-appx:/Assets/Icons/Play_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["PlayIconPressed"]).UriSource = new Uri("ms-appx:/Assets/Icons/Play_Pressed_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["PauseIconNormal"]).UriSource = new Uri("ms-appx:/Assets/Icons/Pause_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["PauseIconPressed"]).UriSource = new Uri("ms-appx:/Assets/Icons/Pause_Pressed_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["RewindIconNormal"]).UriSource = new Uri("ms-appx:/Assets/Icons/Rewind_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["ForwardIconNormal"]).UriSource = new Uri("ms-appx:/Assets/Icons/Forward_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["PlayTrixterIconNormal"]).UriSource = new Uri("ms-appx:/Assets/Icons/PlayIcon_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["PauseTrixterIconNormal"]).UriSource = new Uri("ms-appx:/Assets/Icons/PauseIcon_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["RecordIconNormal"]).UriSource = new Uri("ms-appx:/Assets/Icons/Record_Normal_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["RenderIconNormal"]).UriSource = new Uri("ms-appx:/Assets/Icons/DJ_Icon_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["SaveIconNormal"]).UriSource = new Uri("ms-appx:/Assets/Icons/save_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["ListenIcon"]).UriSource = new Uri("ms-appx:/Assets/Icons/Listen_" + colorname + ".png", UriKind.Absolute);
           ((BitmapImage)App.Current.Resources["NotListenIcon"]).UriSource = new Uri("ms-appx:/Assets/Icons/NotListen_" + colorname + ".png", UriKind.Absolute);

       }
  

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
    }
}
