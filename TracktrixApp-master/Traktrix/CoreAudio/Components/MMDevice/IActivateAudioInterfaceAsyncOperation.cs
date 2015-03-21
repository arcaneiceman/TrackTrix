using System;
using System.Runtime.InteropServices;

namespace DemoApp.CoreAudio.Components.MMDevice
{
    /// <summary>
    /// Represents an asynchronous operation activating a WASAPI interface and provides a method to retrieve the results of the activation.
    /// 
    /// Guid defined in Windows Kits\8.0\Include\um\mmdeviceapi.h
    /// </summary>
    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("72A22D78-CDE4-431D-B8CC-843A71199B6D")]
    public interface IActivateAudioInterfaceAsyncOperation
    {
        /// <summary>
        /// Gets the results of an asynchronous activation of a WASAPI interface initiated by an application calling the ActivateAudioInterfaceAsync function.
        /// </summary>
        /// <param name="activateResult">result code</param>
        /// <param name="activateInterface"></param>
        void GetActivateResult([Out] out int activateResult,
                               [Out, MarshalAs(UnmanagedType.IUnknown)] out object activatedInterface);
    }
}
