using DemoApp.CoreAudio.Common;
using System;

namespace DemoApp.Services.Interfaces
{
    public interface IAudioCaptureStream
    {
        void Start();
        void Stop();
    }
}
