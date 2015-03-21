using SharpDX;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Traktrix.Filters;
using Traktrix.Audio;

namespace Traktrix.AudioPlayer
{
    class PlayingStream
    {
        private XAudio2 xaudio;
        private MasteringVoice masteringVoice;
        private volatile bool playing;
        private volatile Stream SongStream;
        private SourceVoice sourceVoice;
        private int BUFFERSIZE;// 4 KB BUFFER
        AudioBuffer Abuffer = null;
        private static int FilterStatus = 0;
        private byte[] playbuffer;
        DataStream dataStream;
        Stream clonestream;

        //FilterCutOffs are in Filter CLass    
        /*
         * Filter Status is 0 at default. This is NO Filter
         *  0 = No Filter
         *  1 = Center Channel
         *  2 = High Pass 
         *  3 = Low Pass
         *  4 = High Shelf
         *  5 = Low Shelf
         *  6 = Band Pass
         *  7 = Band Stop
         *  8 = CenterCut
         *  9 = Notch
         */

        ~PlayingStream() {
            try
            {
                SongStream.Dispose();
                dataStream.Dispose();
                clonestream.Dispose();
            }
            catch (Exception)
            {
            }
        }

        public PlayingStream()
        {
            var waveFormat = new SharpDX.Multimedia.WaveFormat();
            XAudio2 xaudio = new XAudio2();
            MasteringVoice masteringVoice = new MasteringVoice(xaudio);
            sourceVoice = new SourceVoice(xaudio, waveFormat, true);
            SongStream = new MemoryStream();
            FilterStatus = 0;
        }

        public PlayingStream(IRandomAccessStream s)
        {
            var waveFormat = new SharpDX.Multimedia.WaveFormat();
            XAudio2 xaudio = new XAudio2();
            MasteringVoice masteringVoice = new MasteringVoice(xaudio);
            sourceVoice = new SourceVoice(xaudio, waveFormat, true);
            SongStream = s.AsStreamForRead(BUFFERSIZE);
            sourceVoice.StreamEnd += sourceVoice_StreamEnd;
            Abuffer = new AudioBuffer
            {
                Flags = BufferFlags.EndOfStream
            };
            playing = false;
            BUFFERSIZE = 1024 * 4;
        }
        void sourceVoice_StreamEnd()
        {
            if (playing)
            {
                ReadAndSubmit();
            }
            else
            {
                sourceVoice.FlushSourceBuffers();
            }
        }
        void ReadAndSubmit()
        {
            try
            {

                if (playing == false)
                {
                    return;
                }
                playbuffer = new byte[BUFFERSIZE];
                int bytesRead = SongStream.Read(playbuffer, 0, BUFFERSIZE);
                playbuffer = Filter.ApplyFilter(playbuffer, FilterStatus, BUFFERSIZE);
                dataStream = DataStream.Create(playbuffer, true, true);
                Abuffer.Stream = dataStream;
                Abuffer.AudioBytes = (int)dataStream.Length;
                sourceVoice.SubmitSourceBuffer(Abuffer, null);
                //dataStream.Dispose();
            }
            catch (Exception e)
            {
            }
        }
        public void playSound()
        {
            System.Diagnostics.Debug.WriteLine("play");
            if (playing)
            {
                return;
            }
            else
            {
                playing = true;
                sourceVoice.Start();
                ReadAndSubmit();
            }
        }
        public void pauseSound()
        {
            try
            {
                playing = false;
                sourceVoice.Stop();
            }
            catch (Exception e)
            {
                e.ToString();
                //Could not stop;
            }
        }
        public void stopSound()
        {
            sourceVoice.FlushSourceBuffers();
            sourceVoice.Stop();
            playing = false;
            SongStream.Position = 0;
            //while (SongStream.Position != 0)
            //{
            //    try
            //    {
            //        SongStream.Position = 0;//Song Stream Position Reset
            //    }
            //    catch (Exception e)
            //    {
            //        System.Diagnostics.Debug.WriteLine("Stream Concurrency Issue");

            //    }
            //}

        }
        public void setVolume(float newVol)
        {
            sourceVoice.SetVolume(newVol);
        }

        public float getVolume()
        {
            return sourceVoice.Volume;
        }

        public void SetFilterStatus(int input)
        {
            System.Diagnostics.Debug.WriteLine(" Filter status is " + input);
            FilterStatus = input;
        }

        public int GetFilterStatus()
        {
            return FilterStatus;
        }

        public Stream GetSongStream()
        {
            /* WALI'S EDITING */
            if (clonestream == null)
            {
                clonestream = new MemoryStream();
                SongStream.CopyTo(clonestream, (int)SongStream.Length);
            }

            return clonestream;
        }
    }


}


