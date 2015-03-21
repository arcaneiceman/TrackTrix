using System;
using System.Runtime.InteropServices;

namespace DemoApp.CoreAudio.Components.WASAPI
{
    /// <summary>
    /// Guid defined in Windows Kits\8.0\Include\um\Audioclient.h
    /// </summary>
    [Guid(Constants.IID_IAudioCaptureClient), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioCaptureClient
    {
        int GetBuffer(
            out IntPtr dataBuffer,
            out int numFramesToRead,
            out AudioClientBufferFlags bufferFlags,
            out long devicePosition,
            out long qpcPosition);

        int ReleaseBuffer(int numFramesRead);

        int GetNextPacketSize(out int numFramesInNextPacket);

    }
}
