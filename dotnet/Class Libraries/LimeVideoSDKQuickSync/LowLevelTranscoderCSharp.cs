// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using LimeVideoSDK.QuickSyncTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;



namespace LimeVideoSDK.QuickSync
{

    /// <summary>
    /// This is the low level Transcoder.
    /// The StreamTranscoder should almost always be used instead.
    /// </summary>
    /// <seealso cref="StreamTranscoder" />
    /// <seealso cref="System.IDisposable" />
    unsafe public class LowLevelTranscoderCSharp : IDisposable
    {
        /// <summary>The session</summary>
        public mfxSession session;
        ///// <summary>The device setup</summary>
        //public DeviceSetup deviceSetup;
        /// <summary>The warnings</summary>
        public Dictionary<string, mfxStatus> warnings = new Dictionary<string, mfxStatus>();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct TwoMfxFrameAllocRequest
        {
            public mfxFrameAllocRequest In;
            public mfxFrameAllocRequest Out;
        }

        struct Task
        {
            public mfxBitstream mfxBS;
            public mfxSyncPoint syncp;
        };






        int taskPoolSize;
        Task* pTasks;
        int nFirstSyncTask = 0;
        mfxFrameSurface1* pSurfaces;
        mfxFrameSurface1* pSurfaces2;
        int nSurfNumDecVPP;
        int nSurfNumVPPEnc;
        mfxBitstream* mfxBS;

        

        static IntPtr MyAllocHGlobalAndZero(int cb)
        {
            IntPtr p = Marshal.AllocHGlobal(cb);
            Trace.Assert(p != IntPtr.Zero);
            //RtlZeroMemory(p, cb);
            Marshal.Copy(new byte[cb], 0, p, cb);
            return p;
        }

        static int GetFreeTaskIndex(Task* pTaskPool, int n)
        {
            Trace.Assert(pTaskPool != null);

            for (int i = 0; i < n; i++)
                if (pTaskPool[i].syncp.sync == IntPtr.Zero)
                    return i;
            return (int)mfxStatus.MFX_ERR_NOT_FOUND;
        }

        static int GetFreeSurfaceIndex(mfxFrameSurface1* pSurfacesPool, int n)
        {

            for (int i = 0; i < n; i++)
                if (0 == pSurfacesPool[i].Data.Locked)
                    return i;
            return (int)mfxStatus.MFX_ERR_NOT_FOUND;
        }


        /// <summary>Initializes a new instance of the <see cref="LowLevelTranscoderCSharp"/> class.</summary>
        /// <param name="config">The configuration.</param>
        /// <param name="impl">The implementation.</param>
        /// <param name="forceSystemMemory">if set to <c>true</c> [force system memory].</param>
        public LowLevelTranscoderCSharp(TranscoderConfiguration config, mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO, bool forceSystemMemory = false)
        {
            mfxStatus sts;

            mfxVideoParam mfxDecParams = config.decParams;
            mfxVideoParam mfxVPPParams = config.vppParams;
            mfxVideoParam mfxEncParams = config.encParams;


            session = new mfxSession();
            var ver = new mfxVersion() { Major = 1, Minor = 3 };
            fixed (mfxSession* s = &session)
                sts = UnsafeNativeMethods.MFXInit(impl, &ver, s);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXInit");
            //deviceSetup = new DeviceSetup(session, forceSystemMemory);

          





            //  mfxVideoParam mfxDecParams = new mfxVideoParam();
            //  mfxDecParams.mfx.CodecId = CodecId.MFX_CODEC_AVC;




            int bufsize = (int)1e6;
            mfxBS = (mfxBitstream*)MyAllocHGlobalAndZero(sizeof(mfxBitstream));
            mfxBS->Data = MyAllocHGlobalAndZero(bufsize);
            mfxBS->DataLength = (uint)0;
            mfxBS->MaxLength = (uint)bufsize;
            mfxBS->DataOffset = 0;


            int outwidth = mfxDecParams.mfx.FrameInfo.CropW;
            int outheight = mfxDecParams.mfx.FrameInfo.CropH;



            // Query number of required surfaces for VPP
            //mfxFrameAllocRequest[] VPPRequest = new mfxFrameAllocRequest[2];     // [0] - in, [1] - out
            TwoMfxFrameAllocRequest VPPRequest;
            sts = UnsafeNativeMethods.MFXVideoVPP_QueryIOSurf(session, &mfxVPPParams, (mfxFrameAllocRequest*)&VPPRequest);
            if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION)
            {
                warnings.Add(nameof(UnsafeNativeMethods.MFXVideoVPP_QueryIOSurf), sts);
                sts = 0;
            }
            QuickSyncStatic.ThrowOnBadStatus(sts, "vpp.queryiosurf");



            // Query number required surfaces for dec
            mfxFrameAllocRequest DecRequest;
            sts = UnsafeNativeMethods.MFXVideoDECODE_QueryIOSurf(session, &mfxDecParams, &DecRequest);
            if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION)
            {
                warnings.Add(nameof(UnsafeNativeMethods.MFXVideoDECODE_QueryIOSurf), sts);
                sts = 0;
            }
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(UnsafeNativeMethods.MFXVideoDECODE_QueryIOSurf));


            // Query number of required surfaces for enc
            mfxFrameAllocRequest EncRequest = new mfxFrameAllocRequest();
            sts = UnsafeNativeMethods.MFXVideoENCODE_QueryIOSurf(session, &mfxEncParams, &EncRequest);
            if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION)
            {
                warnings.Add(nameof(UnsafeNativeMethods.MFXVideoENCODE_QueryIOSurf), sts);
                sts = 0;
            }
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(UnsafeNativeMethods.MFXVideoENCODE_QueryIOSurf));











            // Determine the required number of surfaces for decoder output (VPP input) and for VPP output (encoder input)
            nSurfNumDecVPP = DecRequest.NumFrameSuggested + VPPRequest.In.NumFrameSuggested + mfxVPPParams.AsyncDepth;
            nSurfNumVPPEnc = EncRequest.NumFrameSuggested + VPPRequest.Out.NumFrameSuggested + mfxVPPParams.AsyncDepth;






            {
                Trace.Assert((mfxEncParams.IOPattern & IOPattern.MFX_IOPATTERN_IN_SYSTEM_MEMORY) != 0);
                Trace.Assert((mfxDecParams.IOPattern & IOPattern.MFX_IOPATTERN_OUT_SYSTEM_MEMORY) != 0);

                UInt16 width = (UInt16)QuickSyncStatic.ALIGN32(DecRequest.Info.Width);
                UInt16 height = (UInt16)QuickSyncStatic.ALIGN32(DecRequest.Info.Height);
                int bitsPerPixel = 12;
                int surfaceSize = width * height * bitsPerPixel / 8;

                var decVppSurfaceBuffers = Marshal.AllocHGlobal(surfaceSize * nSurfNumDecVPP);
                var vppEncSurfaceBuffers = Marshal.AllocHGlobal(surfaceSize * nSurfNumVPPEnc);

                pSurfaces =
                    (mfxFrameSurface1*)MyAllocHGlobalAndZero(sizeof(mfxFrameSurface1) * nSurfNumDecVPP);

                pSurfaces2 =
                    (mfxFrameSurface1*)MyAllocHGlobalAndZero(sizeof(mfxFrameSurface1) * nSurfNumVPPEnc);

                for (int i = 0; i < nSurfNumDecVPP; i++)
                {
                    pSurfaces[i] = new mfxFrameSurface1();
                    pSurfaces[i].Info = DecRequest.Info;
                    pSurfaces[i].Data.Y_ptr = (byte*)decVppSurfaceBuffers + i * surfaceSize;
                    pSurfaces[i].Data.U_ptr = pSurfaces[i].Data.Y_ptr + width * height;
                    pSurfaces[i].Data.V_ptr = pSurfaces[i].Data.U_ptr + 1;
                    pSurfaces[i].Data.Pitch = width;
                }
                for (int i = 0; i < nSurfNumVPPEnc; i++)
                {
                    pSurfaces2[i] = new mfxFrameSurface1();
                    pSurfaces2[i].Info = EncRequest.Info;
                    pSurfaces2[i].Data.Y_ptr = (byte*)vppEncSurfaceBuffers + i * surfaceSize;
                    pSurfaces2[i].Data.U_ptr = pSurfaces2[i].Data.Y_ptr + width * height;
                    pSurfaces2[i].Data.V_ptr = pSurfaces2[i].Data.U_ptr + 1;
                    pSurfaces2[i].Data.Pitch = width;
                }
            }



            sts = UnsafeNativeMethods.MFXVideoDECODE_Init(session, &mfxDecParams);
            if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION)
            {
                warnings.Add(nameof(UnsafeNativeMethods.MFXVideoDECODE_Init), sts);
                sts = 0;
            }
            QuickSyncStatic.ThrowOnBadStatus(sts, "decode.init");

            sts = UnsafeNativeMethods.MFXVideoENCODE_Init(session, &mfxEncParams);
            if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION)
            {
                warnings.Add(nameof(UnsafeNativeMethods.MFXVideoENCODE_Init), sts);
                sts = 0;
            }
            QuickSyncStatic.ThrowOnBadStatus(sts, "encode.init");

            sts = UnsafeNativeMethods.MFXVideoVPP_Init(session, &mfxVPPParams);
            if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION)
            {
                warnings.Add(nameof(UnsafeNativeMethods.MFXVideoVPP_Init), sts);
                sts = 0;
            }
            QuickSyncStatic.ThrowOnBadStatus(sts, "vpp.init");



            //mfxExtVPPDoNotUse zz;
            //zz.Header.BufferId = BufferId.MFX_EXTBUFF_VPP_DONOTUSE;
            //zz.Header.BufferSz = (uint)sizeof(mfxExtVPPDoUse);
            //mfxExtBuffer** pExtParamsVPPx = stackalloc mfxExtBuffer*[1];
            //pExtParamsVPPx[0] = (mfxExtBuffer*)&zz;
            //var t1 = stackalloc uint[100];
            //zz.AlgList = t1;
            //zz.NumAlg = 100;
            //mfxVideoParam par;
            //par.ExtParam = pExtParamsVPPx;
            //par.NumExtParam = 1;
            //sts = UnsafeNativeMethods.MFXVideoVPP_GetVideoParam(session, &par);
            //Trace.Assert(sts == mfxStatus.MFX_ERR_NONE);
            //Console.WriteLine(zz.NumAlg);
            //for (int i = 0; i < 10; i++)
            //{
            //    Console.WriteLine((BufferId)t1[i]);
            //}
            mfxVideoParam par;







            // Retrieve video parameters selected by encoder.
            // - BufferSizeInKB parameter is required to set bit stream buffer size
            par = new mfxVideoParam();
            sts = UnsafeNativeMethods.MFXVideoENCODE_GetVideoParam(session, &par);
            QuickSyncStatic.ThrowOnBadStatus(sts, "enc.getvideoparams");


            // from mediasdkjpeg-man.pdf
            // BufferSizeInKB = 4 + (Width * Height * BytesPerPx + 1023) / 1024;
            //where Width and Height are weight and height of the picture in pixel, BytesPerPx is number of
            //byte for one pixel.It equals to 1 for monochrome picture, 1.5 for NV12 and YV12 color formats,
            //	2 for YUY2 color format, and 3 for RGB32 color format(alpha channel is not encoded).

            int MaxLength = (par.mfx.BufferSizeInKB * 1000);

            if (MaxLength == 0 && mfxEncParams.mfx.CodecId == CodecId.MFX_CODEC_JPEG)
                MaxLength = (4 + (mfxEncParams.mfx.FrameInfo.CropW * mfxEncParams.mfx.FrameInfo.CropH * 3 + 1023)) / 1000;




            // Create task pool to improve asynchronous performance (greater GPU utilization)

            taskPoolSize = mfxEncParams.AsyncDepth;  // number of tasks that can be submitted, before synchronizing is required
                                                     //  Task* pTasks = stackalloc Task[taskPoolSize];
            pTasks = (Task*)MyAllocHGlobalAndZero(sizeof(Task) * taskPoolSize);
            // GCHandle gch3 = GCHandle.Alloc(pTasks, GCHandleType.Pinned);
            for (int i = 0; i < taskPoolSize; i++)
            {
                // Prepare Media SDK bit stream buffer
                pTasks[i].mfxBS.MaxLength = (uint)MaxLength;
                pTasks[i].mfxBS.Data = MyAllocHGlobalAndZero((int)pTasks[i].mfxBS.MaxLength);
                Trace.Assert(pTasks[i].mfxBS.Data != IntPtr.Zero);
            }

            // GCHandle gch3 = GCHandle.Alloc(pTasks, GCHandleType.Pinned);


        }

        private static void ConfigureComposition(ref mfxVideoParam mfxVPPParams)
        {

            // add composition to VPPParams/mfxvideoparams
            mfxExtVPPComposite extVPPComposite;
            extVPPComposite.Header.BufferId = BufferId.MFX_EXTBUFF_VPP_COMPOSITE;
            extVPPComposite.Header.BufferSz = (uint)sizeof(mfxExtVPPComposite);

            extVPPComposite.NumInputStream = 2;
            var inputStreams = stackalloc mfxVPPCompInputStream[extVPPComposite.NumInputStream];
            extVPPComposite.InputStream_ptr = &inputStreams[0];

            //extVPPComposite.Y = 0;
            //extVPPComposite.U = 128;
            //extVPPComposite.V = 128;

            for (int i = 0; i < extVPPComposite.NumInputStream; i++)
            {
                if (i > 0)
                {
                    var a = (byte*)&inputStreams[i];
                    var b = (byte*)&inputStreams[i - 1];
                    var x = a - b;
                    var c = sizeof(mfxVPPCompInputStream);
                    Trace.Assert(x == sizeof(mfxVPPCompInputStream));
                }
                extVPPComposite.InputStream_ptr[i].DstX = 0;
                extVPPComposite.InputStream_ptr[i].DstY = 0;
                extVPPComposite.InputStream_ptr[i].DstW = 320;
                extVPPComposite.InputStream_ptr[i].DstH = 180;




                if (i > 0)
                    extVPPComposite.InputStream_ptr[i].LumaKeyEnable = 1;
                extVPPComposite.InputStream_ptr[i].LumaKeyMin = 0;
                extVPPComposite.InputStream_ptr[i].LumaKeyMax = 0;
            }

            mfxExtBuffer** pExtParamsVPP2 = stackalloc mfxExtBuffer*[1];
            pExtParamsVPP2[0] = (mfxExtBuffer*)&extVPPComposite;



            mfxVPPParams.ExtParam = pExtParamsVPP2;
            mfxVPPParams.NumExtParam = 1;
        }

        /// <summary>Gets the buffer free count.</summary>
        public int BufferFreeCount { get { return (int)(mfxBS->MaxLength - mfxBS->DataLength); } }
        /// <summary>Gets the size of the buffer.</summary>
        public int BufferSize { get { return (int)mfxBS->MaxLength; } }

        /// <summary>Store bitstream for transcoding</summary>
        /// <param name="inbuf">The inbuf.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <exception cref="QuickSyncException">insufficient space in buffer</exception>
        public void PutBitstream(byte[] inbuf, int offset, int length)
        {
            FastMemcpyMemmove.memmove(mfxBS->Data, mfxBS->Data + (int)mfxBS->DataOffset, (int)mfxBS->DataLength);
            mfxBS->DataOffset = 0;



            int free = (int)(mfxBS->MaxLength - mfxBS->DataLength);
            //Trace.Assert(length <= free);
            if (free < length)
                throw new QuickSyncException("insufficient space in buffer");

            Marshal.Copy(inbuf, offset, mfxBS->Data + (int)mfxBS->DataLength, length);

            mfxBS->DataLength += (uint)length;
        }



        /// <summary>Gets the next frame if available</summary>
        /// <param name="bitStreamChunk">The bit stream chunk.</param>
        /// <returns>true if you should continue to call this method, false if you must go to the next phase.</returns>
        public bool GetNextFrame(ref BitStreamChunk bitStreamChunk)
        {

            mfxSyncPoint syncpD, syncpV;
            mfxFrameSurface1* pmfxOutSurface = (mfxFrameSurface1*)0;

            int nIndex = 0;
            int nIndex2 = 0;


            bitStreamChunk.bytesAvailable = 0;

            ////////

            mfxStatus sts = mfxStatus.MFX_ERR_NONE;
            //
            // Stage 1: Main transcoding loop
            //
            if (mfxStatus.MFX_ERR_NONE <= sts || mfxStatus.MFX_ERR_MORE_DATA == sts || mfxStatus.MFX_ERR_MORE_SURFACE == sts)
            {
                int nTaskIdx = GetFreeTaskIndex(pTasks, taskPoolSize);      // Find free task
                if ((int)mfxStatus.MFX_ERR_NOT_FOUND == nTaskIdx)
                {
                    // No more free tasks, need to sync
                    sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
                    QuickSyncStatic.ThrowOnBadStatus(sts, "syncOper");


                    if (bitStreamChunk.bitstream == null || bitStreamChunk.bitstream.Length < pTasks[nFirstSyncTask].mfxBS.DataLength)
                        bitStreamChunk.bitstream = new byte[pTasks[nFirstSyncTask].mfxBS.DataLength];
                    Trace.Assert(pTasks[nFirstSyncTask].mfxBS.DataOffset == 0);
                    Marshal.Copy(pTasks[nFirstSyncTask].mfxBS.Data, bitStreamChunk.bitstream, 0, (int)pTasks[nFirstSyncTask].mfxBS.DataLength);

                    bitStreamChunk.bytesAvailable = (int)pTasks[nFirstSyncTask].mfxBS.DataLength;

                    // WriteBitStreamFrame(pTasks[nFirstSyncTask].mfxBS, outbs);
                    //MSDK_BREAK_ON_ERROR(sts);
                    pTasks[nFirstSyncTask].syncp.sync = IntPtr.Zero;
                    pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
                    pTasks[nFirstSyncTask].mfxBS.DataOffset = 0;
                    nFirstSyncTask = (nFirstSyncTask + 1) % taskPoolSize;


                    return true;



                }
                else
                {
                    if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                        Thread.Sleep(1);  // just wait and then repeat the same call to DecodeFrameAsync

                    if (mfxStatus.MFX_ERR_MORE_DATA == sts)
                    {
                        return false;
                        // Trace.Assert(false);
                        //sts = ReadBitStreamData(&mfxBS, fSource);       // Read more data to input bit stream
                        //MSDK_BREAK_ON_ERROR(sts);
                    }

                    if (mfxStatus.MFX_ERR_MORE_SURFACE == sts || mfxStatus.MFX_ERR_NONE == sts)
                    {
                        nIndex = GetFreeSurfaceIndex(pSurfaces, nSurfNumDecVPP);        // Find free frame surface
                        Trace.Assert(nIndex != (int)mfxStatus.MFX_ERR_NOT_FOUND);
                    }


                    //  - If input bitstream contains multiple frames DecodeFrameAsync will start decoding multiple frames, and remove them from bitstream

                    sts = UnsafeNativeMethods.MFXVideoDECODE_DecodeFrameAsync(session, mfxBS, &pSurfaces[nIndex], &pmfxOutSurface, &syncpD);

                    if (mfxStatus.MFX_ERR_MORE_SURFACE == sts)
                    {
                        return true;
                    }
                    // Ignore warnings if output is available,
                    // if no output and no action required just repeat the DecodeFrameAsync call
                    if (mfxStatus.MFX_ERR_NONE < sts && syncpD.sync != IntPtr.Zero)
                        sts = mfxStatus.MFX_ERR_NONE;

                    if (sts != mfxStatus.MFX_ERR_MORE_DATA && sts != mfxStatus.MFX_ERR_MORE_SURFACE)
                        QuickSyncStatic.ThrowOnBadStatus(sts, "decodeAsync");

                    if (mfxStatus.MFX_ERR_NONE == sts)
                    {




                        int compositeFrameIndex = 0;
                        morevpp:
                        nIndex2 = GetFreeSurfaceIndex(pSurfaces2, nSurfNumVPPEnc);      // Find free frame surface
                        Trace.Assert(nIndex2 != (int)mfxStatus.MFX_ERR_NOT_FOUND);


                        for (;;)
                        {
                            var z = pmfxOutSurface;
                            // if (compositeFrameIndex == 1)
                            //      z = overlay;
                            Trace.Assert(compositeFrameIndex <= 1);
                            // Process a frame asychronously (returns immediately)
                            sts = UnsafeNativeMethods.MFXVideoVPP_RunFrameVPPAsync(session, z, &pSurfaces2[nIndex2], (mfxExtVppAuxData*)0, &syncpV);

                            // COMPOSITING



                            if (mfxStatus.MFX_ERR_NONE < sts && syncpV.sync == IntPtr.Zero)
                            {    // repeat the call if warning and no output
                                if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)

                                    Thread.Sleep(1);  // wait if device is busy
                            }
                            else if (mfxStatus.MFX_ERR_NONE < sts && syncpV.sync != IntPtr.Zero)
                            {
                                sts = mfxStatus.MFX_ERR_NONE;     // ignore warnings if output is available
                                break;
                            }

                            else
                            {
                                if (sts != mfxStatus.MFX_ERR_MORE_DATA && sts != mfxStatus.MFX_ERR_MORE_SURFACE)
                                    QuickSyncStatic.ThrowOnBadStatus(sts, "vppAsync");
                                break;  // not a warning
                            }
                        }

                        // VPP needs more data, let decoder decode another frame as input
                        if (mfxStatus.MFX_ERR_MORE_DATA == sts)
                        {
                            compositeFrameIndex++;
                            goto morevpp;
                        }
                        else if (mfxStatus.MFX_ERR_MORE_SURFACE == sts)
                        {
                            // Not relevant for the illustrated workload! Therefore not handled.
                            // Relevant for cases when VPP produces more frames at output than consumes at input. E.g. framerate conversion 30 fps -> 60 fps
                            QuickSyncStatic.ThrowOnBadStatus(sts, "vppAsync");
                        }
                        else
                            if (mfxStatus.MFX_ERR_NONE != sts)
                            QuickSyncStatic.ThrowOnBadStatus(sts, "dec or vpp");



                        sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, syncpV, 60000);
                        QuickSyncStatic.ThrowOnBadStatus(sts, "syncOper");

                        for (;;)
                        {
                            // Encode a frame asychronously (returns immediately)
                            //sts = mfxENC.EncodeFrameAsync(NULL, pSurfaces2[nIndex2], &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);
                            sts = UnsafeNativeMethods.MFXVideoENCODE_EncodeFrameAsync(session, (mfxEncodeCtrl*)0, &pSurfaces2[nIndex2], &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);

                            if (mfxStatus.MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp.sync == IntPtr.Zero)
                            {    // repeat the call if warning and no output
                                if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                                    Thread.Sleep(1);  // wait if device is busy
                            }
                            else if (mfxStatus.MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp.sync != IntPtr.Zero)
                            {
                                sts = mfxStatus.MFX_ERR_NONE;     // ignore warnings if output is available
                                break;
                            }
                            else if (mfxStatus.MFX_ERR_NOT_ENOUGH_BUFFER == sts)
                            {
                                // Allocate more bitstream buffer memory here if needed...
                                break;
                            }
                            else if (mfxStatus.MFX_ERR_MORE_SURFACE == sts)
                            {
                                return true;
                            }
                            else
                            {
                                if (sts != mfxStatus.MFX_ERR_MORE_DATA && sts != mfxStatus.MFX_ERR_MORE_SURFACE)
                                    QuickSyncStatic.ThrowOnBadStatus(sts, "encodeAsync");
                                break;
                            }
                        }
                    }
                }
            }


            // MFX_ERR_MORE_DATA means that file has ended, need to go to buffering loop, exit in case of other errors
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;

            QuickSyncStatic.ThrowOnBadStatus(sts, "dec or enc or vpp");


            return true;
        }

        /// <summary>Get frames during 1st stage of flushing</summary>
        /// <param name="bitStreamChunk">A single frame</param>
        /// <returns>true if you should continue to call this method, false if you must go to the next stage.</returns>
        public bool Flush1(ref BitStreamChunk bitStreamChunk)
        {

            mfxSyncPoint syncpD, syncpV;
            mfxFrameSurface1* pmfxOutSurface = (mfxFrameSurface1*)0;

            int nIndex = 0;
            int nIndex2 = 0;


            bitStreamChunk.bytesAvailable = 0;

            ////////

            mfxStatus sts = mfxStatus.MFX_ERR_NONE;
            //
            // Stage 2: Retrieve the buffered decoded frames
            //
            if (mfxStatus.MFX_ERR_NONE <= sts || mfxStatus.MFX_ERR_MORE_SURFACE == sts)
            {
                int nTaskIdx = GetFreeTaskIndex(pTasks, taskPoolSize);      // Find free task
                if ((int)mfxStatus.MFX_ERR_NOT_FOUND == nTaskIdx)
                {
                    // No more free tasks, need to sync
                    sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
                    QuickSyncStatic.ThrowOnBadStatus(sts, "syncOper");


                    if (bitStreamChunk.bitstream == null || bitStreamChunk.bitstream.Length < pTasks[nFirstSyncTask].mfxBS.DataLength)
                        bitStreamChunk.bitstream = new byte[pTasks[nFirstSyncTask].mfxBS.DataLength];
                    Trace.Assert(pTasks[nFirstSyncTask].mfxBS.DataOffset == 0);
                    Marshal.Copy(pTasks[nFirstSyncTask].mfxBS.Data, bitStreamChunk.bitstream, 0, (int)pTasks[nFirstSyncTask].mfxBS.DataLength);

                    bitStreamChunk.bytesAvailable = (int)pTasks[nFirstSyncTask].mfxBS.DataLength;

                    // WriteBitStreamFrame(pTasks[nFirstSyncTask].mfxBS, outbs);
                    //MSDK_BREAK_ON_ERROR(sts);
                    pTasks[nFirstSyncTask].syncp.sync = IntPtr.Zero;
                    pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
                    pTasks[nFirstSyncTask].mfxBS.DataOffset = 0;
                    nFirstSyncTask = (nFirstSyncTask + 1) % taskPoolSize;


                    return true;
                }
                else
                {
                    if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                        Thread.Sleep(1);  // just wait and then repeat the same call to DecodeFrameAsync


                    nIndex = GetFreeSurfaceIndex(pSurfaces, nSurfNumDecVPP);        // Find free frame surface
                    Trace.Assert(nIndex != (int)mfxStatus.MFX_ERR_NOT_FOUND);

                    //  - If input bitstream contains multiple frames DecodeFrameAsync will start decoding multiple frames, and remove them from bitstream

                    sts = UnsafeNativeMethods.MFXVideoDECODE_DecodeFrameAsync(session, null, &pSurfaces[nIndex], &pmfxOutSurface, &syncpD);

                    // Ignore warnings if output is available,
                    // if no output and no action required just repeat the DecodeFrameAsync call
                    if (mfxStatus.MFX_ERR_NONE < sts && syncpD.sync != IntPtr.Zero)
                        sts = mfxStatus.MFX_ERR_NONE;

                    if (sts != mfxStatus.MFX_ERR_MORE_DATA && sts != mfxStatus.MFX_ERR_MORE_SURFACE)
                        QuickSyncStatic.ThrowOnBadStatus(sts, "decodeAsync");

                    if (mfxStatus.MFX_ERR_NONE == sts)
                    {




                        int compositeFrameIndex = 0;
                        // morevpp:
                        nIndex2 = GetFreeSurfaceIndex(pSurfaces2, nSurfNumVPPEnc);      // Find free frame surface
                        Trace.Assert(nIndex2 != (int)mfxStatus.MFX_ERR_NOT_FOUND);


                        for (;;)
                        {
                            var z = pmfxOutSurface;
                            // if (compositeFrameIndex == 1)
                            //      z = overlay;
                            Trace.Assert(compositeFrameIndex <= 1);
                            // Process a frame asychronously (returns immediately)
                            sts = UnsafeNativeMethods.MFXVideoVPP_RunFrameVPPAsync(session, z, &pSurfaces2[nIndex2], (mfxExtVppAuxData*)0, &syncpV);

                            // COMPOSITING



                            if (mfxStatus.MFX_ERR_NONE < sts && syncpV.sync == IntPtr.Zero)
                            {    // repeat the call if warning and no output
                                if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)

                                    Thread.Sleep(1);  // wait if device is busy
                            }
                            else if (mfxStatus.MFX_ERR_NONE < sts && syncpV.sync != IntPtr.Zero)
                            {
                                sts = mfxStatus.MFX_ERR_NONE;     // ignore warnings if output is available
                                break;
                            }
                            else
                            {
                                if (sts != mfxStatus.MFX_ERR_MORE_DATA && sts != mfxStatus.MFX_ERR_MORE_SURFACE)
                                    QuickSyncStatic.ThrowOnBadStatus(sts, "vppAsync");
                                break;  // not a warning
                            }
                        }

                        // VPP needs more data, let decoder decode another frame as input
                        if (mfxStatus.MFX_ERR_MORE_DATA == sts)
                        {
                            //compositeFrameIndex++;
                            //goto morevpp;
                            return false;
                        }
                        else if (mfxStatus.MFX_ERR_MORE_SURFACE == sts)
                        {
                            // Not relevant for the illustrated workload! Therefore not handled.
                            // Relevant for cases when VPP produces more frames at output than consumes at input. E.g. framerate conversion 30 fps -> 60 fps
                            QuickSyncStatic.ThrowOnBadStatus(sts, "flush1");
                        }
                        else
                            if (mfxStatus.MFX_ERR_NONE != sts)
                            QuickSyncStatic.ThrowOnBadStatus(sts, "dec or vpp"); ;


                        for (;;)
                        {
                            // Encode a frame asychronously (returns immediately)
                            //sts = mfxENC.EncodeFrameAsync(NULL, pSurfaces2[nIndex2], &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);
                            sts = UnsafeNativeMethods.MFXVideoENCODE_EncodeFrameAsync(session, (mfxEncodeCtrl*)0, &pSurfaces2[nIndex2], &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);

                            if (mfxStatus.MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp.sync == IntPtr.Zero)
                            {    // repeat the call if warning and no output
                                if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                                    Thread.Sleep(1);  // wait if device is busy
                            }
                            else if (mfxStatus.MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp.sync != IntPtr.Zero)
                            {
                                sts = mfxStatus.MFX_ERR_NONE;     // ignore warnings if output is available
                                break;
                            }
                            else if (mfxStatus.MFX_ERR_NOT_ENOUGH_BUFFER == sts)
                            {
                                // Allocate more bitstream buffer memory here if needed...
                                break;
                            }
                            else
                            {
                                if (sts != mfxStatus.MFX_ERR_MORE_DATA && sts != mfxStatus.MFX_ERR_MORE_SURFACE)
                                    QuickSyncStatic.ThrowOnBadStatus(sts, "encodeAsync");
                                break;
                            }

                        }


                    }
                }
            }

            // MFX_ERR_MORE_DATA means that file has ended, need to go to buffering loop, exit in case of other errors
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;

            QuickSyncStatic.ThrowOnBadStatus(sts, "dec or enc or vpp");

            return true;
        }

        /// <summary>Get frames during 2nd stage of flushing</summary>
        /// <param name="bitStreamChunk">A single frame</param>
        /// <returns>true if you should continue to call this method, false if you must go to the next stage.</returns>
        public bool Flush2(ref BitStreamChunk bitStreamChunk)
        {

            mfxSyncPoint syncpV;
            mfxFrameSurface1* pmfxOutSurface = (mfxFrameSurface1*)0;


            int nIndex2 = 0;


            bitStreamChunk.bytesAvailable = 0;

            ////////

            mfxStatus sts = mfxStatus.MFX_ERR_NONE;
            //
            // Stage 3: Retrieve buffered frames from VPP
            //
            if (mfxStatus.MFX_ERR_NONE <= sts || mfxStatus.MFX_ERR_MORE_DATA == sts || mfxStatus.MFX_ERR_MORE_SURFACE == sts)
            {
                int nTaskIdx = GetFreeTaskIndex(pTasks, taskPoolSize);      // Find free task
                if ((int)mfxStatus.MFX_ERR_NOT_FOUND == nTaskIdx)
                {
                    // No more free tasks, need to sync
                    sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
                    QuickSyncStatic.ThrowOnBadStatus(sts, "syncOper");


                    if (bitStreamChunk.bitstream == null || bitStreamChunk.bitstream.Length < pTasks[nFirstSyncTask].mfxBS.DataLength)
                        bitStreamChunk.bitstream = new byte[pTasks[nFirstSyncTask].mfxBS.DataLength];
                    Trace.Assert(pTasks[nFirstSyncTask].mfxBS.DataOffset == 0);
                    Marshal.Copy(pTasks[nFirstSyncTask].mfxBS.Data, bitStreamChunk.bitstream, 0, (int)pTasks[nFirstSyncTask].mfxBS.DataLength);

                    bitStreamChunk.bytesAvailable = (int)pTasks[nFirstSyncTask].mfxBS.DataLength;

                    // WriteBitStreamFrame(pTasks[nFirstSyncTask].mfxBS, outbs);
                    //MSDK_BREAK_ON_ERROR(sts);
                    pTasks[nFirstSyncTask].syncp.sync = IntPtr.Zero;
                    pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
                    pTasks[nFirstSyncTask].mfxBS.DataOffset = 0;
                    nFirstSyncTask = (nFirstSyncTask + 1) % taskPoolSize;


                    return true;



                }
                else
                {

                    int compositeFrameIndex = 0;
                    //morevpp:
                    nIndex2 = GetFreeSurfaceIndex(pSurfaces2, nSurfNumVPPEnc);      // Find free frame surface
                    Trace.Assert(nIndex2 != (int)mfxStatus.MFX_ERR_NOT_FOUND);


                    for (;;)
                    {
                        var z = pmfxOutSurface;
                        z = null;
                        // if (compositeFrameIndex == 1)
                        //      z = overlay;
                        Trace.Assert(compositeFrameIndex <= 1);
                        // Process a frame asychronously (returns immediately)
                        sts = UnsafeNativeMethods.MFXVideoVPP_RunFrameVPPAsync(session, z, &pSurfaces2[nIndex2], (mfxExtVppAuxData*)0, &syncpV);

                        // COMPOSITING



                        if (mfxStatus.MFX_ERR_NONE < sts && syncpV.sync == IntPtr.Zero)
                        {    // repeat the call if warning and no output
                            if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)

                                Thread.Sleep(1);  // wait if device is busy
                        }
                        else if (mfxStatus.MFX_ERR_NONE < sts && syncpV.sync != IntPtr.Zero)
                        {
                            sts = mfxStatus.MFX_ERR_NONE;     // ignore warnings if output is available
                            break;
                        }
                        else
                        {
                            if (sts != mfxStatus.MFX_ERR_MORE_DATA && sts != mfxStatus.MFX_ERR_MORE_SURFACE)
                                QuickSyncStatic.ThrowOnBadStatus(sts, "vppAsync");
                            break;  // not a warning
                        }
                    }

                    //VPP needs more data, let decoder decode another frame as input
                    if (mfxStatus.MFX_ERR_MORE_DATA == sts)
                    {
                        return false;
                        // compositeFrameIndex++;
                        //goto morevpp;
                    }
                    else
                    if (mfxStatus.MFX_ERR_MORE_SURFACE == sts)
                    {
                        // Not relevant for the illustrated workload! Therefore not handled.
                        // Relevant for cases when VPP produces more frames at output than consumes at input. E.g. framerate conversion 30 fps -> 60 fps
                        QuickSyncStatic.ThrowOnBadStatus(sts, "vpp"); ;
                    }
                    else
                        if (mfxStatus.MFX_ERR_NONE != sts)
                        QuickSyncStatic.ThrowOnBadStatus(sts, "vpp"); ;


                    for (;;)
                    {
                        // Encode a frame asychronously (returns immediately)
                        //sts = mfxENC.EncodeFrameAsync(NULL, pSurfaces2[nIndex2], &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);
                        sts = UnsafeNativeMethods.MFXVideoENCODE_EncodeFrameAsync(session, (mfxEncodeCtrl*)0, &pSurfaces2[nIndex2], &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);

                        if (mfxStatus.MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp.sync == IntPtr.Zero)
                        {    // repeat the call if warning and no output
                            if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                                Thread.Sleep(1);  // wait if device is busy
                        }
                        else if (mfxStatus.MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp.sync != IntPtr.Zero)
                        {
                            sts = mfxStatus.MFX_ERR_NONE;     // ignore warnings if output is available
                            break;
                        }
                        else if (mfxStatus.MFX_ERR_NOT_ENOUGH_BUFFER == sts)
                        {
                            // Allocate more bitstream buffer memory here if needed...
                            break;
                        }
                        else
                        {
                            if (sts != mfxStatus.MFX_ERR_MORE_DATA && sts != mfxStatus.MFX_ERR_MORE_SURFACE)
                                QuickSyncStatic.ThrowOnBadStatus(sts, "encodeAsync");
                            break;
                        }

                    }
                }
            }


            // MFX_ERR_MORE_DATA means that file has ended, need to go to buffering loop, exit in case of other errors
            //MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;
            QuickSyncStatic.ThrowOnBadStatus(sts, "dec or enc or vpp");

            return true;

        }

        /// <summary>Get frames during 2nd stage of flushing</summary>
        /// <param name="bitStreamChunk">A single frame</param>
        /// <returns>true if you should continue to call this method, false if you must go to the next phase.</returns>
        public bool Flush3(ref BitStreamChunk bitStreamChunk)
        {
            bitStreamChunk.bytesAvailable = 0;

            ////////

            mfxStatus sts = mfxStatus.MFX_ERR_NONE;
            //
            // Stage 4: Retrieve the buffered encoded frames
            //
            if (mfxStatus.MFX_ERR_NONE <= sts)
            {
                int nTaskIdx = GetFreeTaskIndex(pTasks, taskPoolSize);      // Find free task
                if ((int)mfxStatus.MFX_ERR_NOT_FOUND == nTaskIdx)
                {
                    // No more free tasks, need to sync
                    sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
                    QuickSyncStatic.ThrowOnBadStatus(sts, "syncOper");


                    if (bitStreamChunk.bitstream == null || bitStreamChunk.bitstream.Length < pTasks[nFirstSyncTask].mfxBS.DataLength)
                        bitStreamChunk.bitstream = new byte[pTasks[nFirstSyncTask].mfxBS.DataLength];
                    Trace.Assert(pTasks[nFirstSyncTask].mfxBS.DataOffset == 0);
                    Marshal.Copy(pTasks[nFirstSyncTask].mfxBS.Data, bitStreamChunk.bitstream, 0, (int)pTasks[nFirstSyncTask].mfxBS.DataLength);

                    bitStreamChunk.bytesAvailable = (int)pTasks[nFirstSyncTask].mfxBS.DataLength;

                    // WriteBitStreamFrame(pTasks[nFirstSyncTask].mfxBS, outbs);
                    //MSDK_BREAK_ON_ERROR(sts);
                    pTasks[nFirstSyncTask].syncp.sync = IntPtr.Zero;
                    pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
                    pTasks[nFirstSyncTask].mfxBS.DataOffset = 0;
                    nFirstSyncTask = (nFirstSyncTask + 1) % taskPoolSize;

                    return true;
                }
                else
                {
                    for (;;)
                    {
                        // Encode a frame asychronously (returns immediately)
                        //sts = mfxENC.EncodeFrameAsync(NULL, pSurfaces2[nIndex2], &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);
                        sts = UnsafeNativeMethods.MFXVideoENCODE_EncodeFrameAsync(session, (mfxEncodeCtrl*)0, null, &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);

                        if (mfxStatus.MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp.sync == IntPtr.Zero)
                        {    // repeat the call if warning and no output
                            if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                                Thread.Sleep(1);  // wait if device is busy
                        }
                        else if (mfxStatus.MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp.sync != IntPtr.Zero)
                        {
                            sts = mfxStatus.MFX_ERR_NONE;     // ignore warnings if output is available
                            break;
                        }
                        else if (mfxStatus.MFX_ERR_NOT_ENOUGH_BUFFER == sts)
                        {
                            // Allocate more bitstream buffer memory here if needed...
                            break;
                        }
                        else
                        {
                            if (sts != mfxStatus.MFX_ERR_MORE_DATA && sts != mfxStatus.MFX_ERR_MORE_SURFACE)
                                QuickSyncStatic.ThrowOnBadStatus(sts, "encodeAsync");
                            break;
                        }
                    }
                }
            }


            if (mfxStatus.MFX_ERR_MORE_DATA == sts)
                return false;

            QuickSyncStatic.ThrowOnBadStatus(sts, "enc error");

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bsc"></param>
        /// <returns>true:all done, false:continue to call me</returns>
        public bool Flush4(ref BitStreamChunk bsc)
        {
            mfxStatus sts;
            bsc.bytesAvailable = 0;
            while (pTasks[nFirstSyncTask].syncp.sync != IntPtr.Zero)
            {
                sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
                QuickSyncStatic.ThrowOnBadStatus(sts, "syncOper");

                if (bsc.bitstream == null || bsc.bitstream.Length < pTasks[nFirstSyncTask].mfxBS.DataLength)
                    bsc.bitstream = new byte[pTasks[nFirstSyncTask].mfxBS.DataLength];
                Trace.Assert(pTasks[nFirstSyncTask].mfxBS.DataOffset == 0);
                Marshal.Copy(pTasks[nFirstSyncTask].mfxBS.Data, bsc.bitstream, 0, (int)pTasks[nFirstSyncTask].mfxBS.DataLength);

                bsc.bytesAvailable = (int)pTasks[nFirstSyncTask].mfxBS.DataLength;

                // WriteBitStreamFrame(pTasks[nFirstSyncTask].mfxBS, outbs);
                //MSDK_BREAK_ON_ERROR(sts);
                pTasks[nFirstSyncTask].syncp.sync = IntPtr.Zero;
                pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
                pTasks[nFirstSyncTask].mfxBS.DataOffset = 0;
                nFirstSyncTask = (nFirstSyncTask + 1) % taskPoolSize;
                return true;
            }
            return false;
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (pSurfaces != null) Marshal.FreeHGlobal((IntPtr)pSurfaces);
                if (pSurfaces2 != null) Marshal.FreeHGlobal((IntPtr)pSurfaces2);
                if (mfxBS != null && mfxBS->Data != null) Marshal.FreeHGlobal((IntPtr)mfxBS->Data);
                if (mfxBS != null) Marshal.FreeHGlobal((IntPtr)mfxBS);
                for (int i = 0; i < taskPoolSize; i++)
                {
                    if (pTasks != null)
                        Marshal.FreeHGlobal(pTasks[i].mfxBS.Data);
                }
                if (pTasks != null)
                    Marshal.FreeHGlobal((IntPtr)pTasks);




                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.        
        /// <summary>Finalizes an instance of the <see cref="LowLevelTranscoderCSharp"/> class.</summary>
        ~LowLevelTranscoderCSharp()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
