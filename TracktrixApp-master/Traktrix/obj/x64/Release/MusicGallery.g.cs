﻿

#pragma checksum "C:\Users\Mutahir\Documents\GitHub\TracktrixApp\Traktrix\MusicGallery.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B24B7AF43CE019FA5596B7ADBE0FF87D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Traktrix
{
    partial class MusicGallery : global::Windows.UI.Xaml.Controls.Page, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 57 "..\..\..\MusicGallery.xaml"
                ((global::Windows.UI.Xaml.UIElement)(target)).LostFocus += this.SearchSong_LostFocus;
                 #line default
                 #line hidden
                #line 57 "..\..\..\MusicGallery.xaml"
                ((global::Windows.UI.Xaml.Controls.SearchBox)(target)).QuerySubmitted += this.SearchSong_QuerySubmitted;
                 #line default
                 #line hidden
                #line 57 "..\..\..\MusicGallery.xaml"
                ((global::Windows.UI.Xaml.Controls.SearchBox)(target)).QueryChanged += this.SearchSong_QueryChanged;
                 #line default
                 #line hidden
                #line 57 "..\..\..\MusicGallery.xaml"
                ((global::Windows.UI.Xaml.Controls.SearchBox)(target)).SuggestionsRequested += this.SearchBoxEventsSuggestionsRequested;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}

