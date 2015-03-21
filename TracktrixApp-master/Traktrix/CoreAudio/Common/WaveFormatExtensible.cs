using System;
using System.Runtime.InteropServices;

namespace DemoApp.CoreAudio.Common
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormatExtensible : WaveFormat
    {
        short wValidBitsPerSample;

        int dwChannelMask;

        Guid subFormat;
    }
}
