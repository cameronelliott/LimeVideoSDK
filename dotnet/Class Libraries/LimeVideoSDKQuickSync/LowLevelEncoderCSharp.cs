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
    /// Turns uncompressed frames into compressed video bitstreams.
    /// </summary>
    unsafe public class LowLevelEncoderCSharp : IDisposable, ILowLevelEncoder
    {
        /// <summary>The session</summary>
        public mfxSession session;
        /// <summary>The warnings</summary>
        public Dictionary<string, mfxStatus> warnings = new Dictionary<string, mfxStatus>();
        /// <summary>The bitstream</summary>
        public mfxBitstream bitstream;
        /// <summary>The frames</summary>
        public mfxFrameSurface1[] Frames;
        /// <summary>The device setup</summary>
        //public DeviceSetup deviceSetup;

        IntPtr[] frameIntPtrs;

        struct Task
        {
            public mfxBitstream mfxBS;
            public mfxSyncPoint syncp;
        };
        Task[] pTasks;
        private bool disposed = false;

        //   IntPtr bitstreamBuffer;
        //   IntPtr surfaceBuffers;
        //   IntPtr surfaceBuffers2;
        List<GCHandle> pinningHandles = new List<GCHandle>();
        int nFirstSyncTask = 0;



        /// <summary>
        /// Place decoder configuration data in here.
        /// Set Config.ImplRequested before calling Init()
        /// Set Config.mfxVideoParams.mfx.CodecId before calling DecodeHeader()
        /// See samples for concrete examples.
        /// </summary>

        int GetFreeTaskIndex(Task[] pTaskPool)
        {
            Trace.Assert(pTaskPool != null);

            for (int i = 0; i < pTaskPool.Length; i++)
                if (pTaskPool[i].syncp.sync == IntPtr.Zero)
                    return i;
            return (int)mfxStatus.MFX_ERR_NOT_FOUND;
        }

        int GetFreeSurfaceIndex(mfxFrameSurface1[] pSurfacesPool)
        {

            for (int i = 0; i < pSurfacesPool.Length; i++)
                if (0 == pSurfacesPool[i].Data.Locked)
                    return i;
            return (int)mfxStatus.MFX_ERR_NOT_FOUND;
        }



        /// <summary>Initializes a new instance of the <see cref="LowLevelEncoderCSharp"/> class.</summary>
        /// <param name="mfxEncParams">The encoder parameters.</param>
        /// <param name="impl">The implementation.</param>
        public LowLevelEncoderCSharp(mfxVideoParam mfxEncParams, mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO)
        {
            mfxStatus sts;

            session = new mfxSession();
            var ver = new mfxVersion() { Major = 1, Minor = 3 };
            fixed (mfxSession* s = &session)
                sts = UnsafeNativeMethods.MFXInit(impl, &ver, s);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXInit");
            //deviceSetup = new DeviceSetup(session, false);





            sts = UnsafeNativeMethods.MFXVideoENCODE_Query(session, &mfxEncParams, &mfxEncParams);
            if (sts > 0)
            {
                warnings.Add(nameof(UnsafeNativeMethods.MFXVideoENCODE_Query), sts);
                sts = 0;
            }
            QuickSyncStatic.ThrowOnBadStatus(sts, "encodequery");


            mfxFrameAllocRequest EncRequest;
            sts = UnsafeNativeMethods.MFXVideoENCODE_QueryIOSurf(session, &mfxEncParams, &EncRequest);
            QuickSyncStatic.ThrowOnBadStatus(sts, "queryiosurf");

            EncRequest.NumFrameSuggested = (ushort)(EncRequest.NumFrameSuggested + mfxEncParams.AsyncDepth);

            EncRequest.Type |= (FrameMemoryType)0x2000; // WILL_WRITE; // This line is only required for Windows DirectX11 to ensure that surfaces can be written to by the application

            UInt16 numSurfaces = EncRequest.NumFrameSuggested;

            // - Width and height of buffer must be aligned, a multiple of 32
            // - Frame surface array keeps pointers all surface planes and general frame info

            UInt16 width = (UInt16)QuickSyncStatic.ALIGN32(mfxEncParams.mfx.FrameInfo.Width);
            UInt16 height = (UInt16)QuickSyncStatic.ALIGN32(mfxEncParams.mfx.FrameInfo.Height);
            int bitsPerPixel = VideoUtility.GetBitsPerPixel(mfxEncParams.mfx.FrameInfo.FourCC);
            int surfaceSize = width * height * bitsPerPixel / 8;
            //byte[] surftaceBuffers = new byte[surfaceSize * numSurfaces]; //XXX
            IntPtr surfaceBuffers = Marshal.AllocHGlobal(surfaceSize * numSurfaces);
            byte* surfaceBuffersPtr = (byte*)surfaceBuffers;


            //         // Allocate surface headers (mfxFrameSurface1) for decoder
            Frames = new mfxFrameSurface1[numSurfaces];
            //MSDK_CHECK_POINTER(pmfxSurfaces, MFX_ERR_MEMORY_ALLOC);
            for (int i = 0; i < numSurfaces; i++)
            {
                Frames[i] = new mfxFrameSurface1();
                Frames[i].Info = mfxEncParams.mfx.FrameInfo;

                switch (mfxEncParams.mfx.FrameInfo.FourCC)
                {
                    case FourCC.NV12:
                        Frames[i].Data.Y_ptr = (byte*)surfaceBuffers + i * surfaceSize;
                        Frames[i].Data.U_ptr = Frames[i].Data.Y_ptr + width * height;
                        Frames[i].Data.V_ptr = Frames[i].Data.U_ptr + 1;
                        Frames[i].Data.Pitch = width;
                        break;
                    case FourCC.YUY2:
                        Frames[i].Data.Y_ptr = (byte*)surfaceBuffers + i * surfaceSize;
                        Frames[i].Data.U_ptr = Frames[i].Data.Y_ptr + 1;
                        Frames[i].Data.V_ptr = Frames[i].Data.U_ptr + 3;
                        Frames[i].Data.Pitch = (ushort)(width * 2);
                        break;
                    default:  //find sysmem_allocator.cpp for more help
                        throw new NotImplementedException();
                }
            }

            frameIntPtrs = new IntPtr[Frames.Length];
            for (int i = 0; i < Frames.Length; i++)
            {
                fixed (mfxFrameSurface1* a = &Frames[i])
                    frameIntPtrs[i] = (IntPtr)a;
            }


            sts = UnsafeNativeMethods.MFXVideoENCODE_Init(session, &mfxEncParams);
            if (sts > 0)
            {
                warnings.Add(nameof(UnsafeNativeMethods.MFXVideoENCODE_Init), sts);
                sts = 0;
            }
            QuickSyncStatic.ThrowOnBadStatus(sts, "encodeinit");

            mfxVideoParam par;
            UnsafeNativeMethods.MFXVideoENCODE_GetVideoParam(session, &par);
            QuickSyncStatic.ThrowOnBadStatus(sts, "encodegetvideoparam");


            // from mediasdkjpeg-man.pdf
            // BufferSizeInKB = 4 + (Width * Height * BytesPerPx + 1023) / 1024;
            //where Width and Height are weight and height of the picture in pixel, BytesPerPx is number of
            //byte for one pixel.It equals to 1 for monochrome picture, 1.5 for NV12 and YV12 color formats,
            //	2 for YUY2 color format, and 3 for RGB32 color format(alpha channel is not encoded).

            if (par.mfx.BufferSizeInKB == 0 && mfxEncParams.mfx.CodecId == CodecId.MFX_CODEC_JPEG)
                par.mfx.BufferSizeInKB = (ushort)((4 + (mfxEncParams.mfx.FrameInfo.CropW * mfxEncParams.mfx.FrameInfo.CropH * 3 + 1023)) / 1000);
            //printf("bufsize %d\n", par.mfx.BufferSizeInKB);



            // Create task pool to improve asynchronous performance (greater GPU utilization)
            int taskPoolSize = mfxEncParams.AsyncDepth;  // number of tasks that can be submitted, before synchronizing is required


            //Task* pTasks             = stackalloc Task[taskPoolSize];
            // GCHandle gch3 = GCHandle.Alloc(pTasks, GCHandleType.Pinned);
            pTasks = new Task[taskPoolSize];

            for (int i = 0; i < taskPoolSize; i++)
            {
                // Prepare Media SDK bit stream buffer
                pTasks[i].mfxBS.MaxLength = (uint)(par.mfx.BufferSizeInKB * 1000);
                pTasks[i].mfxBS.Data = Marshal.AllocHGlobal((int)pTasks[i].mfxBS.MaxLength);
                Trace.Assert(pTasks[i].mfxBS.Data != IntPtr.Zero);
            }

            pinningHandles.Add(GCHandle.Alloc(pTasks, GCHandleType.Pinned));
            pinningHandles.Add(GCHandle.Alloc(Frames, GCHandleType.Pinned));
        }


        /// <summary>Gets the index of a free frame which can be used for encoding.</summary>
        /// <returns>free frame index</returns>
        public int GetFreeFrameIndex()
        {
            int nEncSurfIdx = GetFreeSurfaceIndex(Frames);   // Find free frame surface
                                                             //MSDK_CHECK_ERROR(MFX_ERR_NOT_FOUND, nEncSurfIdx, MFX_ERR_MEMORY_ALLOC);
            Trace.Assert(nEncSurfIdx != (int)mfxStatus.MFX_ERR_NOT_FOUND);
            return nEncSurfIdx;

        }

        void GetBitstreamIfAny(ref BitStreamChunk bsc)
        {
            mfxStatus sts = 0;
            bsc.bytesAvailable = 0;

            Trace.Assert(pTasks[nFirstSyncTask].syncp.sync_ptr != null);

            // No more free tasks, need to sync
            sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
            QuickSyncStatic.ThrowOnBadStatus(sts, "syncoper");

            //  sts = WriteBitStreamFrame(&pTasks[nFirstSyncTask].mfxBS, fSink);
            //  MSDK_BREAK_ON_ERROR(g);
            int n = (int)pTasks[nFirstSyncTask].mfxBS.DataLength;
            if (bsc.bitstream == null || bsc.bitstream.Length < n)
                bsc.bitstream = new byte[pTasks[nFirstSyncTask].mfxBS.MaxLength];
            Trace.Assert(pTasks[nFirstSyncTask].mfxBS.DataOffset == 0);
            Marshal.Copy(pTasks[nFirstSyncTask].mfxBS.Data, bsc.bitstream, 0, n);
            bsc.bytesAvailable = n;
            pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
            pTasks[nFirstSyncTask].syncp.sync_ptr = null;
            nFirstSyncTask = (nFirstSyncTask + 1) % pTasks.Length;
        }



        bool GetBitstreamIfFull(ref BitStreamChunk bsc)
        {
            bsc.bytesAvailable = 0;

            int nTaskIdx = GetFreeTaskIndex(pTasks);      // Find free task
            if ((int)mfxStatus.MFX_ERR_NOT_FOUND == nTaskIdx)
            {
                GetBitstreamIfAny(ref bsc);
                return true;
            }

            return false;
        }

        /// <summary>Encodes a frame.</summary>
        /// <param name="frameIndex">Index of the frame to encode.</param>
        /// <param name="bitStreamChunk">Output frames bitstream data, if available</param>
        public void EncodeFrame(int frameIndex, ref BitStreamChunk bitStreamChunk)
        {
            mfxStatus sts = 0;
            bitStreamChunk.bytesAvailable = 0;

            GetBitstreamIfFull(ref bitStreamChunk);

            // int nEncSurfIdx = 0;
            int nTaskIdx = GetFreeTaskIndex(pTasks);      // Find free task
            Trace.Assert((int)mfxStatus.MFX_ERR_NOT_FOUND != nTaskIdx);

            //int nsource = 0;

            //var buf = new byte[pTasks[0].mfxBS.MaxLength];

            //
            // Stage 1: Main encoding loop
            //
            //if (mfxStatus.MFX_ERR_NONE <= sts || mfxStatus.MFX_ERR_MORE_DATA == sts)
            //{


            // }
            // else
            // {
            //nEncSurfIdx = GetFreeSurfaceIndex(pmfxSurfaces);   // Find free frame surface
            //MSDK_CHECK_ERROR(MFX_ERR_NOT_FOUND, nEncSurfIdx, MFX_ERR_MEMORY_ALLOC);
            //Trace.Assert(nEncSurfIdx != (int)mfxStatus.MFX_ERR_NOT_FOUND);

            // Surface locking required when read/write D3D surfaces
            //sts = mfxAllocator.Lock(mfxAllocator.pthis, pmfxSurfaces[nEncSurfIdx]->Data.MemId, &(pmfxSurfaces[nEncSurfIdx]->Data));
            //MSDK_BREAK_ON_ERROR(sts);

            // sts = LoadRawFrame(pmfxSurfaces[nEncSurfIdx], fSource);
            // MSDK_BREAK_ON_ERROR(sts);

            // from the prototype, we just copy the frame data from a byte array,
            // but in this class we are passed a prepared frame.
            //int pfs = 320 * 180 * 3 / 2;
            //if (nsource * pfs >= yuv.Length)
            //    break;
            //int stride = pmfxSurfaces[nEncSurfIdx].Data.Pitch;
            //for (int i = 0; i < h; i++)
            //    Marshal.Copy(yuv, nsource * pfs + i * w, pmfxSurfaces[nEncSurfIdx].Data.Y + stride * i, w);
            //for (int i = 0; i < h / 2; i++)
            //    Marshal.Copy(yuv, nsource * pfs + i * w + h * w, pmfxSurfaces[nEncSurfIdx].Data.UV + stride * i, w);



            //sts = mfxAllocator.Unlock(mfxAllocator.pthis, pmfxSurfaces[nEncSurfIdx]->Data.MemId, &(pmfxSurfaces[nEncSurfIdx]->Data));
            //MSDK_BREAK_ON_ERROR(sts);


            // Frames[nEncSurfIdx] = frame;
            for (;;)
            {
                // Encode a frame asychronously (returns immediately)
                fixed (mfxFrameSurface1* a = &Frames[frameIndex])
                fixed (mfxBitstream* b = &pTasks[nTaskIdx].mfxBS)
                fixed (mfxSyncPoint* c = &pTasks[nTaskIdx].syncp)
                    sts = UnsafeNativeMethods.MFXVideoENCODE_EncodeFrameAsync(session, null, a, b, c);

                if (mfxStatus.MFX_ERR_NONE < sts && !(pTasks[nTaskIdx].syncp.sync_ptr != null))
                {    // Repeat the call if warning and no output
                    if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                        Thread.Sleep(1);  // Wait if device is busy, then repeat the same call
                }
                else if (mfxStatus.MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp.sync_ptr != null)
                {
                    sts = mfxStatus.MFX_ERR_NONE;     // Ignore warnings if output is available
                    break;
                }
                else if (mfxStatus.MFX_ERR_NOT_ENOUGH_BUFFER == sts)
                {
                    Trace.Assert(false);
                    // Allocate more bitstream buffer memory here if needed...
                    break;
                }
                else
                {
                    break;
                }
            }

            // }

            // MFX_ERR_MORE_DATA means that the input file has ended, need to go to buffering loop, exit in case of other errors
            //MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                sts = mfxStatus.MFX_ERR_NONE;
            //MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);            
            QuickSyncStatic.ThrowOnBadStatus(sts, "encodeFrameAsync");

            return;
        }


        bool flush1state = true;

        mfxSession ILowLevelEncoder.session
        {
            get
            {

                return this.session;
            }
        }

        IntPtr[] ILowLevelEncoder.Frames
        {
            get
            {
                return frameIntPtrs;
            }
        }

        //object ILowLevelEncoder.deviceSetup
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        /// <summary>After passing all frames to be encoded you must call this to flush out bitstream in the engine.</summary>
        /// <param name="bitstreamChunk">Frames returned</param>
        /// <returns>true indicates you must keep calling me</returns>
        public bool Flush(ref BitStreamChunk bitstreamChunk)
        {
            bitstreamChunk.bytesAvailable = 0;
            if (flush1state)
            {
                flush1state = Flush1(ref bitstreamChunk);
                return true;
            }
            return Flush2(ref bitstreamChunk);
        }
        bool Flush1(ref BitStreamChunk bsc)
        {

            bsc.bytesAvailable = 0;
            if (GetBitstreamIfFull(ref bsc))
                return true;

            mfxStatus sts = 0;
            int nTaskIdx = GetFreeTaskIndex(pTasks);      // Find free task
            Trace.Assert((int)mfxStatus.MFX_ERR_NOT_FOUND != nTaskIdx);


            for (;;)
            {
                // Encode a frame asychronously (returns immediately)

                fixed (mfxBitstream* b = &pTasks[nTaskIdx].mfxBS)
                fixed (mfxSyncPoint* c = &pTasks[nTaskIdx].syncp)
                    sts = UnsafeNativeMethods.MFXVideoENCODE_EncodeFrameAsync(session, null, null, b, c);

                if (mfxStatus.MFX_ERR_NONE < sts && !(pTasks[nTaskIdx].syncp.sync_ptr != null))
                {    // Repeat the call if warning and no output
                    if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                        Thread.Sleep(1);  // Wait if device is busy, then repeat the same call
                }
                else if (mfxStatus.MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp.sync_ptr != null)
                {
                    sts = mfxStatus.MFX_ERR_NONE;     // Ignore warnings if output is available
                    break;
                }
                else
                    break;
            }



            // MFX_ERR_MORE_DATA means that the input file has ended, need to go to buffering loop, exit in case of other errors
            //MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;  // no more to flush here
            sts = mfxStatus.MFX_ERR_NONE;
            //MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);            
            QuickSyncStatic.ThrowOnBadStatus(sts, "flush1.encodeFrameAsync");
            return true;  // yes, call me again, more to flush
        }

        bool Flush2(ref BitStreamChunk bsc)
        {
            if (pTasks[nFirstSyncTask].syncp.sync_ptr != null)
            {
                GetBitstreamIfAny(ref bsc);
                return true;
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }




                foreach (var item in pinningHandles)
                {
                    item.Free();
                }
                // Set large fields to null.
                disposed = true;
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~LowLevelEncoderCSharp()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }



        /// <summary>
        /// Passes bitstream to the decoder.
        /// </summary>
        /// <param name="inbuf">Bitstream buffer to pass</param>
        /// <param name="offset">Index into inbuf</param>
        /// <param name="length">Number of bytes to use from inbuf</param>
        void PutBitstream(byte[] inbuf, int offset, int length)
        {
            FastMemcpyMemmove.memmove(bitstream.Data, bitstream.Data + (int)bitstream.DataOffset, (int)bitstream.DataLength);
            bitstream.DataOffset = 0;

            int free = (int)(bitstream.MaxLength - bitstream.DataLength);
            Trace.Assert(length <= free);

            Marshal.Copy(inbuf, offset, bitstream.Data + (int)bitstream.DataLength, length);

            bitstream.DataLength += (uint)length;
        }

        void ClearBitstream()
        {
            bitstream.DataOffset = 0;
            bitstream.DataLength = 0;

        }

        public void LockFrame(ref mfxFrameSurface1 frame, ref mfxFrameData frameData)
        {
            throw new NotImplementedException();
        }

        public void UnlockFrame(ref mfxFrameSurface1 frame, ref mfxFrameData frameData)
        {
            throw new NotImplementedException();
        }

        public void LockFrame(IntPtr frame)
        {
            throw new NotImplementedException();
        }

        public void UnlockFrame(IntPtr frame)
        {
            throw new NotImplementedException();
        }
    }
}
