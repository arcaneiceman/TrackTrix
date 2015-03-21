using System;

namespace DemoApp.CoreAudio.Constants
{
    [Flags]
    public enum AudioClientStreamFlags
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// The audio stream will be a member of a cross-process audio session (AUDCLNT_STREAMFLAGS_CROSSPROCESS)
        /// </summary>
        CrossProcess = 0x00010000,

        /// <summary>
        /// The audio stream will operate in loopback mode (AUDCLNT_STREAMFLAGS_LOOPBACK)
        /// </summary>
        Loopback = 0x00020000,

        /// <summary>
        /// Processing of the audio buffer by the client will be event driven (AUDCLNT_STREAMFLAGS_EVENTCALLBACK)
        /// </summary>
        EventCallback = 0x00040000,

        /// <summary>
        /// The volume and mute settings for an audio session will not persist (AUDCLNT_STREAMFLAGS_NOPERSIST)
        /// </summary>
        NoPersist = 0x00080000,

        /// <summary>
        /// This constant is new in Windows 7. The sample rate of the stream is adjusted to a rate specified by an application (AUDCLNT_STREAMFLAGS_RATEADJUST)
        /// </summary>
        RateAdjust = 0x00100000
    }
}
