
namespace DemoApp.CoreAudio.Enumerations
{
    /// <summary>
    /// The sharing mode for a stream.
    /// 
    /// Defined in Windows Kits\8.0\Include\um\AudioSessionTypes.h
    /// </summary>
    public enum AudioClientShareMode
    {
        /// <summary>
        /// AUDCLNT_SHAREMODE_SHARED
        /// </summary>
        Shared,
        /// <summary>
        /// AUDCLNT_SHAREMODE_EXCLUSIVE
        /// </summary>
        Exclusive,
    }
}
