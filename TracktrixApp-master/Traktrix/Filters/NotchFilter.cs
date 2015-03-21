using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using NAudio.Dsp;

namespace Traktrix.Filters
{
    class NotchFilter : Filter
    {
        internal static byte[] FilterTheBuffer(byte[] input, int Bf, int centerFreq)
        {

            double[] AudioinDouble = BytesToDoubles(input);
            return DoublesToBytes(Notch(AudioinDouble, centerFreq), Bf);
        }
        private static double[] Notch(double[] audioData, int centerFreq)
        {
            var LowShelf = BiQuadFilter.NotchFilter(44100, centerFreq, 4f);
            float temp;
            for (int i = 0; i < audioData.Length; i++)
            {
                temp = DoubletoFloat(audioData[i]);
                temp = LowShelf.Transform(temp);
                audioData[i] = FloattoDouble(temp);
            }
            return audioData;
        }


    }
}
