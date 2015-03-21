using DemoApp.CoreAudio.Common;
using DemoApp.CoreAudio.Constants;
using DemoApp.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Traktrix.Common;
using Windows.Storage;

namespace DemoApp.Services
{
    public class WaveFileWriter : IWaveFileWriter
    {
        private StorageFile _file;
        private Stream _fileStream;
        private BinaryWriter _binaryWriter;
        private long _dataSizePosition;
        private int _dataChunkSize;

        public async Task Begin(string fileName, WaveFormat waveFormat)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            
            //_file = await KnownFolders.MusicLibrary.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            AudioSingleton.Instance.microphone_file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            await OpenFileForWriting();

            WriteWavRiffHeader();
            WriteWavFormatChunkHeader(waveFormat);
            WriteWavDataChunkHeader();
        }

        public void Write(byte[] buffer, int bytesRecorded)
        {
            _fileStream.Write(buffer, 0, bytesRecorded);

            _dataChunkSize += bytesRecorded;
        }

        public void End()
        {
            UpdateWavHeaders();

            _binaryWriter.Flush();
            _binaryWriter.Dispose();
            _fileStream.Dispose();

            if (AudioSingleton.Instance.GuitarModeEnabled)
            {
                AudioSingleton.Instance.GuitarFileComplete = true;
            }
            else
            {
                AudioSingleton.Instance.RenderFileComplete = true;
            }

        }

        private async Task OpenFileForWriting()
        {
            _fileStream = await AudioSingleton.Instance.microphone_file.OpenStreamForWriteAsync();

            _binaryWriter = new BinaryWriter(_fileStream);
        }

        private void WriteWavRiffHeader()
        {
            _binaryWriter.Write("RIFF".ToCharArray());    // Group id
            _binaryWriter.Write((uint)0);                 // File length, total file length minus 8, which is taken up by RIFF
            _binaryWriter.Write("WAVE".ToCharArray());    // Riff type
        }

        private void WriteWavFormatChunkHeader(WaveFormat waveFormat)
        {
            _binaryWriter.Write("fmt ".ToCharArray());

            uint samplesPerSecond = (uint)waveFormat.SampleRate;
            ushort channels = (ushort)waveFormat.Channels;
            ushort bitsPerSample = (ushort)waveFormat.BitsPerSample;
            ushort blockAlign = (ushort)(channels * (bitsPerSample / 8));
            uint averageBytesPerSec = (samplesPerSecond * blockAlign);

            _binaryWriter.Write((uint)(18 + waveFormat.ExtraSize));               // Length of header in bytes
            unchecked { _binaryWriter.Write((short)0xFFFE); }                     // Format tag, 65534 (WAVE_FORMAT_EXTENSIBLE)
            _binaryWriter.Write(channels);                                        // Number of channels
            _binaryWriter.Write(samplesPerSecond);                                // Frequency of the audio in Hz... 44100
            _binaryWriter.Write(averageBytesPerSec);                              // For estimating RAM allocation
            _binaryWriter.Write(blockAlign);                                      // Sample frame size, in bytes
            _binaryWriter.Write(bitsPerSample);

            _binaryWriter.Write((short)waveFormat.ExtraSize);                     // Extra param size
            _binaryWriter.Write(bitsPerSample);                                   // Should be valid bits per sample
            _binaryWriter.Write((uint)3);                                         // Should be channel mask
            byte[] subformat = new Guid(KsMedia.WAVEFORMATEX).ToByteArray();
            _binaryWriter.Write(subformat, 0, subformat.Length);
        }

        private void WriteWavDataChunkHeader()
        {
            // Write the data chunk
            _binaryWriter.Write("data".ToCharArray());                // Chunk id

            _dataSizePosition = _fileStream.Position;
            _binaryWriter.Write((uint)0);                             // Chunk size, length of header in bytes
        }

        private void UpdateWavHeaders()
        {
            UpdateWavRiffHeader();
            UpdateDataChunkHeader();
        }

        private void UpdateWavRiffHeader()
        {
            _binaryWriter.Seek(4, SeekOrigin.Begin);
            _binaryWriter.Write((uint)(_binaryWriter.BaseStream.Length - 8));    // File length
        }

        private void UpdateDataChunkHeader()
        {
            _binaryWriter.Seek((int)_dataSizePosition, SeekOrigin.Begin);
            _binaryWriter.Write((uint)_dataChunkSize);
        }
    }
}