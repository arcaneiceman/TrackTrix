using DemoApp.CoreAudio.Common;
using DemoApp.CoreAudio.Constants;
using DemoApp.CoreAudio.Enumerations;
using System;
using System.Runtime.InteropServices;

namespace DemoApp.CoreAudio.Components.WASAPI
{
    /// <summary>
    /// The IAudioClient interface enables a client to create and initialize an audio stream between an audio application and the audio engine (for a shared-mode stream) 
    /// or the hardware buffer of an audio endpoint device (for an exclusive-mode stream). 
    /// 
    /// Guid defined in Windows Kits\8.0\Include\um\Audioclient.h
    /// </summary>
    [Guid(Constants.IID_IAudioClient), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioClient
    {
        /// <summary>
        /// Initializes the audio stream.
        /// </summary>
        /// <param name="shareMode">The sharing mode for the connection. 
        ///     Through this parameter, the client tells the audio engine whether it wants to share the audio endpoint device with other clients. 
        ///     The client should set this parameter to one of the following: AUDCLNT_SHAREMODE_EXCLUSIVE, AUDCLNT_SHAREMODE_SHARED</param>
        /// <param name="StreamFlags">Flags to control creation of the stream. 
        ///     The client should set this parameter to 0 or to the bitwise OR of one or more of the AUDCLNT_STREAMFLAGS_XXX Constants or the AUDCLNT_SESSIONFLAGS_XXX Constants.</param>
        /// <param name="hnsBufferDuration">The buffer capacity as a time value. 
        ///     This parameter is of type REFERENCE_TIME and is expressed in 100-nanosecond units. 
        ///     This parameter contains the buffer size that the caller requests for the buffer that the audio application will share with the audio engine (in shared mode) 
        ///     or with the endpoint device (in exclusive mode). If the call succeeds, the method allocates a buffer that is a least this large.</param>
        /// <param name="hnsPeriodicity">The device period. 
        ///     This parameter can be nonzero only in exclusive mode. 
        ///     In shared mode, always set this parameter to 0. 
        ///     In exclusive mode, this parameter specifies the requested scheduling period for successive buffer accesses by the audio endpoint device. 
        ///     If the requested device period lies outside the range that is set by the device's minimum period and the system's maximum period, then the method clamps the period to that range. 
        ///     If this parameter is 0, the method sets the device period to its default value. 
        ///     To obtain the default device period, call the IAudioClient::GetDevicePeriod method. 
        ///     If the AUDCLNT_STREAMFLAGS_EVENTCALLBACK stream flag is set and AUDCLNT_SHAREMODE_EXCLUSIVE is set as the ShareMode, 
        ///     then hnsPeriodicity must be nonzero and equal to hnsBufferDuration.</param>
        /// <param name="pFormat">Pointer to a format descriptor. This parameter must point to a valid format descriptor of type WAVEFORMATEX (or WAVEFORMATEXTENSIBLE). </param>
        /// <param name="AudioSessionGuid">Pointer to a session GUID. 
        ///     This parameter points to a GUID value that identifies the audio session that the stream belongs to. 
        ///     If the GUID identifies a session that has been previously opened, the method adds the stream to that session. 
        ///     If the GUID does not identify an existing session, the method opens a new session and adds the stream to that session. 
        ///     The stream remains a member of the same session for its lifetime. Setting this parameter to NULL is equivalent to passing a pointer to a GUID_NULL value.</param>
        /// <returns></returns>
        [PreserveSig]
        int Initialize(AudioClientShareMode shareMode,
            AudioClientStreamFlags StreamFlags,
            long hnsBufferDuration,
            long hnsPeriodicity,
            [In] WaveFormat pFormat,
            [In] ref Guid AudioSessionGuid);

        /// <summary>
        /// The GetBufferSize method retrieves the size (maximum capacity) of the endpoint buffer.
        /// </summary>
        /// <param name="bufferSize">Pointer to a UINT32 variable into which the method writes the number of audio frames that the buffer can hold.</param>
        int GetBufferSize(out uint bufferSize);

        /// <summary>
        /// The GetStreamLatency method retrieves the maximum latency for the current stream and can be called any time after the stream has been initialized.
        /// </summary>
        /// <returns>Pointer to a REFERENCE_TIME variable into which the method writes a time value representing the latency. The time is expressed in 100-nanosecond units.</returns>
        [return: MarshalAs(UnmanagedType.I8)]
        long GetStreamLatency();

        /// <summary>
        /// The GetCurrentPadding method retrieves the number of frames of padding in the endpoint buffer.
        /// </summary>
        /// <param name="currentPadding">Pointer to a UINT32 variable into which the method writes the frame count (the number of audio frames of padding in the buffer).</param>
        int GetCurrentPadding(out int currentPadding);

        /// <summary>
        /// The IsFormatSupported method indicates whether the audio endpoint device supports a particular stream format.
        /// </summary>
        /// <param name="shareMode">The sharing mode for the stream format. 
        ///     Through this parameter, the client indicates whether it wants to use the specified format in exclusive mode or shared mode. 
        ///     The client should set this parameter to one of the following AUDCLNT_SHAREMODE enumeration values: AUDCLNT_SHAREMODE_EXCLUSIVE, AUDCLNT_SHAREMODE_SHARED
        /// </param>
        /// <param name="pFormat">Pointer to the specified stream format. 
        ///     This parameter points to a caller-allocated format descriptor of type WAVEFORMATEX or WAVEFORMATEXTENSIBLE. 
        ///     The client writes a format description to this structure before calling this method.</param>
        /// <param name="closestMatchFormat">Pointer to a pointer variable into which the method writes the address of a WAVEFORMATEX or WAVEFORMATEXTENSIBLE structure. 
        ///     This structure specifies the supported format that is closest to the format that the client specified through the pFormat parameter. 
        ///     For shared mode (that is, if the ShareMode parameter is AUDCLNT_SHAREMODE_SHARED), set ppClosestMatch to point to a valid, non-NULL pointer variable. 
        ///     For exclusive mode, set ppClosestMatch to NULL. 
        ///     The method allocates the storage for the structure. 
        ///     The caller is responsible for freeing the storage, when it is no longer needed, by calling the CoTaskMemFree function. 
        ///     If the IsFormatSupported call fails and ppClosestMatch is non-NULL, the method sets *ppClosestMatch to NULL.</param>
        /// <returns></returns>
        [PreserveSig]
        int IsFormatSupported(
            AudioClientShareMode shareMode,
            [In] WaveFormat pFormat,
            [Out, MarshalAs(UnmanagedType.LPStruct)] out WaveFormatExtensible closestMatchFormat);

        /// <summary>
        /// The GetMixFormat method retrieves the stream format that the audio engine uses for its internal processing of shared-mode streams.
        /// </summary>
        /// <param name="deviceFormatPointer">Pointer to a pointer variable into which the method writes the address of the mix format. 
        ///     This parameter must be a valid, non-NULL pointer to a pointer variable. 
        ///     The method writes the address of a WAVEFORMATEX (or WAVEFORMATEXTENSIBLE) structure to this variable. 
        ///     The method allocates the storage for the structure. 
        ///     The caller is responsible for freeing the storage, when it is no longer needed, by calling the CoTaskMemFree function. 
        ///     If the GetMixFormat call fails, *ppDeviceFormat is NULL.</param>
        int GetMixFormat(out IntPtr deviceFormatPointer);

        /// <summary>
        /// The GetDevicePeriod method retrieves the length of the periodic interval separating successive processing passes by the audio engine on the data in the endpoint buffer.
        /// </summary>
        /// <param name="defaultDevicePeriod">Pointer to a REFERENCE_TIME variable into which the method writes a time value specifying the default interval 
        ///     between periodic processing passes by the audio engine. The time is expressed in 100-nanosecond units. </param>
        /// <param name="minimumDevicePeriod">Pointer to a REFERENCE_TIME variable into which the method writes a time value specifying the minimum interval 
        ///     between periodic processing passes by the audio endpoint device. The time is expressed in 100-nanosecond units.</param>
        int GetDevicePeriod(out long defaultDevicePeriod, out long minimumDevicePeriod);

        /// <summary>
        /// The Start method starts the audio stream.
        /// </summary>
        int Start();

        /// <summary>
        /// The Stop method stops the audio stream.
        /// </summary>
        int Stop();

        /// <summary>
        /// The Reset method resets the audio stream.
        /// </summary>
        int Reset();

        /// <summary>
        /// The SetEventHandle method sets the event handle that the system signals when an audio buffer is ready to be processed by the client.
        /// </summary>
        /// <param name="eventHandle">The event handle.</param>
        int SetEventHandle(IntPtr eventHandle);

        /// <summary>
        /// The GetService method accesses additional services from the audio client object.
        /// </summary>
        /// <param name="interfaceId">The interface ID for the requested service.</param>
        /// <param name="interfacePointer">Pointer to a pointer variable into which the method writes the address of an instance of the requested interface. </param>
        [PreserveSig]
        int GetService([In, MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId, [Out, MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);
    }
}
