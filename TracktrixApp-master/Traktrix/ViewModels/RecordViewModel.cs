using DemoApp.Common;
using DemoApp.CoreAudio.Common;
using DemoApp.Services;
using DemoApp.Services.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows.Input;
using Traktrix.AudioPlayer;

namespace DemoApp.ViewModels
{
    public class RecordViewModel : BindableBase
    {
        private readonly IAudioCaptureStream _audioCaptureStream;
        private readonly IWaveFileWriter _waveFileWriter;
        private BlockingCollection<AudioBufferCapturedEventArgs> _recordedAudioBuffer;

        private bool _isRecording;
        public bool IsRecording
        {
            get { return _isRecording; }
            set { SetProperty(ref _isRecording, value); }
        }

        public RecordViewModel()
        {
            _audioCaptureStream = new AudioCaptureStream(OnAudioStreamActivated, OnAudioBufferCaptured);
            _waveFileWriter = new WaveFileWriter();

            _recordedAudioBuffer = new BlockingCollection<AudioBufferCapturedEventArgs>(new ConcurrentQueue<AudioBufferCapturedEventArgs>());

            StartRecordingCommand = new DelegateCommand(StartRecording, CanStartRecording);
            StopRecordingCommand = new DelegateCommand(StopRecording, CanStopRecording);

            IsRecording = false;
        }

        #region StartRecordingCommand

        public ICommand StartRecordingCommand { get; private set; }

        private bool CanStartRecording(object arg)
        {
            if (arg == null) return false;

            var isRecording = (bool)arg;

            return !isRecording;
        }

        private void StartRecording(object obj) 
        {
            IsRecording = true;

            //AudioPlayer.Instance.StopSong();
            _audioCaptureStream.Start();
            //AudioPlayer.Instance.PlaySong();
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

            while (IsRecording || _recordedAudioBuffer.Count > 0)
            {
                capturedAudioBuffer = _recordedAudioBuffer.Take();
                _waveFileWriter.Write(capturedAudioBuffer.Buffer, capturedAudioBuffer.BytesRecorded);
            }

            _waveFileWriter.End();
        }

        #endregion

        #region StopRecordingCommand

        public ICommand StopRecordingCommand { get; private set; }

        private bool CanStopRecording(object arg)
        {
            if (arg == null) return false;

            var isRecording = (bool)arg;

            return isRecording;
        }

        private void StopRecording(object obj)
        {
            _audioCaptureStream.Stop();

            IsRecording = false;
        }

        #endregion
    }
}
