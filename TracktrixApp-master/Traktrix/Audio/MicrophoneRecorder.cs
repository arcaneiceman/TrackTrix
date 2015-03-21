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
    class MicrophoneRecorder
    {
        //public  MemoryStream inputstream;
        //private MemoryStream BitOutput = new MemoryStream();
        //public volatile bool first = false;
        //public MicrophoneRecorder()
        //{
        //    waveIn = new WasapiCaptureRT()
        //    {
        //        //WaveFormat = new WaveFormat(44100,32,1)
        //    };
        //    //waveIn.WaveFormat = new WaveFormat(44100, 16, 1);
        //    waveIn.DataAvailable += OnDataAvailable;
        //    waveIn.RecordingStopped += OnRecordingStopped;
        //    inputstream = new System.IO.MemoryStream();
        //}



        //public Stream GetMCStream()
        //{
        //    Convert();
        //    return BitOutput;
        //}

        //void OnRecordingStopped(object sender, EventArgs e)
        //{
        //    Cleanup();
        //}

        //void OnDataAvailable(object sender, WaveInEventArgs e)
        //{
        //    if (first == false)
        //    {
        //        first = true;
        //        return;
        //    }
        //    inputstream.Write(e.Buffer, 0, e.BytesRecorded);
        //}

        //public void StartthisRecording()
        //{
        //    System.Diagnostics.Debug.WriteLine("StartRecording");
        //    waveIn.StartRecording();
        //}

        //public void StopthisRecording()
        //{
        //    waveIn.StopRecording();
        //    System.Diagnostics.Debug.WriteLine("StopRecording");
        //}

        //private void Cleanup()
        //{
        //    if (waveIn != null) // working around problem with double raising of RecordingStopped
        //    {
        //        waveIn.Dispose();
        //        waveIn = null;
        //    }
        //}

        private readonly IAudioCaptureStream _audioCaptureStream;
        private readonly IWaveFileWriter _waveFileWriter;
        private BlockingCollection<AudioBufferCapturedEventArgs> _recordedAudioBuffer;

        public MicrophoneRecorder()
        {
            _audioCaptureStream = new AudioCaptureStream(OnAudioStreamActivated, OnAudioBufferCaptured);
            _waveFileWriter = new WaveFileWriter();

            _recordedAudioBuffer = new BlockingCollection<AudioBufferCapturedEventArgs>(new ConcurrentQueue<AudioBufferCapturedEventArgs>());

            _isRecording = false;

        }

        private bool _isRecording;
        //public void Convert()
        //{
        //    byte[] lol = inputstream.ToArray();
        //    float myFloat;
        //    short myshort;
        //    //inputstream.Dispose();
        //    for (int i = 0; i < lol.Length; i = i + 4)
        //    {
        //        myFloat = System.BitConverter.ToSingle(lol, i);
        //        if (myFloat > 1f)
        //        {
        //            myFloat = 1f;
        //        }
        //        else if (myFloat < -1f)
        //        {
        //            myFloat = -1f;
        //        }
        //        myshort = (short)Math.Floor(myFloat * 32767);
        //        BitOutput.WriteByte((byte)(myshort & 0xff));
        //        BitOutput.WriteByte((byte)(((myshort >> 8) & 0xff)));
        //    }
        //    BitOutput.Position = 0; // Reset Position of Stream
        //}

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
            await _waveFileWriter.Begin("recorddemo.wav", waveFormat);

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

    }

}
