using System;
using System.Runtime.InteropServices;

namespace DemoApp.CoreAudio.Common
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
    public class WaveFormat
    {
        /// <summary>format type</summary>
        protected WaveFormatEncoding waveFormatTag;
        public WaveFormatEncoding WaveFormatTag { get { return waveFormatTag; } }

        /// <summary>number of channels</summary>
        protected short channels;
        public short Channels { get { return channels; } }

        /// <summary>sample rate</summary>
        protected int sampleRate;
        public int SampleRate { get { return sampleRate; } }

        /// <summary>for buffer estimation</summary>
        protected int averageBytesPerSecond;
        public int AverageBytesPerSecond { get { return averageBytesPerSecond; } }

        /// <summary>block size of data</summary>
        protected short blockAlign;
        public short BlockAlign { get { return blockAlign; } }

        /// <summary>number of bits per sample of mono data</summary>
        protected short bitsPerSample;
        public short BitsPerSample { get { return bitsPerSample; } }

        /// <summary>number of following bytes</summary>
        protected short extraSize;
        public short ExtraSize
        {
            get { return extraSize; }
            set { extraSize = value; }
        }
    }
}
