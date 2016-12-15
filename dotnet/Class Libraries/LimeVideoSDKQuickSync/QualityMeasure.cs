// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LimeVideoSDKQuickSync
{
    /// <summary>
    /// Methods related to quality measurement and frame comparison
    /// </summary>
    public static class QualityMeasure
    {



        /// <summary>
        /// Compare two uncompressed NV12 disk files for sameness
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static bool NV12FilesSame(string path1, string path2)
        {
            return NV12RawFilesAreTheSame(File.OpenRead(path1), File.OpenRead(path2));
        }

        /// <summary>
        /// Are two NV12 byte arrays the same?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool NV12RawFramesAreTheSame(byte[] a, byte[] b)
        {
            var aa = new MemoryStream(a);
            var bb = new MemoryStream(b);
            return NV12RawFilesAreTheSame(aa, bb);
        }

        /// <summary>
        /// Check if two raw NV12 files are basically equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool NV12RawFilesAreTheSame(Stream a, Stream b)
        {

            var aa = StreamHelper.StreamAsIEnumerable(a);
            var bb = StreamHelper.StreamAsIEnumerable(b);

            return NV12RawFilesAreTheSame(aa, bb);
        }

        /// <summary>
        /// Compare two byte ienumerables for NV12 frames
        /// </summary>
        /// <param name="aa"></param>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static bool NV12RawFilesAreTheSame(IEnumerable<byte> aa, IEnumerable<byte> bb)
        {


            var c = aa.Zip(bb, (x, y) => Math.Abs(x - y));

            return c.All(z => z < 10);
        }

        public static bool PeakSignalNoiseRatioForNV12AreTheSame(byte[] nv12a, byte[] nv12b, int width, int height)
        {
            return PeakSignalNoiseRatioForNV12(nv12a, nv12b, width, height).All(z => z > 25.0);
        }

        /// <summary>
        /// Return the PNSR for two NV12 raw frames of equal size
        /// </summary>
        /// <param name="nv12a">Frame A</param>
        /// <param name="nv12b">Frame B</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static double[] PeakSignalNoiseRatioForNV12(byte[] nv12a, byte[] nv12b, int width, int height)
        {
            int len = height * width * VideoUtility.GetBitsPerPixel(FourCC.NV12) / 8;
            Trace.Assert(nv12a.Length == len);
            Trace.Assert(nv12b.Length == len);



            double mseY = 0;
            double mseU = 0;
            double mseV = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //mseY += Math.Pow(nv12a[x + y * width] - nv12b[x + y * width], 2);
                    var a = (int)(nv12a[x + y * width] - nv12b[x + y * width]);
                    mseY += a * a;
                }
            }
            for (int y = 0; y < height / 2; y++)
            {
                for (int x = 0; x < width / 2; x++)
                {
                    // mseU += Math.Pow(nv12a[x * 2 + 0 + y * width + height * width] - nv12b[x * 2 + 0 + y * width + height * width], 2);
                    //  mseV += Math.Pow(nv12a[x * 2 + 1 + y * width + height * width] - nv12b[x * 2 + 1 + y * width + height * width], 2);

                    var a = (int)(nv12a[x * 2 + 0 + y * width + height * width] - nv12b[x * 2 + 0 + y * width + height * width]);
                    mseU += a * a;
                    a = (int)(nv12a[x * 2 + 1 + y * width + height * width] - nv12b[x * 2 + 1 + y * width + height * width]);
                    mseV += a * a;
                }
            }

            //var diffs = new List<int>();
            //int max = 0;
            //for (int i = 0; i < len; i++)
            //{
            //diffs.Add(nv12a[i] - nv12b[i]);
            //max = Math.Max(max, Math.Abs(nv12a[i] - nv12b[i]));
            //}
            // expensive var foo = diffs.OrderByDescending(x => x).ToArray();

            var psnr = new double[3];
            psnr[0] = 20 * Math.Log10(255) - 10 * Math.Log10(mseY / (width * height));
            psnr[1] = 20 * Math.Log10(255) - 10 * Math.Log10(mseU / (width * height));
            psnr[2] = 20 * Math.Log10(255) - 10 * Math.Log10(mseV / (width * height));
            return psnr;
        }


    }

}
