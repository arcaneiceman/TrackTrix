using System;
using System.Runtime.InteropServices;

namespace DemoApp.CoreAudio.Components.MMDevice
{
    /// <summary>
    /// Wrapper for Mmdevapi.dll, used to get an audio client for capturing
    /// 
    /// http://msdn.microsoft.com/en-us/library/windows/desktop/dd316556(v=vs.85).aspx
    /// </summary>
    public class WindowsMultimediaDevice
    {

        /// <summary>
        /// Enables Windows Store apps to access preexisting Component Object Model (COM) interfaces in the WASAPI family.
        /// 
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/jj128298(v=vs.85).aspx
        /// </summary>
        /// <param name="deviceInterfacePath">A device interface ID for an audio device. 
        ///     This is normally retrieved from a DeviceInformation object or one of the methods of the MediaDevice class.</param>
        /// <param name="riid">The IID of a COM interface in the WASAPI family, such as IAudioClient.</param>
        /// <param name="activationParams">Interface-specific activation parameters.</param>
        /// <param name="completionHandler">An interface implemented by the caller that is called by Windows when the result of the activation procedure is available.</param>
        /// <param name="createAsync">Returns an IActivateAudioInterfaceAsyncOperation interface that represents the asynchronous operation of activating the requested WASAPI interface.</param>
        [DllImport("Mmdevapi.dll", ExactSpelling = true, PreserveSig = false)]
        public static extern void ActivateAudioInterfaceAsync(
            [In, MarshalAs(UnmanagedType.LPWStr)] string deviceInterfacePath,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [In] IntPtr activationParams,
            [In] IActivateAudioInterfaceCompletionHandler completionHandler,
            out IActivateAudioInterfaceAsyncOperation createAsync);
    }
}
