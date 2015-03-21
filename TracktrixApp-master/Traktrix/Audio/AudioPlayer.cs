using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Traktrix.Audio;
using System.IO;
using Traktrix.Common;
using Windows.Storage;
using System.Diagnostics;

namespace Traktrix.AudioPlayer
{
    public class AudioPlayer
    {
        private static AudioPlayer instance;
        private static PlayingStream play_ = new PlayingStream();
        private static MicrophoneRecorder mc = new MicrophoneRecorder();
        private static GuitarTuner GT = new GuitarTuner();

        public static AudioPlayer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AudioPlayer();
                }
                return instance;
            }
        }

        public void LoadSong(IRandomAccessStream sstream)
        {
            if (play_ != null)
            {
                play_ = null;
            }
            play_ = new PlayingStream(sstream);
        }
        public async void PlaySong()
        {
            play_.playSound();
        }

        public void PauseSong()
        {
            play_.pauseSound();
        }
        public void StopSong()
        {
            play_.stopSound();
        }
        public void UnloadSong()
        {
            play_ = null;
        }
        public void CreateRecorder()
        {
            mc = new MicrophoneRecorder();
        }

        public async Task StartRecording()
        {
            //--------------------------------//
            //StopSong();
            //mc.StartthisRecording();
            //while (mc.first != true)
            //{
            //    await Task.Delay(TimeSpan.FromMilliseconds(2));
            //}
            //PlaySong();
            //--------------------------------//

            StopSong();
            PlaySong();

            AudioSingleton.Instance.stopwatch_time = 0;
            AudioSingleton.Instance.temp = new Stopwatch();
            AudioSingleton.Instance.temp.Start();

            mc.StartRecording();
        }

        public void StopRecording()
        {

            //StopSong();
            //---------------------//
            //mc.StopthisRecording();
            //---------------------//
            mc.StopRecording();

        }

        public async Task RenderAudio()
        {
            //-------------------------------------------------------//
            Stream songs = play_.GetSongStream();
            int ftemp = getFilterStatus();
            UnloadSong();

            while (!AudioSingleton.Instance.RenderFileComplete)
            {
                await Task.Delay(100);
            }

            var file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("recorddemo.wav");
            var stream = await file.OpenAsync(FileAccessMode.Read);

            try
            {
                if (AudioSingleton.Instance.mixed_song_stream != null)
                {
                    AudioSingleton.Instance.mixed_song_stream.Dispose();
                }
            }
            catch (Exception)
            {
            }

            AudioSingleton.Instance.mixed_song_stream = AudioRender.RenderAudio(songs, ftemp, stream.AsStream());
            Stream clone_stream = new MemoryStream();
            stream.Dispose();

            AudioSingleton.Instance.mixed_song_stream.AsStream().CopyTo(clone_stream);
            AudioSingleton.Instance.mixed_song_stream.AsStream().Position = 0;
            clone_stream.Position = 0;
            AudioSingleton.Instance.RenderFileComplete = false;
            LoadSong(clone_stream.AsRandomAccessStream());
            //setFilterNo(0);
            PlaySong();


            //-------------------------------------------------------//

            //Stream clone = new MemoryStream();
            //SongStream.CopyTo(clonestream, (int)SongStream.Length);

            //return AudioSingleton.Instance.mixed_song_file;
        }

        public void setFilterNo(int no)
        {
            play_.SetFilterStatus(no);
        }

        public void setVolume(float no)
        {
            play_.setVolume(no);
        }

        public float getVolume()
        {
            return play_.getVolume();
        }

        public Stream getStream()
        {
            return play_.GetSongStream();
        }

        public int getFilterStatus()
        {
            return play_.GetFilterStatus();
        }

        public async void GTListen()
        {
            //Guitar Tuner will have its loop running here
            GT = new GuitarTuner();
                GT.StartRecording();
        }

        public async Task GTStop()
        {
            GT.StopRecording();
            while (!AudioSingleton.Instance.GuitarFileComplete)
            {
                await Task.Delay(100);
            }

            AudioSingleton.Instance.GuitarFileComplete = false;

            var file = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("GuitarMix.wav");
            var stream = await file.OpenAsync(FileAccessMode.Read);
            byte[] input = new byte[stream.AsStream().Length];
            stream.AsStream().Read(input, 0, input.Length);
            input = GT.Convert32to16(input);
            input = GT.ConvertStereotoMono(input);
            int volume = GT.TestVolume(input);

            if (volume > 750)
            {
                System.Diagnostics.Debug.WriteLine("Volume is " + volume);
                GT.FreqTest(input);
            }

            stream.Dispose();
        }
    }
}
