using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Traktrix.Common;
using Windows.Storage;

namespace Traktrix.Audio
{
    class MyWavWriter
    {
        static byte[] RIFF_HEADER = new byte[] { 0x52, 0x49, 0x46, 0x46 };
        static byte[] FORMAT_WAVE = new byte[] { 0x57, 0x41, 0x56, 0x45 };
        static byte[] FORMAT_TAG = new byte[] { 0x66, 0x6d, 0x74, 0x20 };
        static byte[] AUDIO_FORMAT = new byte[] { 0x01, 0x00 };
        static byte[] SUBCHUNK_ID = new byte[] { 0x64, 0x61, 0x74, 0x61 };
        private const int BYTES_PER_SAMPLE = 2;

        static BinaryWriter _binaryWriter;
        static Stream _fileStream;

        public async Task Begin(Stream songstream,string fileName,  int byteStreamSize, int channelCount, int sampleRate)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");

            AudioSingleton.Instance.mixed_song_file = await KnownFolders.MusicLibrary.CreateFileAsync(fileName + "_tractrix_mixed.wav", Windows.Storage.CreationCollisionOption.GenerateUniqueName);

            await OpenFileForWriting();

            //WriteWavRiffHeader();
            //WriteWavFormatChunkHeader(waveFormat);
            //WriteWavDataChunkHeader();
            WriteHeader(_fileStream, byteStreamSize, channelCount, sampleRate);
            //End();
            _binaryWriter.Flush();
            _fileStream.Flush();
            songstream.Position = 0;
            byte[]temp=new byte[songstream.Length];
            songstream.Read(temp,0,(int)songstream.Length);
            _binaryWriter.Write(temp, 0, temp.Length);
            End();
        }

        public async Task OpenFileForWriting()
        {
            _fileStream = await AudioSingleton.Instance.mixed_song_file.OpenStreamForWriteAsync();

            _binaryWriter = new BinaryWriter(_fileStream);
        }

        public static void WriteHeader(System.IO.Stream targetStream, int byteStreamSize, int channelCount, int sampleRate)
        {
            int byteRate = sampleRate * channelCount * BYTES_PER_SAMPLE;
            int blockAlign = channelCount * BYTES_PER_SAMPLE;

            _binaryWriter.Write(RIFF_HEADER, 0, RIFF_HEADER.Length);
            _binaryWriter.Write(PackageInt(byteStreamSize + 42, 4), 0, 4);

            _binaryWriter.Write(FORMAT_WAVE, 0, FORMAT_WAVE.Length);
            _binaryWriter.Write(FORMAT_TAG, 0, FORMAT_TAG.Length);
            _binaryWriter.Write(PackageInt(16, 4), 0, 4);//Subchunk1Size    

            _binaryWriter.Write(AUDIO_FORMAT, 0, AUDIO_FORMAT.Length);//AudioFormat   
            _binaryWriter.Write(PackageInt(channelCount, 2), 0, 2);
            _binaryWriter.Write(PackageInt(sampleRate, 4), 0, 4);
            _binaryWriter.Write(PackageInt(byteRate, 4), 0, 4);
            _binaryWriter.Write(PackageInt(blockAlign, 2), 0, 2);
            _binaryWriter.Write(PackageInt(BYTES_PER_SAMPLE * 8), 0, 2);
            //targetStream.Write(PackageInt(0,2), 0, 2);//Extra param size
            _binaryWriter.Write(SUBCHUNK_ID, 0, SUBCHUNK_ID.Length);
            _binaryWriter.Write(PackageInt(byteStreamSize, 4), 0, 4);
        }

        public static void End()
        {
            _binaryWriter.Flush();
            _binaryWriter.Dispose();
            _fileStream.Dispose();
        }

        static byte[] PackageInt(int source, int length = 2)
        {
            if ((length != 2) && (length != 4))
                throw new ArgumentException("length must be either 2 or 4", "length");
            var retVal = new byte[length];
            retVal[0] = (byte)(source & 0xFF);
            retVal[1] = (byte)((source >> 8) & 0xFF);
            if (length == 4)
            {
                retVal[2] = (byte)((source >> 0x10) & 0xFF);
                retVal[3] = (byte)((source >> 0x18) & 0xFF);
            }
            return retVal;
        }

    }
}
