// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LimeVideoSDKQuickSync
{
    static public class ExtensionMethods
    {

      
      unsafe  static public Bitmap GetBitmap(this ILowLevelDecoder decoder, mfxFrameSurface1 surface)

        {
#if false
            //   if ( this.Data.MemId == IntPtr.Zero)
            {
                Trace.Assert(this.Info.FourCC == FourCC.NV12, "AsBitmap only works on NV12 currently");

                Bitmap bmp = new Bitmap(this.Info.CropW, this.Info.CropH, PixelFormat.Format32bppArgb);

                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

                //  var a = new FrameFormatConverterFromNV12(FourCC.BGR4,bmp.Width, bmp.Height);
                var a = new NV12ToXXXXLowLevelConverter();

                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                a.ConvertFromNV12FrameSurface(FourCC.BGR4, this.Data.Y_ptr, this.Data.UV_ptr, (byte*)bmpData.Scan0, bmp.Width, bmp.Height,
                    bmpData.Stride * bmpData.Height, this.Info.CropW, bmpData.Stride);

                return bmp;
#else
            Trace.Assert(surface.Info.FourCC == FourCC.RGB4, "For vidmem, AsBitmap only works on RGB4 currently");

            Bitmap bmp = new Bitmap(surface.Info.CropW, surface.Info.CropH, PixelFormat.Format32bppArgb);

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);


            BitmapData bmpData;

            bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            mfxFrameData fd = new mfxFrameData();

          
            decoder.LockFrame((IntPtr)(&surface));

            Console.WriteLine(fd.B);
            int pitch = fd.PitchHigh << 16 | fd.PitchLow;


            var zm = new byte[pitch * surface.Info.CropH];
            //for (int i = 0; i < zm.Length; i++)
            //{
            //    zm[i] = 1;
            //}
            //  Marshal.Copy(fd.B, zm, 0, zm.Length);
            //  Console.WriteLine(zm.Sum(z => (long)z));


            if (pitch == bmpData.Stride)
            {
                FastMemcpyMemmove.memcpy(ptr, fd.B, pitch * surface.Info.CropH);
            }
            else
            {
                int minpitch = Math.Min(pitch, bmpData.Stride);
                for (int i = 0; i < surface.Info.CropH; i++)
                {
                    FastMemcpyMemmove.memcpy(ptr + bmpData.Stride * i, fd.B + pitch * i, minpitch);
                }
            }
            decoder.UnlockFrame((IntPtr)(&surface));
            bmp.UnlockBits(bmpData);
            return bmp;

        }
#endif
    }
}
