using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traktrix.Filters
{


    abstract class Filter
    {
        //Filter Controls (with their defaults)
        public static int LowPassCutOff = 2000;
        public static int HighPassCutOff = 2000;
        public static int BandStopLowerCutOff = 800;
        public static int BandStopHigherCutOff = 4000;
        public static int BandPassLowerCutOff = 200;
        public static int BandPassHigherCutOff = 1500;
        public static int HighShelfCutoff = 1000;
        public static int HighShelfGain = 3;
        public static int LowShelfCutoff = 200;
        public static int LowShelfGain = 3;
        public static int NotchCutoff = 300;
        
        public static byte[] DoublesToBytes(double[] incoming, int bf)
        {
            int index = 0;
            short tmp = 0;
            byte[] output = new Byte[bf];
            for (int i = 0; i < output.Length / 2; i++)
            {
                tmp = (short)Math.Round(incoming[i]);
                output[index] = (byte)((short)tmp & 255);
                output[index + 1] = (byte)((((short)tmp) >> 8) & 255);
                index += 2;
            }
            return output;
        }

        public static double[] BytesToDoubles(byte[] incoming)
        {
            //double[] sampleBuffer = new double[Traktrix.Filters.DSP.Utilities.NextPowerOfTwo((uint)incoming.Length)];
            double[] sampleBuffer = new double[incoming.Length / 2];
            int index = 0;
            for (int i = 0; i < incoming.Length; i += 2)
            {
                try
                {
                    if (index <= sampleBuffer.Length - 1)
                    {
                        sampleBuffer[index] = Convert.ToDouble(BitConverter.ToInt16(incoming, i));
                        index++;
                    }
                }
                catch (Exception e1)
                {
                    System.Diagnostics.Debug.WriteLine(e1.StackTrace);
                }
            }
            return sampleBuffer;
        }

        public static byte[] ApplyFilter(byte[] playbuffer, int FilterStatus, int BUFFERSIZE)
        {
            //int extra = playbuffer.Length % 4;
            //if (extra != 0)
            //{
            //    MemoryStream tempholder = new MemoryStream();
            //    tempholder.Read(playbuffer, 0, playbuffer.Length - extra);
            //    playbuffer = tempholder.ToArray();
            //}

            if (FilterStatus == 0)
            {
                // None , No filter : Default 
            }
            else if (FilterStatus == 1)
            {
                //Center Channel Filter Activated!
                playbuffer = CenterChannelInversionFilter.FilterTheBuffer(playbuffer);
            }
            else if (FilterStatus == 2)
            {
                //HighPass Filter Activated
                playbuffer = HighPassFilter.FilterTheBuffer(playbuffer, BUFFERSIZE, HighPassCutOff);
            }
            else if (FilterStatus == 3)
            {
                //LowPass Filter Activated
                playbuffer = LowPassFilter.FilterTheBuffer(playbuffer, BUFFERSIZE, LowPassCutOff);
            }
            else if (FilterStatus == 4)
            {
                playbuffer = HighShelfFilter.FilterTheBuffer(playbuffer, BUFFERSIZE, HighPassCutOff);
            }
            else if (FilterStatus == 5)
            {
                //LowShelf
                playbuffer = LowShelfFilter.FilterTheBuffer(playbuffer, BUFFERSIZE, LowPassCutOff);

            }
            else if (FilterStatus == 6)
            {
                playbuffer = BandPassFilter.FilterTheBuffer(playbuffer, BUFFERSIZE, BandStopLowerCutOff, BandStopHigherCutOff);
            }
            else if (FilterStatus == 7)
            {
                //BandStopActivated
                playbuffer = BandStopFilter.FilterTheBuffer(playbuffer, BUFFERSIZE, BandStopLowerCutOff, BandStopHigherCutOff);
            }
            else if (FilterStatus == 8)
            {
                //hahaha
                playbuffer = CenterCutFFTFilter.FilterTheBuffer(playbuffer);
            }
            else if(FilterStatus ==9){
                playbuffer = NotchFilter.FilterTheBuffer(playbuffer, BUFFERSIZE, NotchCutoff);
            }
            return playbuffer;
        }

        public static float DoubletoFloat(Double value)
        {
            float result = Convert.ToSingle(value);
            if (float.IsPositiveInfinity(result))
            {
                result = float.MaxValue;
            }
            else if (float.IsNegativeInfinity(result))
            {
                result = float.MinValue;
            }
            return result;
        }

        public static double FloattoDouble(float value)
        {
            double result = Convert.ToDouble(value);
            return result;
        }
    }
}
