using System;
using System.Runtime.InteropServices;

namespace DemoApp.CoreAudio.Components.MMDevice
{
    /// <summary>
    /// Provides a callback to indicate that activation of a WASAPI interface is complete.
    /// 
    /// Guid defined in Windows Kits\8.0\Include\um\mmdeviceapi.h
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("41D949AB-9862-444A-80F6-C261334DA5EB")]
    public interface IActivateAudioInterfaceCompletionHandler
    {
        /// <summary>
        /// Indicates that activation of a WASAPI interface is complete and results are available.
        /// </summary>
        /// <param name="activateOperation">An interface representing the asynchronous operation of activating the requested WASAPI interface</param>
        void ActivateCompleted(IActivateAudioInterfaceAsyncOperation activateOperation);
    }
}
