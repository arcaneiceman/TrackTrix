using System;
using System.Collections.Generic;
using System.Text;
//using System.IO.IsolatedStorage;
using System.IO;

// http://afni.nimh.nih.gov/pub/dist/src/Haar.c

namespace Traktrix.Filters.DSP
{
    //public class MFCC
    //{
    //    private static double[] melWorkingFrequencies = new double[11] { 10.0, 20.0, 90.0, 300.0, 680.0, 1270.0, 2030.0, 2970.0, 4050.0, 5250.0, 6570.0 };

    //    public static int numMelFilters(int Nyquist)
    //    {
    //        //System.Diagnostics.Debug.WriteLine("Nyquist:" + Convert.ToString(Nyquist));
    //        double frequency = Nyquist;
    //        double delta = mel(frequency);

    //        int numFilters = 0;

    //        while (frequency > 10)
    //        {
    //            ++numFilters;

    //            frequency -= (delta / 2);
    //            delta = DSP.MFCC.mel(frequency);

    //            //System.Diagnostics.Debug.WriteLine("Frequency:" + Convert.ToString(frequency));
    //        }

    //        return numFilters;
    //    }

    //    /// <summary>
    //    /// Init
    //    /// </summary>
    //    /// <param name="sampleSize"></param>
    //    /// 
    //    public static void initMelFrequenciesRange(int Nyquist)
    //    {
    //        //System.Diagnostics.Debug.WriteLine("Nyquist:" + Convert.ToString(Nyquist));
    //        double frequency = Nyquist;
    //        double delta = mel(frequency);

    //        int numFilters = numMelFilters(Nyquist);

    //        melWorkingFrequencies = new double[numFilters];

    //        frequency = Nyquist;
    //        delta = mel(frequency);
    //        int i = 0;
    //        double cFreq = 0;

    //        while (frequency > 10)            
    //        {
    //            frequency -= (delta / 2);
    //            delta = mel(frequency);
    //            cFreq = Math.Round(frequency);
    //            //melWorkingFrequencies[numFilters] = Math.Round(frequency);
    //            melWorkingFrequencies[numFilters-1-i] = 10;
    //            while (melWorkingFrequencies[numFilters-1-i] < (cFreq - 10)) melWorkingFrequencies[numFilters-1-i] += 10;

    //           // System.Diagnostics.Debug.WriteLine("Frequency:" + Convert.ToString(melWorkingFrequencies[numFilters-1-i]));
    //            ++i;
    //        }

    //    }

    //    public static double[] compute(ref double[] signal)
    //    {
    //        double[] result = new double[melWorkingFrequencies.Length];
    //        double[] mfcc = new double[melWorkingFrequencies.Length];

    //        int segment = 0;
    //        int start = 0;
    //        int end = 0;

    //        for (int i = 0; i < melWorkingFrequencies.Length; i++)
    //        {
    //            result[i] = 0;
    //            segment = (int) Math.Round(mel( melWorkingFrequencies[i] ) / 10 );
    //            /*System.Diagnostics.Debug.WriteLine("slot #" + Convert.ToString(i));
    //            System.Diagnostics.Debug.WriteLine("freq:" + Convert.ToString(melWorkingFrequencies[i]));
    //            System.Diagnostics.Debug.WriteLine("mel:" + Convert.ToString(mel(melWorkingFrequencies[i])));
    //            System.Diagnostics.Debug.WriteLine("segment:"+Convert.ToString(segment));*/

    //            start = (segment - (int)Math.Floor((double)segment / 2));
    //            end = (segment + (segment / 2));
    //            //System.Diagnostics.Debug.WriteLine("\tstart:" + Convert.ToString(start) + "\tend:" + Convert.ToString(end));

    //            for (int j = start; j < end; j++)
    //            {
    //               // System.Diagnostics.Debug.WriteLine("\t\tfilter slopet:" + Convert.ToString(DSP.Filters.Triangular(j-start, segment)));
    //                result[i] += signal[j] * DSP.Filters.Triangular(j, segment);
    //            }
    //            //System.Diagnostics.Debug.WriteLine("result[i]:" + Convert.ToString(result[i]));
    //            result[i] = (result[i] > 0)  ? Math.Log10( Math.Abs(result[i]) ) : 0;
    //        }

    //        for (int i = 0; i < melWorkingFrequencies.Length; i++)
    //        {
    //            for (int j = 0; j < melWorkingFrequencies.Length; j++)
    //            {
    //                mfcc[i] += result[i] * Math.Cos(((Math.PI * i) / melWorkingFrequencies.Length) * (j - 0.5));
    //            }
    //            mfcc[i] *= Math.Sqrt(2.0 / (double) melWorkingFrequencies.Length);
    //            //System.Diagnostics.Debug.WriteLine("result[i]:" + Convert.ToString(mfcc[i]));
    //        }

    //        return mfcc;
    //    }

    //    public static double mel(double value)
    //    {
    //        return (2595.0 * (double)Math.Log10(1.0 + value / 700.0));
    //    }

    //    public static double melinv(double value)
    //    {
    //        return (700.0 * ((double)Math.Pow(10.0, value / 2595.0) - 1.0));
    //    }
    //}

    public class FourierTransform
    {
        public const int Raw = 1;
        public const int Decibel = 2;

        public const int FREQUENCYSLOTCOUNT = 21;
        private static int[] _meterFrequencies = new int[FREQUENCYSLOTCOUNT] { 20, 30, 55, 80, 120, 155, 195, 250, 375, 500, 750, 1000, 1500, 2000, 3000, 4000, 6000, 8000, 12000, 16000, 20000 };
        private static int _frequencySlotCount = FREQUENCYSLOTCOUNT;

        /// <summary>
        /// 	Changes the Frequency Bands to analyze.
        /// 	Affects the other static methods
        /// </summary>
        /// <param name="meterFrequencies"></param>
        public static void SetMeterFrequencies(int[] meterFrequencies)
        {
            _meterFrequencies = meterFrequencies;
            _frequencySlotCount = meterFrequencies.Length;
        }

        public static double[] Spectrum(ref double[] x, int method = Raw)
        {
            //uint pow2Samples = FFT.NextPowerOfTwo((uint)x.Length);
            double[] xre = new double[x.Length];
            double[] xim = new double[x.Length];

            Compute((uint)x.Length, x, null, xre, xim, false);

            double[] decibel = new double[xre.Length / 2];

            for (int i = 0; i < decibel.Length; i++)
                decibel[i] = (method == Decibel) ? 10.0 * Math.Log10((float)(Math.Sqrt((xre[i] * xre[i]) + (xim[i] * xim[i])))) : (float)(Math.Sqrt((xre[i] * xre[i]) + (xim[i] * xim[i])));
            return decibel;
        }


        /// <summary>
        /// Get Number of bits needed for a power of two
        /// </summary>
        /// <param name="PowerOfTwo">Power of two number</param>
        /// <returns>Number of bits</returns>
        public static UInt32 NumberOfBitsNeeded(UInt32 PowerOfTwo)
        {
            if (PowerOfTwo > 0)
            {
                for (UInt32 i = 0, mask = 1; ; i++, mask <<= 1)
                {
                    if ((PowerOfTwo & mask) != 0)
                        return i;
                }
            }
            return 0; // error
        }

        /// <summary>
        /// Reverse bits
        /// </summary>
        /// <param name="index">Bits</param>
        /// <param name="NumBits">Number of bits to reverse</param>
        /// <returns>Reverse Bits</returns>
        public static UInt32 ReverseBits(UInt32 index, UInt32 NumBits)
        {
            UInt32 i, rev;

            for (i = rev = 0; i < NumBits; i++)
            {
                rev = (rev << 1) | (index & 1);
                index >>= 1;
            }

            return rev;
        }

        /// <summary>
        /// Return index to frequency based on number of samples
        /// </summary>
        /// <param name="Index">sample index</param>
        /// <param name="NumSamples">number of samples</param>
        /// <returns>Frequency index range</returns>
        public static Double IndexToFrequency(UInt32 Index, UInt32 NumSamples)
        {
            if (Index >= NumSamples)
                return 0.0;
            else if (Index <= NumSamples / 2)
                return (double)Index / (double)NumSamples;

            return -(double)(NumSamples - Index) / (double)NumSamples;
        }

        /// <summary>
        /// Compute FFT
        /// </summary>
        /// <param name="NumSamples">NumSamples Number of samples (must be power two)</param>
        /// <param name="pRealIn">Real samples</param>
        /// <param name="pImagIn">Imaginary (optional, may be null)</param>
        /// <param name="pRealOut">Real coefficient output</param>
        /// <param name="pImagOut">Imaginary coefficient output</param>
        /// <param name="bInverseTransform">bInverseTransform when true, compute Inverse FFT</param>
        public static void Compute(UInt32 NumSamples, Double[] pRealIn, Double[] pImagIn,
                                                Double[] pRealOut, Double[] pImagOut, Boolean bInverseTransform)
        {
            UInt32 NumBits;    /* Number of bits needed to store indices */
            UInt32 i, j, k, n;
            UInt32 BlockSize, BlockEnd;

            double angle_numerator = 2.0 * Utilities.DDC_PI;
            double tr, ti;     /* temp real, temp imaginary */

            if (pRealIn == null || pRealOut == null || pImagOut == null)
            {
                // error
                throw new ArgumentNullException("Null argument");
            }
            if (!Utilities.IsPowerOfTwo(NumSamples))
            {
                // error
                throw new ArgumentException("Number of samples must be power of 2");
            }
            if (pRealIn.Length < NumSamples || (pImagIn != null && pImagIn.Length < NumSamples) ||
                     pRealOut.Length < NumSamples || pImagOut.Length < NumSamples)
            {
                // error
                throw new ArgumentException("Invalid Array argument detected");
            }

            if (bInverseTransform)
                angle_numerator = -angle_numerator;

            NumBits = NumberOfBitsNeeded(NumSamples);

            /*
            **   Do simultaneous data copy and bit-reversal ordering into outputs...
            */
            for (i = 0; i < NumSamples; i++)
            {
                j = ReverseBits(i, NumBits);
                pRealOut[j] = pRealIn[i];
                pImagOut[j] = (double)((pImagIn == null) ? 0.0 : pImagIn[i]);
            }

            /*
            **   Do the FFT itself...
            */
            BlockEnd = 1;
            for (BlockSize = 2; BlockSize <= NumSamples; BlockSize <<= 1)
            {
                double delta_angle = angle_numerator / (double)BlockSize;
                double sm2 = Math.Sin(-2 * delta_angle);
                double sm1 = Math.Sin(-delta_angle);
                double cm2 = Math.Cos(-2 * delta_angle);
                double cm1 = Math.Cos(-delta_angle);
                double w = 2 * cm1;
                double ar0, ar1, ar2;
                double ai0, ai1, ai2;

                for (i = 0; i < NumSamples; i += BlockSize)
                {
                    ar2 = cm2;
                    ar1 = cm1;

                    ai2 = sm2;
                    ai1 = sm1;

                    for (j = i, n = 0; n < BlockEnd; j++, n++)
                    {
                        ar0 = w * ar1 - ar2;
                        ar2 = ar1;
                        ar1 = ar0;

                        ai0 = w * ai1 - ai2;
                        ai2 = ai1;
                        ai1 = ai0;

                        k = j + BlockEnd;
                        tr = ar0 * pRealOut[k] - ai0 * pImagOut[k];
                        ti = ar0 * pImagOut[k] + ai0 * pRealOut[k];

                        pRealOut[k] = (pRealOut[j] - tr);
                        pImagOut[k] = (pImagOut[j] - ti);

                        pRealOut[j] += (tr);
                        pImagOut[j] += (ti);
                    }
                }

                BlockEnd = BlockSize;
            }

            /*
            **   Need to normalize if inverse transform...
            */
            if (bInverseTransform)
            {
                double denom = (double)(NumSamples);

                for (i = 0; i < NumSamples; i++)
                {
                    pRealOut[i] /= denom;
                    pImagOut[i] /= denom;
                }
            }
        }

        /// <summary>
        /// Calculate normal (power spectrum)
        /// </summary>
        /// <param name="NumSamples">Number of sample</param>
        /// <param name="pReal">Real coefficient buffer</param>
        /// <param name="pImag">Imaginary coefficient buffer</param>
        /// <param name="pAmpl">Working buffer to hold amplitude Xps(m) = | X(m)^2 | = Xreal(m)^2  + Ximag(m)^2</param>
        public static void Norm(UInt32 NumSamples, Double[] pReal, Double[] pImag, Double[] pAmpl)
        {
            if (pReal == null || pImag == null || pAmpl == null)
            {
                // error
                throw new ArgumentNullException("pReal,pImag,pAmpl");
            }
            if (pReal.Length < NumSamples || pImag.Length < NumSamples || pAmpl.Length < NumSamples)
            {
                // error
                throw new ArgumentException("Invalid Array argument detected");
            }

            // Calculate amplitude values in the buffer provided
            for (UInt32 i = 0; i < NumSamples; i++)
            {
                pAmpl[i] = pReal[i] * pReal[i] + pImag[i] * pImag[i];
            }
        }

        public static double normalizeFFTValue(double value)
        {
            return (value < 0.1 && value > -0.1) ? 0 : value;
        }

        /// <summary>
        /// Compute 2D FFT
        /// </summary>
        /// <param name="width">Width of the Matrix (must be power two)</param>
        /// <param name="height">Height of the Matrix (must be power two)</param>
        /// <param name="pRealIn">Real samples</param>
        /// <param name="pImagIn">Imaginary (optional, may be null)</param>
        /// <param name="pRealOut">Real coefficient output</param>
        /// <param name="pImagOut">Imaginary coefficient output</param>
        /// <param name="bInverseTransform">bInverseTransform when true, compute Inverse FFT</param>
        public static void Compute2D(UInt32 width, UInt32 height, ref Double[] pRealIn, Double[] pImagIn, ref Double[] pRealOut, ref Double[] pImagOut, Boolean bInverseTransform = false)
        {
            double[] row = new double[width];
            double[] column = new double[height];

            double[] irow = new double[width];
            double[] icolumn = new double[height];

            double[] xre = new double[width];
            double[] xim = new double[width];

            if (!bInverseTransform)
            {
                for (UInt32 y = 0; y < height; y++)
                {
                    Array.Copy(pRealIn, (int)(y * width), row, 0, (int)width);
                    DSP.FourierTransform.Compute(width, row, null, xre, xim, bInverseTransform);
                    Array.Copy(xre, 0, pRealOut, (int)(y * width), (int)width);
                    Array.Copy(xim, 0, pImagOut, (int)(y * width), (int)width);
                }

                for (int x = 0; x < width; x++)
                {

                    for (int y = 0; y < height; y++)
                    {
                        column[y] = pRealOut[x + (y * width)];
                        icolumn[y] = pImagOut[x + (y * width)];
                    }

                    DSP.FourierTransform.Compute(height, column, icolumn, xre, xim, bInverseTransform);

                    for (int y = 0; y < height; y++)
                    {
                        pRealOut[x + (y * width)] = xre[y];
                        pImagOut[x + (y * width)] = xim[y];
                    }

                }
            }
            else
            {
                for (int x = 0; x < width; x++)
                {

                    for (int y = 0; y < height; y++)
                    {
                        column[y] = pRealIn[x + (y * width)];
                        icolumn[y] = pImagIn[x + (y * width)];
                    }

                    DSP.FourierTransform.Compute(height, column, icolumn, xre, xim, bInverseTransform);

                    for (int y = 0; y < height; y++)
                    {
                        pRealOut[x + (y * width)] = xre[y];
                        pImagOut[x + (y * width)] = xim[y];
                    }

                }

                for (UInt32 y = 0; y < height; y++)
                {
                    Array.Copy(pRealOut, (int)(y * width), row, 0, (int)width);
                    Array.Copy(pImagOut, (int)(y * width), irow, 0, (int)width);

                    DSP.FourierTransform.Compute(width, row, irow, xre, xim, bInverseTransform);
                    Array.Copy(xre, 0, pRealOut, (int)(y * width), (int)width);
                }
            }
        }

        /// <summary>
        /// Find Peak frequency in Hz
        /// </summary>
        /// <param name="NumSamples">Number of samples</param>
        /// <param name="pAmpl">Current amplitude</param>
        /// <param name="samplingRate">Sampling rate in samples/second (Hz)</param>
        /// <param name="index">Frequency index</param>
        /// <returns>Peak frequency in Hz</returns>
        public static Double PeakFrequency(UInt32 NumSamples, Double[] pAmpl, Double samplingRate, ref UInt32 index)
        {
            UInt32 N = NumSamples >> 1;   // number of positive frequencies. (numSamples/2)

            if (pAmpl == null)
            {
                // error
                throw new ArgumentNullException("pAmpl");
            }
            if (pAmpl.Length < NumSamples)
            {
                // error
                throw new ArgumentException("Invalid Array argument detected");
            }

            double maxAmpl = -1.0;
            double peakFreq = -1.0;
            index = 0;

            for (UInt32 i = 0; i < N; i++)
            {
                if (pAmpl[i] > maxAmpl)
                {
                    maxAmpl = (double)pAmpl[i];
                    index = i;
                    peakFreq = (double)(i);
                }
            }

            return samplingRate * peakFreq / (double)(NumSamples);
        }

        public static byte[] GetPeaks(double[] leftChannel, double[] rightChannel, int sampleFrequency)
        {
            byte[] peaks = new byte[_frequencySlotCount];
            byte[] channelPeaks = GetPeaksForChannel(leftChannel, sampleFrequency);

            ComparePeaks(peaks, channelPeaks);
            return peaks;
        }

        private static void ComparePeaks(byte[] overallPeaks, byte[] channelPeaks)
        {
            for (int i = 0; i < _frequencySlotCount; i++)
            {
                overallPeaks[i] = Math.Max(overallPeaks[i], channelPeaks[i]);
            }
        }

        private static byte[] GetPeaksForChannel(double[] normalizedArray, int sampleFrequency)
        {
            double maxAmpl = (32767.0 * 32767.0);

            byte[] peaks = new byte[_frequencySlotCount];
            // update meter
            int centerFreq = (sampleFrequency / 2);
            byte peak;
            for (int i = 0; i < _frequencySlotCount; ++i)
            {
                if (_meterFrequencies[i] > centerFreq)
                {
                    peak = 0;
                }
                else
                {
                    int index = (int)(_meterFrequencies[i] * normalizedArray.Length / sampleFrequency);
                    peak = (byte)Math.Max(0, (17.0 * Math.Log10(normalizedArray[index] / maxAmpl)));
                }

                peaks[i] = peak;
            }

            return peaks;
        }
    }

    public class Wavelet
    {
        // Haar Wavelet

        /// <summary>
        /// Calculate Haar in-place forward fast wavelet transform in 1-dimension.
        /// </summary>
        /// <param name="n">Elements to compute</param>
        /// <param name="signal">Signal to compute</param>
        static void Haar_ip_FFWT_1d(int n, ref double[] signal)
        {
            double a;
            double c;
            int i; int j; int k; int l; int m;

            i = 1;
            j = 2;

            m = (int)Utilities.NextPowerOfTwo((uint)n);

            for (l = n - 1; l >= 0; l--)
            {
                //System.Diagnostics.Debug.WriteLine("l = " + l);
                m /= 2;

                for (k = 0; k < m; k++)
                {
                    a = (signal[j * k] + signal[j * k + i]) / 2.0;
                    c = (signal[j * k] - signal[j * k + i]) / 2.0;
                    signal[j * k] = a;
                    signal[j * k + i] = c;
                }

                i *= 2;
                j *= 2;
            }
        }

        /// <summary>
        /// Calculate Haar in-place inverse fast wavelet transform in 1-dimension.
        /// </summary>
        /// <param name="n">Elements to compute</param>
        /// <param name="signal">Signal to compute (must be power two)</param>
        static void Haar_ip_IFWT_1d(int n, ref double[] signal)
        {
            double a0;
            double a1;
            int i;
            int j;
            int k;
            int l;
            int m;

            i = (int)Utilities.NextPowerOfTwo((uint)(n - 1));
            j = 2 * i;
            m = 1;

            for (l = 1; l <= n; l++)
            {
                //System.Diagnostics.Debug.WriteLine("l = " + l);

                for (k = 0; k < m; k++)
                {
                    a0 = signal[j * k] + signal[j * k + i];
                    a1 = signal[j * k] - signal[j * k + i];
                    signal[j * k] = a0;
                    signal[j * k + i] = a1;
                }

                i /= 2;
                j /= 2;
                m *= 2;
            }
        }

        /// <summary>
        /// Calculate one iteration of the Haar forward FWT in 1-dimension.
        /// </summary>
        /// <param name="n">Elements to compute</param>
        /// <param name="signal">Signal to compute (must be power two)</param>
        static void Haar_forward_pass_1d(int n, ref double[] signal)
        {
            int i;
            int npts;

            npts = (int)Utilities.NextPowerOfTwo((uint)n);
            double[] a = new double[npts / 2];
            double[] c = new double[npts / 2];

            for (i = 0; i < npts / 2; i++)
            {
                a[i] = (signal[2 * i] + signal[2 * i + 1]) / 2.0;
                c[i] = (signal[2 * i] - signal[2 * i + 1]) / 2.0;
            }

            for (i = 0; i < npts / 2; i++)
            {
                signal[i] = a[i];
                signal[i + npts / 2] = c[i];
            }

        }

        /// <summary>
        /// Calculate the Haar forward fast wavelet transform in 1-dimension.
        /// </summary>
        /// <param name="signal">Signal to compute (must be power two)</param>
        public static void Haar_forward_FWT_1d(ref double[] signal)
        {
            int m;
            int npts;

            npts = (int)Utilities.NextPowerOfTwo((uint)signal.Length);

            for (m = signal.Length - 1; m >= 0; m--)
            {
                Wavelet.Haar_forward_pass_1d(m + 1, ref signal);
                //Wavelet.Haar_ip_FFWT_1d(m + 1, ref signal);
            }

            /*int i = 0;
            int w = signal.Length;
            double[] vecp = new double[signal.Length];
            for (i = 0; i < signal.Length; i++)
                vecp[i] = 0;

            //while (w > 1)
           // {
                w /= 2;
                for (i = 0; i < w; i++)
                {
                    vecp[i] = (signal[2 * i] + signal[2 * i + 1]) / Math.Sqrt(2.0);
                    vecp[i + w] = (signal[2 * i] - signal[2 * i + 1]) / Math.Sqrt(2.0);
                }

                for (i = 0; i < (w * 2); i++)
                    signal[i] = vecp[i];
           // }*/

        }



        /// <summary>
        /// Calculate one iteration of the Haar inverse FWT in 1-dimension.
        /// </summary>
        /// <param name="n">Elements to compute</param>
        /// <param name="signal">Signal to compute (must be power two)</param>
        static void Haar_inverse_pass_1d(int n, ref double[] signal)
        {
            int i;
            int npts = (int)Utilities.NextPowerOfTwo((uint)n);
            double[] r = new double[npts];

            for (i = 0; i < npts / 2; i++)
            {
                r[2 * i] = signal[i] + signal[i + npts / 2];
                r[2 * i + 1] = signal[i] - signal[i + npts / 2];
            }

            for (i = 0; i < npts; i++)
            {
                signal[i] = r[i];
            }
        }


        /// <summary>
        /// Calculate the Haar inverse fast wavelet transform in 1-dimension.
        /// </summary>
        /// <param name="signal">Signal to compute (must be power two)</param>
        public static void Haar_inverse_FWT_1d(ref double[] signal)
        {
            for (int m = 2; m <= signal.Length; m++)
            {
                Wavelet.Haar_inverse_pass_1d(m, ref signal);
            }
        }


        /// <summary>
        /// Calculate one iteration of the Haar forward FWT in 2-dimensions.
        /// </summary>
        /// <param name="n">Elements to compute</param>
        /// <param name="signal">Signal to compute (must be power two)</param>
        /* static void Haar_forward_pass_2d(int n, ref double[][] signal)
         {
             int i, j;
             int npts;
            
             npts = (int) Utilities.NextPowerOfTwo( (uint) n);

             for (i = 0; i < npts; i++)
             {
                 Wavelet.Haar_forward_pass_1d(n, ref signal[i]);
             }

             double[] c = new double [npts];

             for (j = 0; j < npts; j++)
             {
                 for (i = 0; i < npts; i++)
                     c[i] = signal[i][j];
                 Wavelet.Haar_forward_pass_1d(n, ref c);
                 for (i = 0; i < npts; i++)
                     signal[i][j] = c[i];
             }
         }*/

        /// <summary>
        /// Calculate the Haar forward fast wavelet transform in 2-dimensions.
        /// </summary>
        /// <param name="n">Elements to compute</param>
        /// <param name="signal">Signal to compute (must be power two)</param>
        public static void Haar_forward_FWT_2d(int width, int height, ref double[] signal)
        {
            /*for (int m = n - 1; m >= 0; m--)
            {
                Haar_forward_pass_2d(m + 1, ref signal);
            }*/

            double[] row = new double[Utilities.NextPowerOfTwo((uint)width)];
            double[] column = new double[Utilities.NextPowerOfTwo((uint)height)];

            for (UInt32 y = 0; y < height; y++)
            {
                Array.Copy(signal, (int)(y * width), row, 0, (int)width);
                DSP.Wavelet.Haar_forward_FWT_1d(ref row);
                Array.Copy(row, 0, signal, (int)(y * width), (int)width);
            }

            for (int x = 0; x < width; x++)
            {

                for (int y = 0; y < height; y++)
                {
                    column[y] = signal[x + (y * width)];
                }

                DSP.Wavelet.Haar_forward_FWT_1d(ref column);

                for (int y = 0; y < height; y++)
                {
                    signal[x + (y * width)] = column[y];
                }

            }

        }

        /// <summary>
        /// Calculate one iteration of the Haar inverse FWT in 2-dimensions.
        /// </summary>
        /// <param name="signal">Signal to compute (must be power two)</param>
        /* static void Haar_inverse_pass_2d(int n, ref double[][] signal)
         {
             int i, j;
             int npts;
            
             npts = (int) Utilities.NextPowerOfTwo((uint) n);

             for (i = 0; i < npts; i++)
             {
                 Wavelet.Haar_inverse_pass_1d(n, ref signal[i]);
             }

             double[] c = new double[npts];

             for (j = 0; j < npts; j++)
             {
                 for (i = 0; i < npts; i++)
                     c[i] = signal[i][j];

                 Wavelet.Haar_inverse_pass_1d(n, ref c);

                 for (i = 0; i < npts; i++)
                     signal[i][j] = c[i];
             }         
         }*/


        /// <summary>
        /// Calculate the Haar inverse fast wavelet transform in 2-dimensions.
        /// </summary>
        /// <param name="signal">Signal to compute (must be power two)</param>
        public static void Haar_inverse_FWT_2d(int width, int height, ref double[] signal)
        {
            /*for (int m = 1; m <= signal.Length; m++)
            {
                Haar_inverse_pass_2d(m, ref signal);
            }*/

            double[] row = new double[Utilities.NextPowerOfTwo((uint)width)];
            double[] column = new double[Utilities.NextPowerOfTwo((uint)height)];

            for (int x = 0; x < width; x++)
            {

                for (int y = 0; y < height; y++)
                {
                    column[y] = signal[x + (y * width)];
                }

                DSP.Wavelet.Haar_inverse_FWT_1d(ref column);

                for (int y = 0; y < height; y++)
                {
                    signal[x + (y * width)] = column[y];
                }

            }

            for (UInt32 y = 0; y < height; y++)
            {
                Array.Copy(signal, (int)(y * width), row, 0, (int)width);
                DSP.Wavelet.Haar_inverse_FWT_1d(ref row);
                Array.Copy(row, 0, signal, (int)(y * width), (int)width);
            }
        }

        // The 1D Haar Transform 
        public static void haar1d(ref double[] vec, int n)
        {
            int i = 0;
            int w = n;
            double[] vecp = new double[n];
            for (i = 0; i < n; i++)
                vecp[i] = 0;

            while (w > 1)
            {
                w /= 2;
                for (i = 0; i < w; i++)
                {
                    vecp[i] = (vec[2 * i] + vec[2 * i + 1]) / Math.Sqrt(2.0);
                    vecp[i + w] = (vec[2 * i] - vec[2 * i + 1]) / Math.Sqrt(2.0);
                }

                for (i = 0; i < (w * 2); i++)
                    vec[i] = vecp[i];
            }

        }

        // A Modified version of 1D Haar Transform, used by the 2D Haar Transform function
        static void haar1(ref double[] vec, int n, int w)
        {
            int i = 0;
            double[] vecp = new double[n];
            for (i = 0; i < n; i++)
                vecp[i] = 0;

            w /= 2;
            for (i = 0; i < w; i++)
            {
                vecp[i] = (vec[2 * i] + vec[2 * i + 1]) / Math.Sqrt(2.0);
                vecp[i + w] = (vec[2 * i] - vec[2 * i + 1]) / Math.Sqrt(2.0);
            }

            for (i = 0; i < (w * 2); i++)
                vec[i] = vecp[i];

        }

        // The 2D Haar Transform 
        public static void haar2(ref double[] matrix, int rows, int cols)
        {
            double[] temp_row = new double[cols];
            double[] temp_col = new double[rows];

            int i = 0, j = 0;
            int w = cols, h = rows;
            while (w > 1 || h > 1)
            {
                if (w > 1)
                {
                    for (i = 0; i < h; i++)
                    {
                        for (j = 0; j < cols; j++)
                            temp_row[j] = matrix[i + (j * cols)];

                        haar1(ref temp_row, cols, w);

                        for (j = 0; j < cols; j++)
                            matrix[i + (j * cols)] = temp_row[j];
                    }
                }

                if (h > 1)
                {
                    for (i = 0; i < w; i++)
                    {
                        for (j = 0; j < rows; j++)
                            temp_col[j] = matrix[j + (i * cols)];
                        haar1(ref temp_col, rows, h);
                        for (j = 0; j < rows; j++)
                            matrix[j + (i * cols)] = temp_col[j];
                    }
                }

                if (w > 1)
                    w /= 2;
                if (h > 1)
                    h /= 2;
            }

        }
    }

    public class Utilities
    {
        public const Double DDC_PI = 3.14159265358979323846;

        public static double MSE(ref double[] signal_1, ref double[] signal_2, int SizeToCompare)
        {
            double result = 0;

            if (signal_1.Length < SizeToCompare) return -1;
            if (signal_2.Length < SizeToCompare) return -1;

            for (int i = 0; i < signal_1.Length; i++)
            {
                result += Math.Pow(signal_1[i] - signal_2[i], 2);
            }

            return (result / signal_1.Length);
        }

        /// <summary>
        /// Verifies a number is a power of two
        /// </summary>
        /// <param name="x">Number to check</param>
        /// <returns>true if number is a power two (i.e.:1,2,4,8,16,...)</returns>
        public static Boolean IsPowerOfTwo(UInt32 x)
        {
            return ((x != 0) && (x & (x - 1)) == 0);
        }

        /// <summary>
        /// Get Next power of number.
        /// </summary>
        /// <param name="x">Number to check</param>
        /// <returns>A power of two number</returns>
        public static UInt32 NextPowerOfTwo(UInt32 x)
        {
            x = x - 1;
            x = x | (x >> 1);
            x = x | (x >> 2);
            x = x | (x >> 4);
            x = x | (x >> 8);
            x = x | (x >> 16);
            return x + 1;
        }

        public static Double[] triangularExtraction(ref  Double[] value, uint width, uint height, uint num, int fill = -1)
        {
            Double[] result = new Double[num * 2];
            uint sidew = (uint)Math.Round(Math.Sqrt(num * 2));
            if (sidew > height) sidew = height;
            uint sideh = sidew;
            int index = 0;
            string _match = "";
            for (int y = 0; y < sideh; y++)
            {
                for (int x = 0; x < sidew; x++)
                {
                    result[index] = value[x + (y * width)];
                    _match += "[" + Math.Round(result[index]) + "],";
                    if (fill != -1)
                    {
                        value[x + (y * width)] = fill;
                        value[width - 1 - x + (y * width)] = fill;
                        value[x + ((height - y - 1) * width)] = fill;
                        value[width - 1 - x + ((height - y - 1) * width)] = fill;
                    }
                    index++;
                    //if (index >= num) break; 
                }
                System.Diagnostics.Debug.WriteLine(_match); _match = "";
                --sidew;
            }
            return result;
        }


        //static public void saveSignal(ref double[] signal, string fileName)
        //{
        //    IsolatedStorageFileStream fileStream = null;
        //    IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();

        //    //create new file
        //    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
        //    {
        //        fileStream = new IsolatedStorageFileStream(fileName, FileMode.Create, FileAccess.ReadWrite, myIsolatedStorage);

        //        using (StreamWriter writeFile = new StreamWriter(fileStream))
        //        {
        //            string _signal = "";
        //            for (int i = 0; i < signal.Length; i++)
        //            {
        //                _signal += Convert.ToString(signal[i]);
        //                if (i < signal.Length - 1) _signal += ";";
        //            }
        //            writeFile.WriteLine(_signal);
        //            writeFile.Close();
        //        }
        //    }
        //}

        //    static public double[] loadSignal(string fileName)
        //    {
        //        double[] signal;
        //        IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();

        //        if (!myIsolatedStorage.FileExists(fileName)) return null;

        //        IsolatedStorageFileStream fileStream = null;

        //        using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
        //        {
        //            fileStream = new IsolatedStorageFileStream(fileName, FileMode.Open, FileAccess.Read, myIsolatedStorage);

        //            using (StreamReader readFile = new StreamReader(fileStream))
        //            {
        //                string _signal = readFile.ReadLine();
        //                string[] __signal = _signal.Split(';');
        //                signal = new double[__signal.Length];

        //                for (int i = 0; i < __signal.Length; i++)
        //                {
        //                    signal[i] = Convert.ToDouble(__signal[i]);                     
        //                }

        //                readFile.Close();
        //            }
        //        }

        //        return signal;
        //    }

        //}

        public class Filters
        {
            public static double Triangular(double value, double samples)
            {
                return (2 / (samples + 1)) * (((samples + 1) / 2) - Math.Abs(value - ((samples - 1) / 2)));
            }

            public static double[] EnhanceHighFrequencies(ref double[] signal)
            {
                double[] result = new double[signal.Length];

                for (int i = 1; i < signal.Length; i++)
                {
                    result[i] = signal[i] - 0.95 * signal[i - 1];
                }

                return result;
            }
        }
    }
}

