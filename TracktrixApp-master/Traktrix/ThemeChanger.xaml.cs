using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Traktrix.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class ThemeChanger : SettingsFlyout
    {
        public ThemeChanger()
        {
            this.InitializeComponent();
            BackgroundSelector.Loaded += BackgroundSelector_Loaded;
            ThemeSelector.Loaded += ThemeSelector_Loaded;
        }

       async void BackgroundSelector_Loaded(object sender, RoutedEventArgs e)
        {
            var colorname = await AudioSingleton.LoadObject<object>("BackgroundName");
            if (colorname.Equals("One"))
            {
                BackgroundSelector.SelectedItem = One;
            }
            else if (colorname.Equals("Two"))
            {
                BackgroundSelector.SelectedItem = Two;
            }
            else if (colorname.Equals("Three"))
            {
                BackgroundSelector.SelectedItem = Three;
            }
            else if (colorname.Equals("Four"))
            {
                BackgroundSelector.SelectedItem = Four;
            }
            else if (colorname.Equals("Five"))
            {
                BackgroundSelector.SelectedItem = Five;
            }
            else if (colorname.Equals("Six"))
            {
                BackgroundSelector.SelectedItem = Six;
            }
            else if (colorname.Equals("Eight"))
            {
                BackgroundSelector.SelectedItem = Eight;
            }
            else if (colorname.Equals("Seven"))
            {
                BackgroundSelector.SelectedItem = Seven;
            }
            else if (colorname.Equals("Nine"))
            {
                BackgroundSelector.SelectedItem = Nine;
            }
        }

        async void ThemeSelector_Loaded(object sender, RoutedEventArgs e)
        {
            var colorname = (string)(await AudioSingleton.LoadObject<object>("ColorName"));
            if (colorname.Equals("Blue"))
            {
                ThemeSelector.SelectedItem = Blue_item;
            }
            else if (colorname.Equals("Red"))
            {
                ThemeSelector.SelectedItem = Red_item;
            }
            else if (colorname.Equals("Green"))
            {
                ThemeSelector.SelectedItem = Green_item;
            }
            else if (colorname.Equals("Gray"))
            {
                ThemeSelector.SelectedItem = Gray_item;
            }
            else if (colorname.Equals("Orange"))
            {
                ThemeSelector.SelectedItem = Orange_item;
            }
            else
            {
                ThemeSelector.SelectedItem = Yellow_item;
            }

        }
       

        private async void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ThemeSelector.SelectedItem as GridViewItem;
            var colorname = "";
            Windows.UI.Color color;
            Windows.UI.Color secondary_color;
            Windows.UI.Color tertiary_color;

            if (item.Name.Equals("Blue_item"))
            {
                colorname = "Blue";
                color = Windows.UI.Color.FromArgb(255, 27, 161, 226);
                secondary_color = Windows.UI.Color.FromArgb(153, 27, 161, 226);
                tertiary_color = Windows.UI.Color.FromArgb(102, 27, 161, 226);
            }
            else if (item.Name.Equals("Red_item"))
            {
                colorname = "Red";
                color = Windows.UI.Color.FromArgb(255, 229, 20, 0);
                secondary_color = Windows.UI.Color.FromArgb(153, 229, 20, 0);
                tertiary_color = Windows.UI.Color.FromArgb(102, 229, 20, 0);
              
            }
            else if (item.Name.Equals("Green_item"))
            {
                colorname = "Green";
                color = Windows.UI.Color.FromArgb(255, 164, 196, 0);
                secondary_color = Windows.UI.Color.FromArgb(153, 164, 196, 0);
                tertiary_color = Windows.UI.Color.FromArgb(102, 164, 196, 0);
              
            }
            else if (item.Name.Equals("Gray_item"))
            {
                colorname = "Gray";
                color = Windows.UI.Color.FromArgb(255, 157, 157, 157);
                secondary_color = Windows.UI.Color.FromArgb(153, 157, 157, 157);
                tertiary_color = Windows.UI.Color.FromArgb(102, 157, 157, 157);
               
            }
            else if (item.Name.Equals("Orange_item"))
            {
                colorname = "Orange";
                color = Windows.UI.Color.FromArgb(255, 250, 104, 0);
                secondary_color = Windows.UI.Color.FromArgb(153, 250, 104, 0);
                tertiary_color = Windows.UI.Color.FromArgb(102, 250, 104, 0);
              
            }
            else
            {
                colorname = "Yellow";
                color = Windows.UI.Color.FromArgb(255, 240, 163, 10);
                secondary_color = Windows.UI.Color.FromArgb(153, 240, 163, 10);
                tertiary_color = Windows.UI.Color.FromArgb(102, 240, 163, 10);
             
            }

            ((TextBlock)App.Current.Resources["ColorName"]).Text = colorname;
            ((SolidColorBrush)App.Current.Resources["OrangeColor"]).Color = color;
            ((SolidColorBrush)App.Current.Resources["SearchBoxHitHighlightSelectedForegroundThemeBrush"]).Color = secondary_color;
            ((SolidColorBrush)App.Current.Resources["SearchBoxBorderThemeBrush"]).Color = tertiary_color;
            
            ((BitmapImage)App.Current.Resources["Logo"]).UriSource = new Uri("ms-appx:/Assets/Logo_"+ colorname + ".jpg", UriKind.Absolute);
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

            try
            {
                await AudioSingleton.SaveObject<object>(colorname, "ColorName");
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1);
            }
        }

        private async void BackgroundSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = BackgroundSelector.SelectedItem as GridViewItem;
            ((BitmapImage)App.Current.Resources["MainBackgroundImage"]).UriSource = new Uri("ms-appx:/Assets/Wallpapers/" + item.Name + ".jpg", UriKind.Absolute);
            
            try
            {
                await AudioSingleton.SaveObject<object>(item.Name, "BackgroundName");
            }
            catch (Exception e1)
            {
                System.Diagnostics.Debug.WriteLine(e1);
            }

        }
    }
}
