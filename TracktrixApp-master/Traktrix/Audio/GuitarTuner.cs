using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.IO;
using Windows.Storage;
using Traktrix.AudioPlayer;
using Windows.Media.Transcoding;
using Windows.Media.MediaProperties;
using DemoApp;
using DemoApp.Services.Interfaces;
using System.Collections.Concurrent;
using DemoApp.Services;
using DemoApp.CoreAudio.Common;
using Traktrix.Common;

namespace Traktrix.Audio
{
    class GuitarTuner
    {
        private readonly IAudioCaptureStream _audioCaptureStream;
        private readonly IWaveFileWriter _waveFileWriter;
        private BlockingCollection<AudioBufferCapturedEventArgs> _recordedAudioBuffer;
        public static volatile string closestFrequency;
        public static volatile string noteName;
        ~GuitarTuner()
        {
            try
            {
                AudioSingleton.Instance.mixed_song_stream.Dispose();
            }
            catch (Exception)
            {
            }
        }
        public GuitarTuner()
        {
            _audioCaptureStream = new AudioCaptureStream(OnAudioStreamActivated, OnAudioBufferCaptured);
            _waveFileWriter = new WaveFileWriter();

            _recordedAudioBuffer = new BlockingCollection<AudioBufferCapturedEventArgs>(new ConcurrentQueue<AudioBufferCapturedEventArgs>());

            _isRecording = false;
            noteName = "";
            closestFrequency = "";

        }

        private bool _isRecording;

        private bool CanStartRecording(object arg)
        {
            if (arg == null) return false;

            var isRecording = (bool)arg;

            return !isRecording;
        }

        public void StartRecording()
        {
            _isRecording = true;

            _audioCaptureStream.Start();
        }

        public void StopRecording()
        {
            _audioCaptureStream.Stop();

            _isRecording = false;
        }

        private async void OnAudioStreamActivated(WaveFormat waveFormat)
        {
            await _waveFileWriter.Begin("GuitarMix.wav", waveFormat);

            var surpressWarning = Task.Factory.StartNew(WriteCapturedAudioToFile);
        }

        private void OnAudioBufferCaptured(AudioBufferCapturedEventArgs e)
        {
            _recordedAudioBuffer.Add(e);
        }

        private void WriteCapturedAudioToFile()
        {
            AudioBufferCapturedEventArgs capturedAudioBuffer = null;

            while (_isRecording || _recordedAudioBuffer.Count > 0)
            {
                capturedAudioBuffer = _recordedAudioBuffer.Take();
                _waveFileWriter.Write(capturedAudioBuffer.Buffer, capturedAudioBuffer.BytesRecorded);
            }

            _waveFileWriter.End();
        }

        public byte[] ConvertStereotoMono(byte[] input)
        {
            double[] temp = Filters.Filter.BytesToDoubles(input);
            double[] x = new double[temp.Length / 2];
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = temp[i * 2];
            }
            return Filters.Filter.DoublesToBytes(x, x.Length * 2);
        }

        public byte[] Convert32to16(byte[] input)
        {
            float myFloat;
            short myshort;
            MemoryStream BitOutput = new MemoryStream();
            for (int i = 0; i < input.Length; i = i + 4)
            {
                myFloat = System.BitConverter.ToSingle(input, i);
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
            return BitOutput.ToArray();
        }

        private void RunTests(byte[] input)
        {
            input = Convert32to16(input);
            input = ConvertStereotoMono(input);
            int volume = TestVolume(input);

            if (volume > 750)
            {
                System.Diagnostics.Debug.WriteLine("Volume is " + volume);
                FreqTest(input);
            }
            //FreqTest(input);
            //MatchSound(input);
        }

        public int TestVolume(byte[] data)
        {
            //RMS Method
            double rms = 0;
            ushort byte1 = 0;
            ushort byte2 = 0;
            short value = 0;
            int volume = 0;
            rms = (short)(byte1 | (byte2 << 8));

            for (int i = 0; i < data.Length - 1; i += 2)
            {
                byte1 = data[i];
                byte2 = data[i + 1];

                value = (short)(byte1 | (byte2 << 8));
                rms += Math.Pow(value, 2);
            }

            rms /= (double)(data.Length / 2);
            volume = (int)Math.Floor(Math.Sqrt(rms));
            return volume;
        }

        public void FreqTest(byte[] input)
        {
            double[] x = Filters.Filter.BytesToDoubles(input);
            double freq = Tuner.FrequencyTools.FindFundamentalFrequency(x, 44100, 60, 1300);
            //System.Diagnostics.Debug.WriteLine("Frequency Detected was " + freq);

            double closest_frequency;
            string note_name;
            FindClosestNote(freq, out closest_frequency, out note_name);
            closestFrequency = Math.Round(closest_frequency, 2).ToString();
            noteName = note_name;
            //AudioSingleton.Instance.freq
            System.Diagnostics.Debug.WriteLine("Closest Freq was " + closestFrequency + " and Note was " + noteName);
            // the above line is he "out method". String notename is the notenam and coloest freq is a number  a double.
            //if u want u can make two public  volatile  values
            //and either use get set methods or not
            // query them about once in 200 millisecon (SEE WHAT I DO NOW)

        }

        static string[] NoteNames = { "A", "A#", "B/H", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
        static double ToneStep = Math.Pow(2, 1.0 / 12);
        private void FindClosestNote(double frequency, out double closestFrequency, out string noteName)
        {
            const double AFrequency = 440.0;
            const int ToneIndexOffsetToPositives = 120;
            try
            {
                int toneIndex = (int)Math.Round(Math.Log(frequency / AFrequency, ToneStep));
                noteName = NoteNames[(ToneIndexOffsetToPositives + toneIndex) % NoteNames.Length];
                closestFrequency = Math.Pow(ToneStep, toneIndex) * AFrequency;
            }
            catch (Exception r)
            {
                noteName = "undertermined";
                closestFrequency = 0.0;
            }
        }
    }

}
