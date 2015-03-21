using DemoApp.CoreAudio.Common;
using DemoApp.CoreAudio.Components.MMDevice;
using DemoApp.CoreAudio.Components.WASAPI;
using DemoApp.Services.Interfaces;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Traktrix.Common;
using Windows.Media.Devices;

namespace DemoApp.Services
{
    public class AudioCaptureStream : IAudioCaptureStream
    {
        private Action<WaveFormat> _audioStreamActivatedEventHandler;
        private Action<AudioBufferCapturedEventArgs> _audioBufferCapturedEventHandler;
        private bool _isRecording;

        public AudioCaptureStream(Action<WaveFormat> audioStreamActivatedEventHandler,
            Action<AudioBufferCapturedEventArgs> audioBufferCapturedEventHandler)
        {
            if (audioStreamActivatedEventHandler == null) throw new ArgumentNullException("audioStreamActivatedEventHandler");
            if (audioBufferCapturedEventHandler == null) throw new ArgumentNullException("audioBufferCapturedEventHandler");

            _audioStreamActivatedEventHandler = audioStreamActivatedEventHandler;
            _audioBufferCapturedEventHandler = audioBufferCapturedEventHandler;
        }

        public void Start()
        {
            _isRecording = true;

            var defaultAudioCaptureId = MediaDevice.GetDefaultAudioCaptureId(AudioDeviceRole.Default);
            var completionHandler = new ActivateAudioInterfaceCompletionHandler(StartCapture);
            IActivateAudioInterfaceAsyncOperation createAsync;

            WindowsMultimediaDevice.ActivateAudioInterfaceAsync(
                defaultAudioCaptureId, new Guid(CoreAudio.Components.WASAPI.Constants.IID_IAudioClient), IntPtr.Zero, completionHandler, out createAsync);
        }

        public void Stop()
        {
            _isRecording = false;
        }

        private void StartCapture(IAudioClient audioClient, WaveFormat waveFormat)
        {
            //AudioSingleton.Instance.stopwatch.Interval = new TimeSpan(0, 0, 0, 0, 1);
            //AudioSingleton.Instance.stopwatch.Tick += AudioSingleton.Instance.stopwatch_Tick;
            //AudioSingleton.Instance.stopwatch.Start();
            //AudioSingleton.Instance.stopwatch_time = 0;

            _audioStreamActivatedEventHandler(waveFormat);

            uint bufferSize;
            audioClient.GetBufferSize(out bufferSize);

            object audioCaptureClientInterface;
            audioClient.GetService(new Guid(CoreAudio.Components.WASAPI.Constants.IID_IAudioCaptureClient), out audioCaptureClientInterface);
            var audioCaptureClient = (IAudioCaptureClient)audioCaptureClientInterface;

            var sleepMilliseconds = CalculateCaptureDelay(waveFormat, bufferSize);

            audioClient.Start();

            try
            {
                AudioSingleton.Instance.temp.Stop();
                AudioSingleton.Instance.stopwatch_time = AudioSingleton.Instance.temp.ElapsedMilliseconds + sleepMilliseconds;
            }
            catch (Exception)
            {
                
                //throw;
            }
            
            //AudioSingleton.Instance.stopwatch.Stop();
            //AudioSingleton.Instance.stopwatch.Tick -= AudioSingleton.Instance.stopwatch_Tick;
            //AudioSingleton.Instance.stopwatch_time = AudioSingleton.Instance.stopwatch_time + sleepMilliseconds;

            System.Diagnostics.Debug.WriteLine("The time is: " + AudioSingleton.Instance.stopwatch_time);

            while (_isRecording)
            {
                Task.Delay(sleepMilliseconds);

                CaptureAudioBuffer(waveFormat, bufferSize, audioCaptureClient, sleepMilliseconds);
            }

            audioClient.Stop();
        }

        private void CaptureAudioBuffer(WaveFormat waveFormat, uint bufferSize, IAudioCaptureClient audioCaptureClient, int sleepMilliseconds)
        {
            var bytesPerFrame = (waveFormat.Channels * waveFormat.BitsPerSample / 8);
            byte[] recordBuffer = new byte[bufferSize * bytesPerFrame]; ;
            int recordBufferOffset = 0;

            var numFramesInNextPacket = GetNumberOfFramesInNextPacket(audioCaptureClient);

            while (numFramesInNextPacket != 0)
            {
                CopyAudioBuffer(audioCaptureClient, bytesPerFrame, recordBuffer, ref recordBufferOffset, ref numFramesInNextPacket);
            }

            _audioBufferCapturedEventHandler(new AudioBufferCapturedEventArgs(recordBuffer, recordBufferOffset));
        }

        private static void CopyAudioBuffer(IAudioCaptureClient audioCaptureClient, int bytesPerFrame, byte[] recordBuffer, ref int recordBufferOffset, ref int numFramesInNextPacket)
        {
            IntPtr dataBuffer;
            int numFramesToRead;
            AudioClientBufferFlags bufferFlags;
            long devicePosition;
            long qpcPosition;
            var buffer = audioCaptureClient.GetBuffer(out dataBuffer, out numFramesToRead, out bufferFlags, out devicePosition, out qpcPosition);

            var bytesAvailable = (numFramesInNextPacket * bytesPerFrame);

            Marshal.Copy(dataBuffer, recordBuffer, recordBufferOffset, bytesAvailable);

            recordBufferOffset += bytesAvailable;
            audioCaptureClient.ReleaseBuffer(numFramesToRead);
            audioCaptureClient.GetNextPacketSize(out numFramesInNextPacket);
        }

        private int GetNumberOfFramesInNextPacket(IAudioCaptureClient audioCaptureClient)
        {
            int numFramesInNextPacket;
            audioCaptureClient.GetNextPacketSize(out numFramesInNextPacket);
            return numFramesInNextPacket;
        }

        private int CalculateCaptureDelay(WaveFormat waveFormat, uint bufferSize)
        {
            long actualDuration = (long)((double)CoreAudio.Components.MMDevice.Constants.REFTIMES_PER_SEC * bufferSize / waveFormat.SampleRate);
            
            int sleepMilliseconds = (int)(actualDuration / CoreAudio.Components.MMDevice.Constants.REFTIMES_PER_MILLISEC / 2);

            return sleepMilliseconds;
        }
    }
}
