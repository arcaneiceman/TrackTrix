using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traktrix.Filters
{
    class BandStopFilter : Filter
    {
        internal static byte[] FilterTheBuffer(byte[] input, int Bf, int cutoff1, int cutoff2)
        {
            double[] AudioinDouble = BytesToDoubles(input);
            return DoublesToBytes(BandStop(AudioinDouble, cutoff1,cutoff2), Bf);
        }
        private static double[] BandStop(double[] audioData, int cutoff1, int cutoff2)
        {

            double[] RealPart = new double[audioData.Length]; // Real part
            double[] Imaginary = new double[audioData.Length]; // Imaginary part

            double[] AnsReal = new double[audioData.Length]; // Real part
            double[] AnsImaginary = new double[audioData.Length]; // Imaginary part

            Traktrix.Filters.DSP.FourierTransform.Compute((uint)audioData.Length, audioData, null, RealPart, Imaginary, false);
            for (int i = 0; i < RealPart.Length; i += 2)
            {
                if (i < ((cutoff1 * (RealPart.Length / 2)) / 44100) * 2 || i > ((cutoff2 * (RealPart.Length / 2)) / 44100) * 2)//known Sample Rate = 44100 Hz
                    RealPart[i] = RealPart[i + 1] = 0;
            }
            Traktrix.Filters.DSP.FourierTransform.Compute((uint)RealPart.Length, RealPart, null, AnsReal, AnsImaginary, true);
            return AnsReal;
        }


    }
}
