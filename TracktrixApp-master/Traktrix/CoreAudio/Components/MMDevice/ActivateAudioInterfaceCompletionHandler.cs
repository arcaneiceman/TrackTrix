using DemoApp.CoreAudio.Common;
using DemoApp.CoreAudio.Components.WASAPI;
using DemoApp.CoreAudio.Constants;
using DemoApp.CoreAudio.Enumerations;
using System;
using System.Runtime.InteropServices;

namespace DemoApp.CoreAudio.Components.MMDevice
{
    public class ActivateAudioInterfaceCompletionHandler : IActivateAudioInterfaceCompletionHandler, IAgileObject
    {
        private Action<IAudioClient, WaveFormat> _onAudioClientActivated;

        public ActivateAudioInterfaceCompletionHandler(Action<IAudioClient, WaveFormat> onAudioClientActivated)
        {
            if (onAudioClientActivated == null) throw new ArgumentNullException("onAudioClientActivated");

            _onAudioClientActivated = onAudioClientActivated;
        }

        public void ActivateCompleted(IActivateAudioInterfaceAsyncOperation activateOperation)
        {
            int activateResult;
            object activatedInterface;
            activateOperation.GetActivateResult(out activateResult, out activatedInterface);

            if (activateResult != 0)
                throw new Exception(string.Format("Failed to activate WASAPI interface, HRESULT '{0}'", activateResult));

            var audioClient = (activatedInterface as IAudioClient);
            if (audioClient == null)
                throw new Exception(string.Format("Failed to activate audio client, expected IAudioClient but activatedInterface is '{0}'",
                    (activatedInterface != null ? activatedInterface.GetType().FullName : "null")));

            Initialize(audioClient);
        }

        private void Initialize(IAudioClient audioClient)
        {
            var waveFormat = InitializeWaveFormat(audioClient);

            InitializeAudioClient(audioClient, waveFormat);

            _onAudioClientActivated(audioClient, waveFormat);
        }

        private static void InitializeAudioClient(IAudioClient audioClient, WaveFormat waveFormat)
        {
            var audioSessionGuid = Guid.Empty;
            long hnsBufferDuration = Constants.REFTIMES_PER_MILLISEC * 100;

            Marshal.ThrowExceptionForHR(
                audioClient.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.None, hnsBufferDuration, 0, waveFormat,
                    ref audioSessionGuid));
        }

        private WaveFormat InitializeWaveFormat(IAudioClient audioClient)
        {
            IntPtr deviceFormatPointer;
            Marshal.ThrowExceptionForHR(audioClient.GetMixFormat(out deviceFormatPointer));
            var waveFormat = (WaveFormat)Marshal.PtrToStructure(deviceFormatPointer, typeof(WaveFormat));

            switch (waveFormat.WaveFormatTag)
            {
                case WaveFormatEncoding.Pcm:
                    waveFormat.ExtraSize = 0;
                    break;
                case WaveFormatEncoding.Extensible:
                    waveFormat = (WaveFormatExtensible)Marshal.PtrToStructure(deviceFormatPointer, typeof(WaveFormatExtensible));
                    break;
            }

            Marshal.FreeCoTaskMem(deviceFormatPointer);

            return waveFormat;
        }
    }
}
