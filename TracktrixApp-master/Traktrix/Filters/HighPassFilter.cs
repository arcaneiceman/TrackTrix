using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using NAudio.Dsp;

namespace Traktrix.Filters
{
    class HighPassFilter : Filter
    {

        internal static byte[] FilterTheBuffer(byte[] input, int Bf, int cutoff)
        {
            double[] AudioinDouble = BytesToDoubles(input);
            return DoublesToBytes(HighPassF(AudioinDouble, cutoff), Bf);
        }
        private static double[] HighPassF(double[] audioData, int cutoff)
        {

            //double[] RealPart = new double[audioData.Length]; // Real part
            //double[] Imaginary = new double[audioData.Length]; // Imaginary part

            //double[] AnsReal = new double[audioData.Length]; // Real part
            //double[] AnsImaginary = new double[audioData.Length]; // Imaginary part

            //Traktrix.Filters.DSP.FourierTransform.Compute((uint)audioData.Length, audioData, null, RealPart, Imaginary, false);
            //for (int i = 0; i < RealPart.Length; i += 2)
            //{
            //    if (i < ((cutoff * (RealPart.Length / 2)) / 44100) * 2)//known Sample Rate = 44100 Hz
            //        RealPart[i] = RealPart[i + 1] = 0;
                 
            //}
            //Traktrix.Filters.DSP.FourierTransform.Compute((uint)RealPart.Length, RealPart, null, AnsReal, AnsImaginary, true);
            //return AnsReal;

            var HighPass = BiQuadFilter.HighPassFilter(44100, cutoff, 4f);
            float temp;
            for (int i = 0; i < audioData.Length; i++)
            {
                audioData[i] = audioData[i] * 0.5d;
                temp = DoubletoFloat(audioData[i]);
                temp = HighPass.Transform(temp);
                audioData[i] = FloattoDouble(temp);
            }
            return audioData;
        }


    }
}
