using DemoApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Traktrix.Common;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Traktrix.Audio
{
    public static class AudioRender
    {


        public static Stream FillGap(Stream inputstream)
        {
           byte[] songholder = new byte[inputstream.Length];
           inputstream.Read(songholder, 0, songholder.Length);
           var time = AudioSingleton.Instance.stopwatch_time;//Time in Milliseconds!
           var sizeofarray=(88200/1000)*time;
           byte[] gap = new byte[sizeofarray];
           MemoryStream output = new MemoryStream();
           output.Write(gap, 0, gap.Length);
           output.Write(songholder, 0, songholder.Length);
           return output;
        }

        public static Stream Convert(Stream inputstream)
        {
            byte[] lol = new byte[inputstream.Length];
            inputstream.Read(lol, 0, lol.Length);
            Stream BitOutput = new MemoryStream();
            float myFloat;
            short myshort;
            //inputstream.Dispose();
            for (int i = 0; i < lol.Length; i = i + 4)
            {
                myFloat = System.BitConverter.ToSingle(lol, i);
                if (myFloat > 1f)
                {
                    myFloat = 1f;
                }
                else if (myFloat < -1f)
                {
                    myFloat = -1f;
                }
                myshort = (short)Math.Floor(myFloat * 32767);
                BitOutput.WriteByte((byte)(myshort & 0xff));
                BitOutput.WriteByte((byte)(((myshort >> 8) & 0xff)));
            }
            BitOutput.Position = 0; // Reset Position of Stream
            return BitOutput;
        }
        public static IRandomAccessStream RenderAudio(Stream Song, int FilterNo, Stream Recording)
        {
            //conversion code 
            Recording = Convert(Recording);
            //conversion code end for recording 

            //GAP FILLING code
            Recording = FillGap(Recording);
            //GapFilling code
            Song.Position = 0; Recording.Position = 0; //Resetting the Position of the Streams
            byte[] SongBytes;
            byte[] RecordBytes = new byte[Recording.Length];
            double[] RecordingDoubles = new double[Recording.Length / 2];

            byte[] filterbuffer = new byte[1024 * 8]; //8kb buffer
            //apply filter using buffers
            MemoryStream FsongStream = new MemoryStream();
            int bytesread;
            while (true)
            {
                bytesread = Song.Read(filterbuffer, 0, filterbuffer.Length);
                if (bytesread != filterbuffer.Length)
                {
                    break;
                }
                //HARD CODED
                filterbuffer = Filters.Filter.ApplyFilter(filterbuffer, 1, filterbuffer.Length);
                FsongStream.Write(filterbuffer, 0, filterbuffer.Length);
            }

            SongBytes = FsongStream.ToArray();
            Recording.Read(RecordBytes, 0, (int)Recording.Length);
            double[] SongDoubles = new double[SongBytes.Length / 2];
            SongDoubles = Filters.Filter.BytesToDoubles(SongBytes);
            RecordingDoubles = Filters.Filter.BytesToDoubles(RecordBytes);
            for (int i = 0; i < RecordingDoubles.Length; i++)
            {
                SongDoubles[i] = (SongDoubles[i] * 0.5d + RecordingDoubles[i] * 0.95d);
            }
            //SaveFile
            //here
            SongBytes = Filters.Filter.DoublesToBytes(SongDoubles, SongBytes.Length);
            MemoryStream ms = new MemoryStream(SongBytes);
            ms.Position = 0;
            return ms.AsRandomAccessStream();
        }



        public static async Task<IRandomAccessStream> ConvertMicrophoneOutput()
        {
            var src_file = await KnownFolders.MusicLibrary.GetFileAsync("recorddemo.wav");
            var dst_file = await KnownFolders.MusicLibrary.CreateFileAsync("final.wav", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            MediaTranscoder temp_transcoder = new MediaTranscoder();
            MediaEncodingProfile profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium);
            var preparedTranscodeResult = await temp_transcoder.PrepareFileTranscodeAsync(src_file, dst_file, profile);
            await preparedTranscodeResult.TranscodeAsync();

            var stream = await dst_file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            //WaveFileWriter._fileStream.Position = 0;
            //return WaveFileWriter._fileStream.AsRandomAccessStream();
            return stream;
        }







    }
}
