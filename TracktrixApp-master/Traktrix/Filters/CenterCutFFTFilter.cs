using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traktrix.Filters
{
    class CenterCutFFTFilter : Filter
    {
        public static byte[] FilterTheBuffer(byte[] input)
        {

            MemoryStream ByteStream1 = new MemoryStream();
            MemoryStream ByteStream2 = new MemoryStream();
            //splitting channels
            for (int i = 0; i < input.Length; i = i + 4)
            {
                ByteStream1.WriteByte(input[i]);
                ByteStream1.WriteByte(input[i + 1]);
                ByteStream2.WriteByte(input[i + 2]);
                ByteStream2.WriteByte(input[i + 3]);
            }
            Byte[] ChannelLeft = ByteStream1.ToArray();
            Byte[] ChannelRight = ByteStream2.ToArray();

            double[] ArrayLeft = BytesToDoubles(ChannelLeft);
            double[] ArrayRight = BytesToDoubles(ChannelRight);

            double[] LeftReal = new double[ArrayLeft.Length];
            double[] RightReal = new double[ArrayRight.Length];
            double[] LeftImaginary = new double[ArrayLeft.Length];
            double[] RightImaginary = new double[ArrayRight.Length];

            Traktrix.Filters.DSP.FourierTransform.Compute((uint)ArrayRight.Length, ArrayRight, null, RightReal, RightImaginary, false);
            Traktrix.Filters.DSP.FourierTransform.Compute((uint)ArrayLeft.Length, ArrayLeft, null, LeftReal, LeftImaginary, false);

            //METHOD 1 //
            //L-R
            for (int i = 0; i < RightReal.Length; i++)
            {
                LeftReal[i] = LeftReal[i] - RightReal[i];
                RightReal[i] = RightReal[i] - LeftReal[i];
            }

            //METHOD 2 //
            ////calculation of |L| and |R| assuming they are the same length
            //double Lmag = 0.0;
            //double Rmag = 0.0;
            //for (int i = 0; i < RightReal.Length; i++)
            //{
            //    Lmag = Lmag + (LeftReal[i]) * (LeftReal[i]);
            //    Rmag = Rmag + (RightReal[i]) * (RightReal[i]);
            //}
            //Lmag = Math.Sqrt(Lmag);
            //Rmag = Math.Sqrt(Rmag);

            //double C, a;
            ////for each
            //for (int i = 0; i < LeftReal.Length; i++)
            //{
            //    C = (LeftReal[i] / Lmag) + (RightReal[i] / Rmag);
            //    a= SolveQuadratic(C*C, C*(LeftReal[i]+RightReal[i]), (LeftReal[i]*RightReal[i]));
            //    if (a == 0)
            //    {
            //        continue;// protection against a
            //    }
            //    LeftReal[i] = LeftReal[i] - (a * C);
            //    RightReal[i] = RightReal[i] - (a * C);
            //}

            ////METHOD 3


            //double[] avgbf = new double[4];

            //for (int i = 4; i < LeftReal.Length; i++)
            //{
            //    avgbf[0] = LeftReal[i - 4];
            //    avgbf[1] = LeftReal[i - 3];
            //    avgbf[2] = LeftReal[i - 2];
            //    avgbf[3] = LeftReal[i - 1];
            //    double avg = (avgbf[0] + avgbf[1] + avgbf[2] + avgbf[3]) / 4;
            //    if (LeftReal[i] > 60000d)
            //    {
            //        LeftReal[i] = avg;
            //    }
            //}


            double[] RightAns = new double[RightReal.Length];
            double[] LeftAns = new double[LeftReal.Length];
            double[] RightIm = new double[RightReal.Length];
            double[] LeftIm = new double[LeftReal.Length];

            Traktrix.Filters.DSP.FourierTransform.Compute((uint)RightReal.Length, RightReal, null, RightAns, RightIm, true);
            Traktrix.Filters.DSP.FourierTransform.Compute((uint)LeftReal.Length, LeftReal, null, LeftAns, LeftIm, true);

            byte[] final1 = DoublesToBytes(RightAns, ChannelRight.Length);
            byte[] final2 = DoublesToBytes(LeftAns, ChannelLeft.Length);

            MemoryStream output = new MemoryStream();
            for (int i = 0; i < final1.Length; i = i + 2)
            {
                output.WriteByte(final1[i]);
                output.WriteByte(final1[i + 1]);
                output.WriteByte(final2[i]);
                output.WriteByte(final2[i + 1]);
            }
            byte[] result = output.ToArray();
            ByteStream1.Dispose();
            ByteStream2.Dispose();
            output.Dispose();
            return result;
        }



        //// quadratic equation is a second order of polynomial       equation in       a single variable 
        //// x = [ -b +/- sqrt(b^2 - 4ac) ] / 2a
        //public static double SolveQuadratic(double a, double b, double c)
        //{
        //    double sqrtpart = b * b - 4 * a * c;
        //    double x, x1, x2, img;
        //    if (sqrtpart > 0)
        //    {
        //        x1 = (-b + System.Math.Sqrt(sqrtpart)) / (2 * a);
        //        x2 = (-b - System.Math.Sqrt(sqrtpart)) / (2 * a);
        //        if (x1 > x2)
        //        {
        //            return x1;
        //        }
        //        else
        //        {
        //            return x2;
        //        }
        //    }
        //    else if (sqrtpart < 0)
        //    {
        //        return 0;
        //    }
        //    else
        //    {
        //        x = (-b + System.Math.Sqrt(sqrtpart)) / (2 * a);
        //        return x;
        //    }
        //}

    }
}
