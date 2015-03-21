using CSWindowsStoreAppSaveCollection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Traktrix.AudioPlayer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinRtUtility;

namespace Traktrix.Common
{
    public class AudioSingleton
    {
        public IRandomAccessStream mixed_song_stream;
        public StorageFile mixed_song_file;
        public StorageFile microphone_file;
        private static AudioSingleton instance;
        public bool Started = false;
        public Boolean AllSongsLoaded = false;
        public Boolean SongPlaying = false;
        public Boolean IsShuffleEnabled = false;
        public Boolean IsRepeatEnabled = false;
        public Boolean IsPaused = false;
        public Boolean IsMuted = false;
        public StorageFile currentSong;
        public StorageFile copySong;
        public List<StorageFile> PlayList;
        public List<StorageFile> PreviousSongList;
        public List<StorageFile> AllSongs;
        public List<String> Suggestions;
        public String LastNormalVolume = "";
        public String LastHoverVolumeImage = "";
        public IRandomAccessStream stream;
        public double LastVolumeValue = 0;
        public double SongTotalSeconds = 0;
        public double ElapsedSeconds = 0;
        public DispatcherTimer temp_timer;
        public DispatcherTimer song_changer;
        public TextBlock previous_textblock;
        public bool LoadPlayingPage = false;
        public int IndexValue = 0;
        public bool ObjectSelected = false;
        public bool LoadTrixterMode = false;
        public bool isRecording = false;
        public bool MixedSongPlaying = false;
        public DispatcherTimer stopwatch = new DispatcherTimer();
        public long stopwatch_time;
        public Stopwatch temp;
        public volatile bool _isListening;
        public bool GuitarModeEnabled = false;
        public bool GuitarFileComplete = false;
        public bool RenderFileComplete = false;

        private AudioSingleton() { }

        public static AudioSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AudioSingleton();
                }
                return instance;
            }
        }

        public void stopwatch_Tick(object sender, object e)
        {
            stopwatch_time = stopwatch_time + 1;
        }
        
        public async Task GeneratePlaylist()
        {
            instance.song_changer = new DispatcherTimer();
            instance.song_changer.Interval = TimeSpan.FromMilliseconds(500);
            List<StorageFile> temp1 = new List<StorageFile>();
            List<StorageFile> temp2 = new List<StorageFile>();
            instance.Suggestions = new List<String>();

            StorageFolder musicFolder = KnownFolders.MusicLibrary;
            StorageFolderQueryResult queryresult = musicFolder.CreateFolderQuery(CommonFolderQuery.DefaultQuery);

            IReadOnlyList<StorageFolder> folderList = await queryresult.GetFoldersAsync();

            foreach (StorageFolder folder in folderList)
            {
                IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();

                foreach (StorageFile file in fileList)
                {
                    if (file.FileType.Equals(".mp3") || file.FileType.Equals(".wav"))
                    {
                        var prop = await file.Properties.GetMusicPropertiesAsync();
                        //try
                        //{
                        //    await file.RenameAsync(prop.Title + file.FileType);
                        //}
                        //catch (Exception e1)
                        //{
                        //    System.Diagnostics.Debug.WriteLine("Cannot rename song");
                        //}

                        temp1.Add(file);
                        temp2.Add(file);
                        Suggestions.Add(prop.Title);
                    }
                }
            }

            IReadOnlyList<StorageFile> rootList = await KnownFolders.MusicLibrary.GetFilesAsync();

            foreach (StorageFile file in rootList)
            {
                if (file.FileType.Equals(".mp3") || file.FileType.Equals(".wav"))
                {
                    var prop = await file.Properties.GetMusicPropertiesAsync();

                    //try
                    //{
                    //    await file.RenameAsync(prop.Title + file.FileType);
                    //}
                    //catch (Exception e)
                    //{
                    //    System.Diagnostics.Debug.WriteLine("Cannot rename song");
                    //}

                    temp1.Add(file);
                    temp2.Add(file);
                    Suggestions.Add(prop.Title);
                }
            }

            instance.PlayList = temp1.OrderBy((p) => (p.DisplayName.ToLower())).ToList();
            instance.AllSongs = temp2.OrderBy((p) => (p.DisplayName.ToLower())).ToList();
            AllSongsLoaded = true;

           instance.PreviousSongList = new List<StorageFile>();
        }

        public void AddToPreviousSongList(StorageFile temp)
        {
            if (PreviousSongList.Count <= 20)
            {
                PreviousSongList.Add(temp);
            }
            else
            {
                PreviousSongList.RemoveAt(0);
                PreviousSongList.Add(temp);
            }
        }

        public static void UpdateTimer_Tick(object sender, object e)
        {
            if (AudioSingleton.Instance.ElapsedSeconds >= AudioSingleton.Instance.SongTotalSeconds)
            {
                (sender as DispatcherTimer).Stop();
                AudioSingleton.Instance.ElapsedSeconds = 0;
            }
            else
            {
                AudioSingleton.Instance.ElapsedSeconds++;
            }
        }

        public static async Task SaveObject<T>(T obj, String name)
        {
            //new up ObjectStorageHelper specifying that we want to interact with the Local storage folder
            var objectStorageHelper = new ObjectStorageHelper<T>(StorageType.Local);
            //Save the object (via XML Serialization) to the specified folder, asynchronously
            await objectStorageHelper.SaveAsync(obj, name);
        }

        public static async Task<T> LoadObject<T>(String name)
        {
            //new up ObjectStorageHelper specifying that we want to interact with the Local storage folder
            var objectStorageHelper = new ObjectStorageHelper<T>(StorageType.Local);
            T obj = default(T);//Get the object from the storage folder

            try
            {
                obj = await objectStorageHelper.LoadAsync(name);
            }
            catch (Exception e)
            {
            }

            return obj;
        }

        public static async void SaveList<T>(String name, T object_)
        {
            try
            {
                string localData = ObjectSerializer<T>.ToXml(object_);

                if (!string.IsNullOrEmpty(localData))
                {
                    StorageFile localFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(name + ".xml", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(localFile, localData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }

        public static async Task<T> LoadList<T>(String name)
        {
            StorageFile localFile;
            try
            {
                localFile = await ApplicationData.Current.LocalFolder.GetFileAsync(name + ".xml");
            }
            catch (FileNotFoundException ex)
            {
                localFile = null;
            }
            if (localFile != null)
            {
                string localData = await FileIO.ReadTextAsync(localFile);

                return ObjectSerializer<T>.FromXml(localData);
            }

            return default(T);
        }

    }

}
