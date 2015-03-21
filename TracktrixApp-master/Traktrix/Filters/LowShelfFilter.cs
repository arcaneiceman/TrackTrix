using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using NAudio.Dsp;

namespace Traktrix.Filters
{
    class LowShelfFilter : Filter
    {
        internal static byte[] FilterTheBuffer(byte[] input, int Bf, int cutoff)
        {

            double[] AudioinDouble = BytesToDoubles(input);
            return DoublesToBytes(LowShelf(AudioinDouble, cutoff), Bf);
        }
        private static double[] LowShelf(double[] audioData, int cutoff)
        {
            var LowShelf = BiQuadFilter.LowShelf(44100, cutoff, 1f, -4f);
            float temp;
            for (int i = 0; i < audioData.Length; i++)
            {
                audioData[i] = audioData[i] * 0.5d;
                temp = DoubletoFloat(audioData[i]);
                temp = LowShelf.Transform(temp);
                audioData[i] = FloattoDouble(temp);
            }
            return audioData;
        }


    }
}
