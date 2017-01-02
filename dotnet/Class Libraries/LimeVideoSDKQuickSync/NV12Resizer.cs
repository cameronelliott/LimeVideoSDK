// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using LimeVideoSDK.QuickSync;
using LimeVideoSDK.QuickSyncTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;



namespace LimeVideoSDK.CPUConvertResize
{

    /// <summary>
    /// For converting non-NV12 raw frames to NV12
    /// For example to convert RGB3 or I420 to NV12
    /// See methods for fourcc formats supported
    /// </summary>
    public unsafe class NV12FromXXXXConverter
    {
        /// <summary>
        /// Where output nv12 frames can be found
        /// </summary>
        byte[] nv12buf;
        int w, h;
        FourCC infourcc;
        NV12FromXXXXLowLevelConverter llconverter;

        /// <summary>
        /// Takes 
        /// </summary>
        /// <param name="nv12surf"></param>
        /// <param name="srcframe"></param>
        /// <param name="offset"></param>
        public unsafe void ConvertToNV12FrameSurface(ref mfxFrameSurface1 nv12surf, byte[] srcframe, int offset)
        {

            int compactFrameSize = w * h * VideoUtility.GetBitsPerPixel(infourcc) / 8;
            //Trace.Assert(srcframe.Length == compactFrameSize);

            // expect by caller
            //encoder.LockFrame(f);
            fixed (byte* frameptr = &srcframe[offset])
            {
                byte* nv12 = (byte*)nv12surf.Data.Y;
                byte* nv12uv = (byte*)nv12surf.Data.UV;

                llconverter.ConvertToNV12FrameSurface(infourcc, nv12surf.Data.Pitch, w, h, frameptr, nv12, nv12uv);

            }
            //encoder.UnlockFrame(f);
        }

        /// <summary>Converts to n V12 frame surface.</summary>
        /// <param name="nv12surfx">The nv12surfx.</param>
        /// <param name="srcframe">The srcframe.</param>
        /// <param name="offset">The offset.</param>
        public unsafe void ConvertToNV12FrameSurface(ref IntPtr nv12surfx, byte[] srcframe, int offset)
        {

            int compactFrameSize = w * h * VideoUtility.GetBitsPerPixel(infourcc) / 8;
            //Trace.Assert(srcframe.Length == compactFrameSize);

         
            var nv12surf = (mfxFrameSurface1*)nv12surfx;
            // expect by caller
            //encoder.LockFrame(f);
            fixed (byte* frameptr = &srcframe[offset])
            {
                byte* nv12 = (byte*)nv12surf->Data.Y;
                byte* nv12uv = (byte*)nv12surf->Data.UV;

                llconverter.ConvertToNV12FrameSurface(infourcc, nv12surf->Data.Pitch, w, h, frameptr, nv12, nv12uv);

            }
            //encoder.UnlockFrame(f);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="infourcc"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public NV12FromXXXXConverter(FourCC infourcc, int width, int height)
        {
            this.h = height;
            this.w = width;
            this.infourcc = infourcc;
            nv12buf = new byte[height * width * VideoUtility.GetBitsPerPixel(FourCC.NV12) / 8];
            llconverter = new NV12FromXXXXLowLevelConverter();
        }


        /// <summary>
        /// Convert a non-NV12 raw frame to NV12. for RGB3, I420, etc...
        /// </summary>
        /// <param name="inbuf">Raw frame</param>
        /// <param name="allocateNewArray">True = allocate a new array on each call</param>
        public unsafe byte[] ConvertToNV12(byte[] inbuf, bool allocateNewArray = true)
        {

            int inbuflen = h * w * VideoUtility.GetBitsPerPixel(infourcc) / 8;
            Trace.Assert(inbuf.Length == inbuflen);

            byte[] buf = nv12buf;

            if (allocateNewArray)
                buf = new byte[nv12buf.Length];


            fixed (byte* nv12 = buf)
            fixed (byte* srcframe = inbuf)
                llconverter.ConvertToNV12FrameSurface(infourcc, w, w, h, srcframe, nv12, nv12 + w * h);

            return buf;
        }
    }


    /// <summary>
    /// Supports convertering uncompressed NV12 frames to other formats
    /// </summary>
    public unsafe class NV12ToXXXXConverter
    {
        int w, h;
        FourCC outfourcc;
        byte[] outbuf = null;
        NV12ToXXXXLowLevelConverter llconverter;

        /// <summary>Converts a frame to a byte[] in the specified FourCC</summary>
        /// <param name="frameData">The frame data.</param>
        /// <param name="reuseOutputBuffer">if set to <c>true</c> [reuse output buffer].</param>
        /// <returns>The frame data in packed format</returns>
        public unsafe byte[] ConvertFromNV12(mfxFrameData frameData, bool reuseOutputBuffer = false)
        {
            if (!reuseOutputBuffer)
                outbuf = new byte[outbuf.Length];

            byte* nv12 = (byte*)frameData.Y;
            byte* nv12uv = (byte*)frameData.UV;

            int pitch = frameData.PitchHigh << 16 | frameData.PitchLow;

            // lock(decoder,frame);                // now callers responsbility
            fixed (byte* dstframe = outbuf)
                llconverter.ConvertFromNV12(outfourcc, nv12, nv12uv, dstframe, w, h, outbuf.Length, pitch);
            // unlock(encoder,frame); 

            return outbuf;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="outfourcc">>One of NV12 RGB3 RGB4 UYVY YUY2 YV12 BGR3 P411 P422 I420_IYUV</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public NV12ToXXXXConverter(FourCC outfourcc, int width, int height)
        {
            this.outfourcc = outfourcc;
            this.h = height;
            this.w = width;

            outbuf = new byte[w * h * VideoUtility.GetBitsPerPixel(outfourcc) / 8];
            llconverter = new NV12ToXXXXLowLevelConverter();
        }

        /// <summary>
        /// Convert frame from NV12 to another format such as RGB4, I420, etc...
        /// </summary>
        /// <param name="nv12buf"></param>
        /// <param name="reuseOutputBuffer">False = allocate new memory on each call</param>
        public byte[] ConvertFromNV12(byte[] nv12buf, bool reuseOutputBuffer = false)
        {
            if (!reuseOutputBuffer)
                outbuf = new byte[outbuf.Length];

            fixed (byte* nv12 = nv12buf)
            fixed (byte* dstframe = outbuf)
                llconverter.ConvertFromNV12(outfourcc, nv12, nv12 + w * h, dstframe, w, h, outbuf.Length);

            return outbuf;
        }
    }



    /// <summary>
    /// This class is for resizing NV12 frames
    /// </summary>
    public class NV12Resizer
    {

        /// <summary>
        /// Where you get the resized output frame from
        /// </summary>
        public byte[] outbuf;
        NativeWidthHeight src;
        NativeWidthHeight dst;
        NativeXY dstOffset = new NativeXY(0, 0);
        byte[] pSpec;
        byte[] pInitBuf;
        byte[] pBuf;
        int inwidth, inheight, outwidth, outheight;

        List<GCHandle> gcHandles = new List<GCHandle>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inwidth"></param>
        /// <param name="inheight"></param>
        /// <param name="outwidth"></param>
        /// <param name="outheight"></param>
        unsafe public NV12Resizer(int inwidth, int inheight, int outwidth, int outheight)
        {

            this.inwidth = inwidth;
            this.inheight = inheight;
            this.outwidth = outwidth;
            this.outheight = outheight;
            outbuf = new byte[(outwidth * outheight * 3 / 2) + 1];
            outbuf[outbuf.Length - 1] = 0xbb;
            Trace.Assert(outbuf[outbuf.Length - 1] == 0xbb);



            src = new NativeWidthHeight(inwidth, inheight);
            dst = new NativeWidthHeight(outwidth, outheight);


            int pSpecSize;
            int pInitBufSize;
            int pBufSize;
            NativeResizeConvertStatus sts;
            sts = NV12UnsafeNativeMethods.ResizeYUV420GetSize(src, dst, 16, 0, &pSpecSize, &pInitBufSize);
            Trace.Assert(sts == NativeResizeConvertStatus.NativeStatusNoErr);

            pSpec = new byte[pSpecSize + 1];
            pInitBuf = new byte[pInitBufSize + 1];
            pSpec[pSpec.Length - 1] = 0xbb;
            pInitBuf[pInitBuf.Length - 1] = 0xbb;
            Trace.Assert(pSpec[pSpec.Length - 1] == 0xbb);
            Trace.Assert(pInitBuf[pInitBuf.Length - 1] == 0xbb);


            gcHandles.Add(GCHandle.Alloc(pSpec, GCHandleType.Pinned));
            gcHandles.Add(GCHandle.Alloc(pInitBuf, GCHandleType.Pinned));

            //pSpec[0] = 0xbb;
            //pSpec[pSpec.Length - 1] = 0xbb;
            //pInitBuf[0] = 0xbb;
            //pInitBuf[pInitBuf.Length - 1] = 0xbb;

            fixed (byte* spec = &pSpec[0])
            fixed (byte* tmpbuf = &pInitBuf[0])
            {


                sts = NV12UnsafeNativeMethods.ResizeYUV420LanczosInit(src, dst, 2, spec, tmpbuf);
                Trace.Assert(sts == NativeResizeConvertStatus.NativeStatusNoErr);

                sts = NV12UnsafeNativeMethods.ResizeYUV420GetBufferSize(spec, dst, &pBufSize);
                Trace.Assert(sts == NativeResizeConvertStatus.NativeStatusNoErr);

                pBuf = new byte[pBufSize + 1];
                gcHandles.Add(GCHandle.Alloc(pBuf, GCHandleType.Pinned));
                pBuf[pBuf.Length - 1] = 0xbb;
                Trace.Assert(pBuf[pBuf.Length - 1] == 0xbb);
            }
        }

        /// <summary>
        /// Convert a frame. Find the output in the member 'outputbuf'.
        /// </summary>
        /// <param name="nv12inbuf">Input frame. Must be width*height*3/2 bytes long.</param>
        unsafe public void Convert(byte[] nv12inbuf)
        {
            Trace.Assert(nv12inbuf.Length == inwidth * inheight * 3 / 2);
            NativeResizeConvertStatus sts;

            fixed (byte* spec = &pSpec[0])
            fixed (byte* pbufptr = &pBuf[0])
            fixed (byte* nv12in = nv12inbuf)
            fixed (byte* nv12out = &outbuf[0])
            {
                Trace.Assert(outbuf[outbuf.Length - 1] == 0xbb);
                Trace.Assert(pSpec[pSpec.Length - 1] == 0xbb);
                Trace.Assert(pInitBuf[pInitBuf.Length - 1] == 0xbb);
                Trace.Assert(pBuf[pBuf.Length - 1] == 0xbb);
                sts = NV12UnsafeNativeMethods.ResizeYUV420Lanczos_8u_P2R(nv12in, inwidth, nv12in + inwidth * inheight, inwidth, nv12out, outwidth, nv12out + outwidth * outheight, outwidth, dstOffset, dst, 1, null, spec, pbufptr);
                Trace.Assert(sts == NativeResizeConvertStatus.NativeStatusNoErr);
                Trace.Assert(pBuf[pBuf.Length - 1] == 0xbb);
                Trace.Assert(outbuf[outbuf.Length - 1] == 0xbb);
                Trace.Assert(pSpec[pSpec.Length - 1] == 0xbb);
                Trace.Assert(pInitBuf[pInitBuf.Length - 1] == 0xbb);
            }
        }


        /// <summary>
        /// Returns a sequence of resized NV12 frames
        /// </summary>
        /// <param name="frames">Input frames</param>
        /// <param name="inWidth"></param>
        /// <param name="inHeight"></param>
        /// <param name="outWidth"></param>
        /// <param name="outHeight"></param>
        /// <param name="reuseOutputBuffer">When true, you will get the same array on each return, when false you get a new array on each return</param>
        /// <returns>Sequence of resized frames</returns>
        public static IEnumerable<byte[]> ResizeNV12FrameSequence(IEnumerable<byte[]> frames, int inWidth, int inHeight, int outWidth, int outHeight, bool reuseOutputBuffer = true)
        {

            var c = new NV12Resizer(inWidth, inHeight, outWidth, outHeight);

            foreach (var i in frames)
            {
                c.Convert(i);

                if (reuseOutputBuffer)
                    yield return c.outbuf;
                else
                    yield return (byte[])c.outbuf.Clone();
            }
        }
    }
}

