// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using LimeVideoSDK.QuickSyncTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


namespace LimeVideoSDK.CPUConvertResize
{
    using QuickSync;
    using NV12Convert = NV12UnsafeNativeMethods;

    /// <summary>
    /// NativeXY is a simple X,Y point struct for interop with C++ NV12 convert/resize funcs
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeXY
    {
        public int x;
        public int y;
        public NativeXY(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    };


    /// <summary>
    /// NativeWidthHeight is a simple X,Y point struct for interop with C++ NV12 convert/resize funcs
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeWidthHeight
    {
        public int width;
        public int height;
        public NativeWidthHeight(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    };

    public enum NativeResizeConvertStatus
    {
        NativeStatusNoErr = 0,
    };

    /// <summary>
    /// For converting non NV12 raw frames to NV12
    /// See members for info on supported Fourccs
    /// </summary>
    public unsafe class NV12FromXXXXLowLevelConverter
    {
        NativeWidthHeight roisizea = new NativeWidthHeight();
        byte*[] pointers = new byte*[3];
        int[] intarr = new int[3];

        /// <summary>
        /// <para>Convert a raw frame to NV12.</para>
        /// <para>Used to convert fourcc formats to NV12.</para>
        /// 
        /// </summary>
        /// <param name="srcFourcc">One of NV12 RGB3 RGB4 UYVY YUY2 YV12 BGR3 BGR4 P411 P422 I420_IYUV</param>
        /// <param name="nv12pitch">output pitch in bytes</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="inFrameptr"></param>
        /// <param name="nv12">Y output plane destination</param>
        /// <param name="nv12uv">UV output plane destination</param>
        public unsafe void ConvertToNV12FrameSurface(FourCC srcFourcc, int nv12pitch, int width, int height, byte* inFrameptr, byte* nv12, byte* nv12uv)
        {
            NativeResizeConvertStatus sts = NativeResizeConvertStatus.NativeStatusNoErr;
            roisizea.height = height;
            roisizea.width = width;
            switch (srcFourcc)
            {
                //case FourCC.A2RGB10:
                //    break;
                //case FourCC.ARGB16:
                //    break;

                case FourCC.NV12:
                    //memmove(nv12, inFrameptr, height * width);
                    //memmove(nv12uv, inFrameptr + height * width, height * width / 2);

                    for (int j = 0; j < height; j++)
                        // Marshal.Copy(uncompressed, i * framelen + j * w, e.Frames[ix].Data.Y + e.Frames[ix].Data.Pitch * j, w);
                        FastMemcpyMemmove.memcpy(nv12 + nv12pitch * j, inFrameptr + width * j, width);
                    for (int j = 0; j < height / 2; j++)
                        FastMemcpyMemmove.memcpy(nv12uv + nv12pitch * j, inFrameptr + width * height + width * j, width);
                    //  Marshal.Copy(uncompressed, i * framelen + j * w + h * w, e.Frames[ix].Data.UV + e.Frames[ix].Data.Pitch * j, w);


                    break;
                //case FourCC.NV16:
                //    break;
                //case FourCC.P010:
                //    break;
                //case FourCC.P210:
                //    break;
                //case FourCC.P8:
                //    break;
                //case FourCC.P8_TEXTURE:
                //    break;
                //case FourCC.R16:
                //    break;





                case FourCC.BGR3:
                    sts = NV12Convert.BGR3ToNV12(inFrameptr, width * 3, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);
                    break;
                case FourCC.BGR4:  // this was not supported originally due to lack of support in intel IPP

                    sts = NV12Convert.BGR4ToNV12(inFrameptr, width * 4, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);
                    break;

                case FourCC.IYUV:
                    pointers[0] = inFrameptr;
                    pointers[1] = inFrameptr + height * width;
                    pointers[2] = inFrameptr + height * width + height * width / 2 / 2;
                    intarr[0] = width;
                    intarr[1] = width / 2;
                    intarr[2] = width / 2;
                    fixed (byte** p1 = pointers)
                    fixed (int* p2 = intarr)
                    {
                        sts = NV12Convert.IYUVToNV12(p1, p2, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);
                    }
                    break;

                case FourCC.P411:

                    pointers[0] = inFrameptr;
                    pointers[1] = inFrameptr + height * width;
                    pointers[2] = inFrameptr + height * width + height * width / 4;
                    intarr[0] = width;
                    intarr[1] = width / 4;
                    intarr[2] = width / 4;
                    fixed (byte** p1 = pointers)
                    fixed (int* p2 = intarr)
                    {
                        sts = NV12Convert.P411ToNV12(p1, p2, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);
                    }
                    break;

                case FourCC.P422:
                    pointers[0] = inFrameptr;
                    pointers[1] = inFrameptr + height * width;
                    pointers[2] = inFrameptr + height * width + height * width / 2;
                    intarr[0] = width;
                    intarr[1] = width / 2;
                    intarr[2] = width / 2;
                    fixed (byte** p1 = pointers)
                    fixed (int* p2 = intarr)
                    {
                        sts = NV12Convert.P422ToNV12(p1, p2, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);
                    }
                    break;

                case FourCC.RGB3:
                    sts = NV12Convert.RGB3ToNV12(inFrameptr, width * 3, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);
                    break;
                case FourCC.RGB4:
                    sts = NV12Convert.RGB4ToNV12(inFrameptr, width * 4, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);
                    break;


                case FourCC.YUY2:
                    sts = NV12Convert.YUY2ToNV12(inFrameptr, width * 2, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);

                    break;
                case FourCC.YV12:
                    //YVU 
                    // YCbCr == YUV
                    pointers[0] = inFrameptr;
                    pointers[1] = inFrameptr + height * width;
                    pointers[2] = inFrameptr + height * width + height * width / 2 / 2;
                    intarr[0] = width;
                    intarr[1] = width / 2;
                    intarr[2] = width / 2;
                    fixed (byte** p1 = pointers)
                    fixed (int* p2 = intarr)
                        sts = NV12Convert.YV12ToNV12(p1, p2, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);

                    break;
                case FourCC.UYVY:

                    // sts = NV12Convert.ippiCbYCr422ToYCbCr420_8u_C2P2R(inFrameptr, width * 2, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);
                    sts = NV12Convert.UYVYToNV12(inFrameptr, width * 2, nv12, nv12pitch, nv12uv, nv12pitch, roisizea);
                    break;
                default:
                    break;
            }
            if (sts != NativeResizeConvertStatus.NativeStatusNoErr)
                throw new Exception("FourCC convert error:" + sts.ToString());
        }
    }



    /// <summary>
    /// Convert NV12 frames to other FourCC formats
    /// </summary>
    public unsafe class NV12ToXXXXLowLevelConverter
    {
        byte*[] pdst = new byte*[3];
        int[] dststep = new int[3];
        NativeWidthHeight roisize = new NativeWidthHeight();


        /// <summary>Converts an NV12 mfxFrameSurface1 to another FourCC with IntPtr destination.</summary>
        /// <param name="frame">Inout frame.</param>
        /// <param name="outputFourCC">The output FourCC.</param>
        /// <param name="outbuf">The outbuf destination.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="outputPitch">The output pitch.</param>
        /// <param name="outputlen">The outputlen.</param>
        public unsafe void ConvertFromNV12FrameSurface1(mfxFrameSurface1 frame, FourCC outputFourCC, IntPtr outbuf, int width, int height, int outputPitch, int outputlen)
        {
            Trace.Assert(height * outputPitch == outputlen);


            byte* nv12 = (byte*)frame.Data.Y;
            byte* nv12uv = (byte*)frame.Data.UV;



            int pitch = frame.Data.PitchHigh << 16 | frame.Data.PitchLow;

            this.ConvertFromNV12(outputFourCC, nv12, nv12uv, (byte*)outbuf, width, height, outputlen, width, outputPitch);


            return;
        }

        /// <summary>
        /// Convert an NV12 frame to another fourcc format
        /// </summary>
        /// <param name="destFourcc">One of NV12 RGB3 RGB4 UYVY YUY2 YV12 BGR3 P411 P422 I420_IYUV</param>
        /// <param name="Y">Y plane of NV12 input</param>
        /// <param name="UV">UV plane of NV12 input</param>
        /// <param name="outputFramePtr">where output frame goes</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="outputLen"></param>
        /// <param name="srcPitch">Pitch of the source NV12</param>
        /// <param name="dstPitch">Pitch of the dest frame</param>
        /// <param name="alpha">Only used for formats with sn alpha plane: RGB4, ...</param>
        public unsafe void ConvertFromNV12(FourCC destFourcc, byte* Y, byte* UV, byte* outputFramePtr, int width, int height, int outputLen, int srcPitch = 0, int dstPitch = 0, byte alpha = 255)
        {


            int bitsPerPixel = VideoUtility.GetBitsPerPixel(destFourcc);
            // int dstpitch = bitsPerPixel * width / 8;
            int compactFrameSize = width * height * bitsPerPixel / 8;
            Trace.Assert(outputLen == compactFrameSize);



            //byte* nv12uv = nv12 + width * height;


            roisize.height = height;
            roisize.width = width;

            NativeResizeConvertStatus sts = NativeResizeConvertStatus.NativeStatusNoErr;



            if (srcPitch == 0)
                srcPitch = width * 1;

            if (dstPitch == 0)
                dstPitch = width * VideoUtility.GetPackedPitchMultiplier(destFourcc);


            switch (destFourcc)
            {
                case FourCC.BGR3:
                    sts = NV12Convert.NV12ToBGR3(Y, srcPitch, UV, srcPitch, outputFramePtr, dstPitch, roisize);
                    break;
                case FourCC.BGR4:
                    sts = NV12Convert.NV12ToBGR4(Y, srcPitch, UV, srcPitch, outputFramePtr, dstPitch, roisize, alpha);
                    break;


                case FourCC.RGB4:
                    sts = NV12Convert.NV12ToRGB4(Y, srcPitch, UV, srcPitch, outputFramePtr, dstPitch, roisize, alpha);
                    break;
                case FourCC.RGB3:
                    sts = NV12Convert.NV12ToRGB3(Y, srcPitch, UV, srcPitch, outputFramePtr, dstPitch, roisize);
                    break;




                case FourCC.YUY2:
                    sts = NV12Convert.NV12ToYUY2(Y, srcPitch, UV, srcPitch, outputFramePtr, dstPitch, roisize);

                    break;
                case FourCC.YV12:
                    pdst[0] = outputFramePtr;
                    // This seems wrong, backwards, maybe a bug in IPP 7.x
                    // indicies should be 1,2 not 2,1
                    //manual check indicates this is what works correctly 2/22/15
                    pdst[2] = outputFramePtr + height * width;
                    pdst[1] = outputFramePtr + height * width + height * width / 2 / 2;
                    dststep[0] = dstPitch;
                    dststep[1] = dstPitch / 2;
                    dststep[2] = dstPitch / 2;
                    fixed (byte** p1 = pdst)
                    fixed (int* p2 = dststep)
                        sts = NV12Convert.NV12ToYV12(Y, srcPitch, UV, srcPitch, p1, p2, roisize);
                    break;
                case FourCC.UYVY:
                    sts = NV12Convert.NV12ToUYVY(Y, srcPitch, UV, srcPitch, outputFramePtr, dstPitch, roisize);
                    break;
                case FourCC.I420_IYUV:
                    pdst[0] = outputFramePtr;
                    pdst[1] = outputFramePtr + height * width;
                    pdst[2] = outputFramePtr + height * width + height * width / 2 / 2;
                    dststep[0] = dstPitch;
                    dststep[1] = dstPitch / 2;
                    dststep[2] = dstPitch / 2;
                    fixed (byte** p1 = pdst)
                    fixed (int* p2 = dststep)
                        sts = NV12Convert.NV12ToYV12(Y, srcPitch, UV, srcPitch, p1, p2, roisize);
                    break;
                case FourCC.P411:

                    pdst[0] = outputFramePtr;
                    pdst[1] = outputFramePtr + height * width;
                    pdst[2] = outputFramePtr + height * width + height * width / 4;
                    dststep[0] = dstPitch;
                    dststep[1] = dstPitch / 4;
                    dststep[2] = dstPitch / 4;
                    fixed (byte** p1 = pdst)
                    fixed (int* p2 = dststep)
                        sts = NV12Convert.NV12ToP411(Y, srcPitch, UV, srcPitch, p1, p2, roisize);
                    break;
                case FourCC.P422:
                    pdst[0] = outputFramePtr;
                    pdst[1] = outputFramePtr + height * width;
                    pdst[2] = outputFramePtr + height * width + height * width / 2;
                    dststep[0] = dstPitch;
                    dststep[1] = dstPitch / 2;
                    dststep[2] = dstPitch / 2;
                    fixed (byte** p1 = pdst)
                    fixed (int* p2 = dststep)
                        sts = NV12Convert.NV12ToP422(Y, srcPitch, UV, srcPitch, p1, p2, roisize);
                    break;
                case FourCC.NV12:

                    // XXX nonono
                    // does not work for pitch!=width  !!
                    //memmove(outputFramePtr, Y, height * width);
                    //memmove(outputFramePtr + height * width, UV, height * width / 2);
                    for (int i = 0; i < height; i++)
                        FastMemcpyMemmove.memcpy(outputFramePtr + i * dstPitch, Y + i * srcPitch, width);
                    outputFramePtr += height * dstPitch;
                    for (int i = 0; i < height / 2; i++)
                        FastMemcpyMemmove.memcpy(outputFramePtr + i * dstPitch, UV + i * srcPitch, width);
                    break;
                default:
                    throw new Exception("fourcc conversion not supported, contact Lime Video  support for options");
            }

            if (sts != NativeResizeConvertStatus.NativeStatusNoErr)
                throw new Exception("FourCC convert error:" + sts.ToString());
        }
    }
}
