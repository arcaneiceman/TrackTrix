using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using NAudio.Dsp;

namespace Traktrix.Filters
{
    class HighShelfFilter : Filter
    {
        internal static byte[] FilterTheBuffer(byte[] input, int Bf, int cutoff)
        {
            double[] AudioinDouble = BytesToDoubles(input);
            return DoublesToBytes(HighShelf(AudioinDouble, cutoff), Bf);
        }
        private static double[] HighShelf(double[] audioData, int cutoff)
        {
            var HighShelf = BiQuadFilter.HighShelf(44100, cutoff, 1f, 1f);
            float temp;
            for (int i = 0; i < audioData.Length; i++)
            {
                temp = DoubletoFloat(audioData[i]);
                temp = HighShelf.Transform(temp);
                audioData[i] = FloattoDouble(temp);
            }
            return audioData;
        }


    }
}
