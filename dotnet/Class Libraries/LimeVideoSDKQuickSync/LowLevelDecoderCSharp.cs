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
    /// Turns compressed video bitstreams into uncompressed frames.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    unsafe public class LowLevelDecoderCSharp : IDisposable, ILowLevelDecoder
    {
        public mfxSession session;
        mfxBitstream bitstream;
        //public DeviceSetup deviceSetup;
      public  VideoAccelerationSupport videoAccelerationSupport;


        /// <summary>The default bitstream buffer size</summary>
        int defaultBitstreamBufferSize = 65536;

        mfxVideoParam videoParam;
        mfxFrameAllocRequest DecRequest;
        mfxFrameAllocRequest[] VPPRequest = new mfxFrameAllocRequest[2];   // non blitable

        mfxFrameSurface1[] pmfxSurfaces;  // non blitable
        mfxFrameSurface1[] pmfxSurfaces2;  // non blitable
        IntPtr bitstreamBuffer;
        IntPtr surfaceBuffers;
        IntPtr surfaceBuffers2;
        bool enableVPP;
        List<GCHandle> pinningHandles = new List<GCHandle>();
        bool disposed = false;

        mfxSession ILowLevelDecoder.session
        {
            get
            {
                return session;
            }
        }
        VideoAccelerationSupport ILowLevelDecoder.videoAccelerationSupport
        {
            get
            {
                return videoAccelerationSupport;
            }
        }





        /// <summary>
        /// Constructor
        /// </summary>
        public LowLevelDecoderCSharp(mfxVideoParam mfxDecParamsX,
            mfxVideoParam? VPPParamsX = null,
            mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO)
        {


            mfxStatus sts;
            bool enableVPP = VPPParamsX != null;


            if (VPPParamsX == null)
            {
                // Create a default VPPParamsX
                var foo = new mfxVideoParam();
                foo.AsyncDepth = 1;
                foo.IOPattern = IOPattern.MFX_IOPATTERN_OUT_SYSTEM_MEMORY | IOPattern.MFX_IOPATTERN_IN_SYSTEM_MEMORY;
                foo.vpp.In = mfxDecParamsX.mfx.FrameInfo;
                foo.vpp.Out = mfxDecParamsX.mfx.FrameInfo;
                VPPParamsX = foo;
            }


            mfxVideoParam VPPParams = VPPParamsX != null ? VPPParamsX.Value : new mfxVideoParam();
            mfxVideoParam mfxDecParams = mfxDecParamsX;

            // NOTE
            // IF I am worried about interop issues with stuff moving due to GC,
            // just pin ever single blitable here
            pinningHandles.Add(GCHandle.Alloc(pmfxSurfaces, GCHandleType.Pinned));
            pinningHandles.Add(GCHandle.Alloc(pmfxSurfaces2, GCHandleType.Pinned));
            //pinningHandles.Add(GCHandle.Alloc(struct1, GCHandleType.Pinned));
            //pinningHandles.Add(GCHandle.Alloc(struct1, GCHandleType.Pinned));


            this.videoParam = mfxDecParams;
            this.enableVPP = enableVPP;





            session = new mfxSession();
            var ver = new mfxVersion() { Major = 1, Minor = 3 };
            fixed (mfxSession* s = &session)
                sts = UnsafeNativeMethods.MFXInit(impl, &ver, s);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXInit");







            bool decVideoMemOut = (mfxDecParams.IOPattern & IOPattern.MFX_IOPATTERN_OUT_VIDEO_MEMORY) != 0;
            bool vppVideoMemIn = (VPPParams.IOPattern & IOPattern.MFX_IOPATTERN_IN_VIDEO_MEMORY) != 0;
            bool vppVideoMemOut = (VPPParams.IOPattern & IOPattern.MFX_IOPATTERN_OUT_VIDEO_MEMORY) != 0;

            Trace.Assert(!enableVPP || decVideoMemOut == vppVideoMemIn, "When the VPP is enabled, the memory type from DEC into VPP must be of same type");



            if (vppVideoMemIn || vppVideoMemOut)
            {
                //if you want to use video memory, you need to have a way to allocate the Direct3D or Vaapi frames
                videoAccelerationSupport = new VideoAccelerationSupport(session);
            }

            fixed (mfxFrameAllocRequest* p = &DecRequest)
                sts = UnsafeNativeMethods.MFXVideoDECODE_QueryIOSurf(session, &mfxDecParams, p);
            if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION)
                sts = 0;
            QuickSyncStatic.ThrowOnBadStatus(sts, "DECODE_QueryIOSurf");


            if (enableVPP)
            {
                fixed (mfxFrameAllocRequest* p = &VPPRequest[0])
                    sts = UnsafeNativeMethods.MFXVideoVPP_QueryIOSurf(session, &VPPParams, p);
                if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION)
                    sts = 0;
                QuickSyncStatic.ThrowOnBadStatus(sts, "VPP_QueryIOSurf");


                VPPRequest[1].Type |= FrameMemoryType.WILL_READ;
            }






            //mfxU16 nSurfNumDecVPP = DecRequest.NumFrameSuggested + VPPRequest[0].NumFrameSuggested;
            //mfxU16 nSurfNumVPPOut = VPPRequest[1].NumFrameSuggested;

            int nSurfNumVPPOut = 0;

            var numSurfaces = DecRequest.NumFrameSuggested + VPPRequest[0].NumFrameSuggested + VPPParams.AsyncDepth;
            if (enableVPP)
                nSurfNumVPPOut = 0 + VPPRequest[1].NumFrameSuggested + VPPParams.AsyncDepth;



            bitstreamBuffer = Marshal.AllocHGlobal(defaultBitstreamBufferSize);
            bitstream.Data = bitstreamBuffer;
            bitstream.DataLength = 0;
            bitstream.MaxLength = (uint)defaultBitstreamBufferSize;
            bitstream.DataOffset = 0;


            //mfxFrameAllocRequest DecRequest;
            //sts = UnsafeNativeMethods.MFXVideoDECODE_QueryIOSurf(session, &mfxDecParams, &DecRequest);
            //if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION) sts = 0;
            //Trace.Assert(sts == mfxStatus.MFX_ERR_NONE);



            //allocate decoder frames via directx       
            mfxFrameAllocResponse DecResponse = new mfxFrameAllocResponse();
            if (decVideoMemOut)
            {
                DecRequest.NumFrameMin = DecRequest.NumFrameSuggested = (ushort)numSurfaces;
                fixed (mfxFrameAllocRequest* p = &DecRequest)
                    videoAccelerationSupport.AllocFrames(p, &DecResponse);

            }


            //allocate vpp frames via directx      
            mfxFrameAllocResponse EncResponse = new mfxFrameAllocResponse();
            if (vppVideoMemOut)
            {
                VPPRequest[1].NumFrameMin = VPPRequest[1].NumFrameSuggested = (ushort)nSurfNumVPPOut;
                fixed (mfxFrameAllocRequest* p = &VPPRequest[1])
                    videoAccelerationSupport.AllocFrames(p, &EncResponse);
            }





            // Allocate surfaces for decoder
            // - Width and height of buffer must be aligned, a multiple of 32
            // - Frame surface array keeps pointers all surface planes and general frame info
            UInt16 width = (UInt16)QuickSyncStatic.ALIGN32(DecRequest.Info.Width);
            UInt16 height = (UInt16)QuickSyncStatic.ALIGN32(DecRequest.Info.Height);
            int bitsPerPixel = VideoUtility.GetBitsPerPixel(mfxDecParams.mfx.FrameInfo.FourCC);
            int surfaceSize = width * height * bitsPerPixel / 8;
            //byte[] surfaceBuffers = new byte[surfaceSize * numSurfaces]; //XXX

            if (!decVideoMemOut)
                surfaceBuffers = Marshal.AllocHGlobal(surfaceSize * numSurfaces);




            //         // Allocate surface headers (mfxFrameSurface1) for decoder
            pmfxSurfaces = new mfxFrameSurface1[numSurfaces];
            pinningHandles.Add(GCHandle.Alloc(pmfxSurfaces, GCHandleType.Pinned));

            //MSDK_CHECK_POINTER(pmfxSurfaces, MFX_ERR_MEMORY_ALLOC);
            for (int i = 0; i < numSurfaces; i++)
            {
                pmfxSurfaces[i] = new mfxFrameSurface1();
                pmfxSurfaces[i].Info = mfxDecParams.mfx.FrameInfo;
                if (!decVideoMemOut)
                {
                    switch (mfxDecParams.mfx.FrameInfo.FourCC)
                    {
                        case FourCC.NV12:
                            pmfxSurfaces[i].Data.Y_ptr = (byte*)surfaceBuffers + i * surfaceSize;
                            pmfxSurfaces[i].Data.U_ptr = pmfxSurfaces[i].Data.Y_ptr + width * height;
                            pmfxSurfaces[i].Data.V_ptr = pmfxSurfaces[i].Data.U_ptr + 1;
                            pmfxSurfaces[i].Data.Pitch = width;
                            break;
                        case FourCC.YUY2:
                            pmfxSurfaces[i].Data.Y_ptr = (byte*)surfaceBuffers + i * surfaceSize;
                            pmfxSurfaces[i].Data.U_ptr = pmfxSurfaces[i].Data.Y_ptr + 1;
                            pmfxSurfaces[i].Data.V_ptr = pmfxSurfaces[i].Data.U_ptr + 3;
                            pmfxSurfaces[i].Data.Pitch = (ushort)(width * 2);
                            break;
                        default:  //find sysmem_allocator.cpp for more help
                            throw new NotImplementedException();
                    }

                }
                else
                {
                    pmfxSurfaces[i].Data.MemId = DecResponse.mids_ptr[i];   // MID (memory id) represent one D3D NV12 surface 
                }
            }







            if (enableVPP)
            {

                UInt16 width2 = (UInt16)QuickSyncStatic.ALIGN32(VPPRequest[1].Info.CropW);
                UInt16 height2 = (UInt16)QuickSyncStatic.ALIGN32(VPPRequest[1].Info.CropH);
                int bitsPerPixel2 = VideoUtility.GetBitsPerPixel(VPPParams.vpp.Out.FourCC);        // NV12 format is a 12 bits per pixel format
                int surfaceSize2 = width2 * height2 * bitsPerPixel2 / 8;
                int pitch2 = width2 * bitsPerPixel2 / 8;

                if (!vppVideoMemOut)
                    surfaceBuffers2 = Marshal.AllocHGlobal(surfaceSize2 * nSurfNumVPPOut);

                pmfxSurfaces2 = new mfxFrameSurface1[nSurfNumVPPOut];
                pinningHandles.Add(GCHandle.Alloc(pmfxSurfaces2, GCHandleType.Pinned));
                //MSDK_CHECK_POINTER(pmfxSurfaces, MFX_ERR_MEMORY_ALLOC);
                for (int i = 0; i < nSurfNumVPPOut; i++)
                {
                    pmfxSurfaces2[i] = new mfxFrameSurface1();
                    pmfxSurfaces2[i].Info = VPPParams.vpp.Out;

                    if (!vppVideoMemOut)
                    {

                        pmfxSurfaces2[i].Data.Pitch = (ushort)pitch2;
                        switch (VPPParams.vpp.Out.FourCC)
                        {
                            case FourCC.NV12:
                                pmfxSurfaces2[i].Data.Y_ptr = (byte*)surfaceBuffers2 + i * surfaceSize2;
                                pmfxSurfaces2[i].Data.U_ptr = pmfxSurfaces2[i].Data.Y_ptr + width * height;
                                pmfxSurfaces2[i].Data.V_ptr = pmfxSurfaces2[i].Data.U_ptr + 1;
                                break;
                            case FourCC.RGB4:
                                pmfxSurfaces2[i].Data.B_ptr = (byte*)surfaceBuffers2 + i * surfaceSize2;
                                pmfxSurfaces2[i].Data.G_ptr = (byte*)surfaceBuffers2 + i * surfaceSize2 + 1;
                                pmfxSurfaces2[i].Data.R_ptr = (byte*)surfaceBuffers2 + i * surfaceSize2 + 2;
                                // pmfxSurfaces2[i].Data.A_ptr = (byte*)surfaceBuffers2 + i * surfaceSize2+3;
                                //   pmfxSurfaces2[i].Data. = pmfxSurfaces2[i].Data.Y_ptr + width * height;
                                //  pmfxSurfaces2[i].Data.V_ptr = pmfxSurfaces2[i].Data.U_ptr + 1;
                                break;
                            default:
                                break;
                        }


                    }
                    else
                    {
                        pmfxSurfaces2[i].Data.MemId = EncResponse.mids_ptr[i];   // MID (memory id) represent one D3D NV12 surface 
                    }
                }
            }






            sts = UnsafeNativeMethods.MFXVideoDECODE_Init(session, &mfxDecParams);
            if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION) sts = 0;
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXVideoDECODE_Init");


            if (enableVPP)
            {
                sts = UnsafeNativeMethods.MFXVideoVPP_Init(session, &VPPParams);
                if (sts == mfxStatus.MFX_WRN_PARTIAL_ACCELERATION) sts = 0;
                QuickSyncStatic.ThrowOnBadStatus(sts, "MFXVideoVPP_Init");
            }

        }









        static int GetFreeSurfaceIndex(mfxFrameSurface1[] pSurfacesPool)
        {

            for (int i = 0; i < pSurfacesPool.Length; i++)
                if (0 == pSurfacesPool[i].Data.Locked)
                    return i;
            return (int)mfxStatus.MFX_ERR_NOT_FOUND;
        }




        /// <summary>See if a decoded frame is available</summary>
        /// <param name="frame">A frame</param>
        /// <returns>True when more data is necessary to decode a frame</returns>
        public unsafe bool DecodeFrame(out mfxFrameSurface1? frame)
        {

            mfxFrameSurface1* p;
            bool moreDataNeeded = DecodeFrame(&p);

            if (p != (mfxFrameSurface1*)0)
            {
                frame = *p;
            }
            else
            {
                frame = null;
            }

            return moreDataNeeded;
        }

        /// <summary>This is the 1st stage flush after all bitstream has been passed in as input.</summary>
        /// <param name="frame">The frame.</param>
        /// <returns>True when more data is necessary to decode a frame</returns>
        public unsafe bool Flush1(out mfxFrameSurface1? frame)
        {

            mfxFrameSurface1* p;
            bool moreDataNeeded = Flush1(&p);

            if (p != (mfxFrameSurface1*)0)
            {
                frame = *p;
            }
            else
            {
                frame = null;
            }

            return moreDataNeeded;
        }

        /// <summary>This is the 2nd stage flush after all bitstream has been passed in as input.</summary>
        /// <param name="frame">The frame.</param>
        /// <returns>True when more data is necessary to decode a frame</returns>
        public unsafe bool Flush2(out mfxFrameSurface1? frame)
        {

            mfxFrameSurface1* p;
            bool moreDataNeeded = Flush2(&p);

            if (p != (mfxFrameSurface1*)0)
            {
                frame = *p;
            }
            else
            {
                frame = null;
            }

            return moreDataNeeded;
        }

        /// <summary>
        /// Place a decoded frame in 'frame' if one is available.
        /// </summary>
        /// <param name="frame">Where to pyt frame.</param>
        /// <returns>
        /// true:keep calling me
        /// false:this phase done
        /// </returns>
        /// 
        bool DecodeFrame(mfxFrameSurface1** frame)
        {
            mfxStatus sts = 0;


            *frame = (mfxFrameSurface1*)0;

          

            mfxSyncPoint syncpD;
            mfxSyncPoint syncpV;
            mfxFrameSurface1* pmfxOutSurface = (mfxFrameSurface1*)0;
            int nIndex = 0;
            int nIndex2 = 0;


            //
            // Stage 1: Main decoding loop
            //
            if (mfxStatus.MFX_ERR_NONE <= sts || mfxStatus.MFX_ERR_MORE_DATA == sts || mfxStatus.MFX_ERR_MORE_SURFACE == sts)
            {
                if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                    Thread.Sleep(1);  // Wait if device is busy, then repeat the same call to DecodeFrameAsync

                //if (MFX_ERR_MORE_DATA == sts) {
                //	sts = ReadBitStreamData(&config.mfxBS, fSource);       // Read more data into input bit stream
                //	MSDK_BREAK_ON_ERROR(sts);
                //}
                foo:

                if (mfxStatus.MFX_ERR_MORE_SURFACE == sts || mfxStatus.MFX_ERR_NONE == sts)
                {
                    nIndex = GetFreeSurfaceIndex(pmfxSurfaces);        // Find free frame surface
                    QuickSyncStatic.ThrowOnBadStatus((mfxStatus)nIndex, "cannot find free surface");
                }




                // Decode a frame asychronously (returns immediately)
                //  - If input bitstream contains multiple frames DecodeFrameAsync will start decoding multiple frames, and remove them from bitstream
                // it might have been better to use marshal.XXX to pin this?
                fixed (mfxFrameSurface1* p1 = &pmfxSurfaces[nIndex])
                fixed (mfxBitstream* p2 = &bitstream)
                {
                    sts = UnsafeNativeMethods.MFXVideoDECODE_DecodeFrameAsync(session, p2, p1, &pmfxOutSurface, &syncpD);
                    if (!enableVPP && mfxStatus.MFX_ERR_NONE == sts && syncpD.sync != IntPtr.Zero)
                    {
                        sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, syncpD, 60000);     // Synchronize. Wait until decoded frame is ready
                        *frame = pmfxOutSurface;
                    }
                }




                // Decode a frame asychronously (returns immediately)
                //sts = mfxDEC->DecodeFrameAsync(&config.mfxBS, pmfxSurfaces[nIndex], &pmfxOutSurface, &syncpD);

                //if (sts == MFX_WRN_VIDEO_PARAM_CHANGED)
                //	;

                // I had a problem where I was getting a lot of these, I suspect
                // when you get this return code, and you sync anyway, it forces more of them
                // be sure to test this statement under vmware in software mode
                // it seems this uniquely happens there that it uses this to ask for more internal surfaces.
                if (sts == mfxStatus.MFX_ERR_MORE_SURFACE)
                    goto foo;



                // Ignore warnings if output is available,
                // if no output and no action required just repeat the DecodeFrameAsync call
                if (mfxStatus.MFX_ERR_NONE < sts && syncpD.sync != IntPtr.Zero)
                    sts = mfxStatus.MFX_ERR_NONE;






            }
            if (sts == mfxStatus.MFX_ERR_MORE_SURFACE) // decoder needs to be called again, it is eating memory.SWmode
                return true;
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;
            if (sts < 0)
                throw new QuickSyncException("DecodeFrame fail", sts);



            if (enableVPP && sts == mfxStatus.MFX_ERR_NONE)
            {
                fixed (mfxFrameSurface1* p1 = &pmfxSurfaces2[nIndex2])
                {
                    nIndex2 = GetFreeSurfaceIndex(pmfxSurfaces2);   // Find free frame surface
                    QuickSyncStatic.ThrowOnBadStatus((mfxStatus)nIndex2, "cannot find free surface");



                    tryagain:

                    // Process a frame asychronously (returns immediately)

                    sts = UnsafeNativeMethods.MFXVideoVPP_RunFrameVPPAsync(session, pmfxOutSurface, p1, null, &syncpV);

                    //if (sts == MFX_WRN_VIDEO_PARAM_CHANGED)
                    //	;

                    if (mfxStatus.MFX_ERR_NONE < sts && syncpV.sync_ptr == null)
                    {    // repeat the call if warning and no output
                        if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                        {
                            Thread.Sleep(1);  // wait if device is busy
                            goto tryagain;
                        }
                    }
                    else if (mfxStatus.MFX_ERR_NONE < sts && syncpV.sync_ptr != null)
                    {
                        sts = mfxStatus.MFX_ERR_NONE;     // ignore warnings if output is available
                    }
                    else if (mfxStatus.MFX_ERR_MORE_DATA == sts) // VPP needs more data, let decoder decode another frame as input
                    {
                        //continue;
                        return false;
                    }
                    else if (mfxStatus.MFX_ERR_MORE_SURFACE == sts)
                    {
                        // Not relevant for the illustrated workload! Therefore not handled.
                        // Relevant for cases when VPP produces more frames at output than consumes at input. E.g. framerate conversion 30 fps -> 60 fps
                        //break;
                        return true;
                    }
                    else if (sts < 0)
                        throw new QuickSyncException("RunFrameVPPAsync fail", sts);
                    // MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts); //MSDK_BREAK_ON_ERROR(sts);
                    else if (mfxStatus.MFX_ERR_NONE == sts && syncpV.sync != IntPtr.Zero)
                    {
                        sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, syncpV, 60000);     // Synchronize. Wait until decoded frame is ready
                        *frame = p1;
                        return true;
                    }
                }

            }

            return true;
        }


        /// <summary>
        /// Reset the decoder, useful to discard bitstream, and start over
        /// 11 1 15 Cam
        /// SPS/PPS FOR AVC WILL BE REQUIRED AFTER RESET!
        /// It appears a RESET to the IMSDK causes the decoder to enter seek mode for PPS/SPS!
        /// You won't get any frames back until you give the docoder SPS/PPS!
        /// </summary>
        public void Reset(mfxVideoParam p)
        {
            bitstream.DataLength = 0;
            bitstream.DataOffset = 0;
            mfxStatus sts = UnsafeNativeMethods.MFXVideoDECODE_Reset(session, &p);
            if (sts < 0)
                throw new QuickSyncException("Reset fail", sts);
        }




#if false

        public bool DecodeFrame(mfxFrameSurface1 frame)
        {
            Trace.Assert(stage == Stage.MemoryAllocated);




            //Console.WriteLine(999);
            mfxStatus sts =UnsafeNativeMethods.MFXVideoDECODE_DecodeFrameAsync(session,&mfxBS,)
            //Console.WriteLine(sts) ;
            if (sts == mfxStatus.MFX_ERR_MORE_SURFACE) // decoder needs to be called again, it is eating memory.SWmode
                return false;
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
            {
                //  stage++;
                return true;
            }
            if (sts < 0)
                throw new LimeVideoSDKLowLevelException("DecodeFrame fail", sts);
            if (sts == mfxStatus.MFX_WRN_VIDEO_PARAM_CHANGED)
            {
                frame.WarningVideoParamChanged = true;
            }
            else
            {
                frame.WarningVideoParamChanged = false;
                if (sts > 0)
                {
                    Console.WriteLine("warn: " + sts.ToString());
                }
            }



            frame.ptr = foo;

            return false;
        } 

        /// <summary>
        /// The first stage of flushing the internal frame buffers.
        /// </summary>
        /// <param name="frame">The frame into which valid frames will be placed.</param>
        /// <returns>
        /// True indicates more data is needed.
        /// False indicates buffer is sufficient.    
        /// </returns>
        public bool Flush1(mfxFrameSurface1 frame)
        {
            if (stage == Stage.MemoryAllocated)
            {
                stage++;
            }
            Trace.Assert(stage == Stage.DecodingDone);

            void* foo = (void*)0;
            frame.ptr = (void*)0;

            mfxStatus sts = UnsafeNativeMethods.CLowLevelDecoder_Flush1(handle, &foo);
            if (sts == mfxStatus.MFX_ERR_MORE_SURFACE) // decoder needs to be called again, it is eating memory.SWmode
                return false;
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
            {
                stage++;
                return true;
            }
            if (sts != 0)
                throw new LimeVideoSDKLowLevelException("Flush1 fail", sts);

            frame.ptr = foo;

            return false;

        }
#endif
        /// <summary>
        /// 
        /// </summary>
        /// <param name="frame"></param>
        /// <returns>
        /// true:keep calling me
        /// false:this phase done
        /// </returns>
        bool Flush1(mfxFrameSurface1** frame)
        {
            mfxStatus sts = 0;
            *frame = (mfxFrameSurface1*)0;
            mfxSyncPoint syncpD, syncpV;
            mfxFrameSurface1* pmfxOutSurface = (mfxFrameSurface1*)0;
            int nIndex = 0;
            int nIndex2 = 0;


            //
            // Stage 2: Retrieve the buffered decoded frames
            //
            //while (MFX_ERR_NONE <= sts || MFX_ERR_MORE_SURFACE == sts) {
            if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                Thread.Sleep(1);  // Wait if device is busy, then repeat the same call to DecodeFrameAsync

            nIndex = GetFreeSurfaceIndex(pmfxSurfaces);        // Find free frame surface
            QuickSyncStatic.ThrowOnBadStatus((mfxStatus)nIndex, "cannot find free surface");

            // Decode a frame asychronously (returns immediately)
            fixed (mfxFrameSurface1* p1 = &pmfxSurfaces[nIndex])
                sts = UnsafeNativeMethods.MFXVideoDECODE_DecodeFrameAsync(session, null, p1, &pmfxOutSurface, &syncpD);

            // Ignore warnings if output is available,
            // if no output and no action required just repeat the DecodeFrameAsync call
            if (mfxStatus.MFX_ERR_NONE < sts && syncpD.sync_ptr != null)
                sts = mfxStatus.MFX_ERR_NONE;


            if (!enableVPP)
            {
                if (mfxStatus.MFX_ERR_NONE == sts)
                {
                    sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, syncpD, 60000);     // Synchronize. Wait until decoded frame is ready
                    *frame = pmfxOutSurface;
                }
            }
            if (sts == mfxStatus.MFX_ERR_MORE_SURFACE) // decoder needs to be called again, it is eating memory.SWmode
                return true;
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;
            if (sts < 0)
                throw new QuickSyncException("Flush1 fail", sts);



            if (enableVPP && sts == mfxStatus.MFX_ERR_NONE)
            {
                fixed (mfxFrameSurface1* p1 = &pmfxSurfaces2[nIndex2])
                {
                    nIndex2 = GetFreeSurfaceIndex(pmfxSurfaces2);   // Find free frame surface
                    QuickSyncStatic.ThrowOnBadStatus((mfxStatus)nIndex2, "cannot find free surface");


                    for (;;)
                    {
                        // Process a frame asychronously (returns immediately)

                        sts = UnsafeNativeMethods.MFXVideoVPP_RunFrameVPPAsync(session, pmfxOutSurface, p1, null, &syncpV);

                        //if (sts == MFX_WRN_VIDEO_PARAM_CHANGED)
                        //	;

                        if (mfxStatus.MFX_ERR_NONE < sts && syncpV.sync_ptr == null)
                        {    // repeat the call if warning and no output
                            if (mfxStatus.MFX_WRN_DEVICE_BUSY == sts)
                                Thread.Sleep(1);  // wait if device is busy
                        }
                        else if (mfxStatus.MFX_ERR_NONE < sts && syncpV.sync_ptr != null)
                        {
                            sts = mfxStatus.MFX_ERR_NONE;     // ignore warnings if output is available
                            break;
                        }
                        else
                            break;  // not a warning


                        // VPP needs more data, let decoder decode another frame as input
                        if (mfxStatus.MFX_ERR_MORE_DATA == sts)
                        {
                            //continue;
                            return false;
                        }
                        else if (mfxStatus.MFX_ERR_MORE_SURFACE == sts)
                        {
                            // Not relevant for the illustrated workload! Therefore not handled.
                            // Relevant for cases when VPP produces more frames at output than consumes at input. E.g. framerate conversion 30 fps -> 60 fps
                            //break;
                            return true;
                        }
                        else
                            if (sts < 0)
                            throw new QuickSyncException("RunFrameVPPAsync fail", sts);
                        // MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts); //MSDK_BREAK_ON_ERROR(sts);
                    }


                    if (mfxStatus.MFX_ERR_NONE == sts && syncpV.sync != IntPtr.Zero)
                    {
                        sts = UnsafeNativeMethods.MFXVideoCORE_SyncOperation(session, syncpV, 60000);     // Synchronize. Wait until decoded frame is ready
                        *frame = p1;
                    }
                }
            }
            return true;

            //}

            // MFX_ERR_MORE_DATA means that decoder is done with buffered frames, need to go to VPP buffering loop, exit in case of other errors
            //MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
            //MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);




        }



        /// <returns>
        /// true:keep calling me
        /// false:this phase done
        /// </returns>
        bool Flush2(mfxFrameSurface1** frame)
        {

            *frame = (mfxFrameSurface1*)0;
            // mfxSyncPoint syncpD;
            mfxFrameSurface1* pmfxOutSurface = (mfxFrameSurface1*)0;


            // bool UseVPP = false;
            //   if (UseVPP)
            return false;

#if false
            //
            // Stage 3: Retrieve the buffered VPP frames
            //
            //while (MFX_ERR_NONE <= sts) {
            int nIndex2 = GetFreeSurfaceIndex(pmfxSurfaces2, nSurfNumVPPOut);   // Find free frame surface
              QuickSyncStatic.ThrowOnBadStatus((mfxStatus)nIndex2, "cannot find free surface");

            // Process a frame asychronously (returns immediately)
            sts = mfxVPP->RunFrameVPPAsync(NULL, pmfxSurfaces2[nIndex2], NULL, &syncpV);
            if (MFX_ERR_MORE_DATA == sts)
                return sts; // continue;
            MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_SURFACE);
            MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts); //MSDK_BREAK_ON_ERROR(sts);

            sts = session.SyncOperation(syncpV, 60000);     // Synchronize. Wait until frame processing is ready
            MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

            ++nFrame;
            if (bEnableOutput)
            {
                //sts = WriteRawFrame(pmfxSurfaces2[nIndex2], fSink);
                //MSDK_BREAK_ON_ERROR(sts);
                *frame = pmfxSurfaces2[nIndex2];
                return sts;

                //printf("Frame number: %d\r", nFrame);
            }
            //}

            // MFX_ERR_MORE_DATA indicates that all buffers has been fetched, exit in case of other errors
            //MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
            //MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

#endif


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

                if (surfaceBuffers != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(surfaceBuffers);
                    surfaceBuffers = IntPtr.Zero;
                }
                if (surfaceBuffers2 != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(surfaceBuffers2);
                    surfaceBuffers2 = IntPtr.Zero;
                }
                if (bitstreamBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(bitstreamBuffer);
                    bitstreamBuffer = IntPtr.Zero;
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
        ~LowLevelDecoderCSharp()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }




        /// <summary>
        /// Puts the bitstream.
        /// </summary>
        /// <param name="inbuf">The inbuf.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <exception cref="QuickSyncException">insufficient space in buffer</exception>
        public void PutBitstream(byte[] inbuf, int offset, int length)
        {
            FastMemcpyMemmove.memmove(bitstream.Data, bitstream.Data + (int)bitstream.DataOffset, (int)bitstream.DataLength);
            bitstream.DataOffset = 0;

            int free = (int)(bitstream.MaxLength - bitstream.DataLength);
            //Trace.Assert(length <= free);
            if (free < length)
                throw new QuickSyncException("insufficient space in buffer");

            Marshal.Copy(inbuf, offset, bitstream.Data + (int)bitstream.DataLength, length);

            bitstream.DataLength += (uint)length;
        }



        public int GetInternalBitstreamBufferFree()
        {
            return (int)(bitstream.MaxLength - bitstream.DataLength);
        }

        void ILowLevelDecoder.ClearBitstream()
        {
            bitstream.DataOffset = 0;
            bitstream.DataLength = 0;
        }















        public void LockFrame(IntPtr memId)
        {
            if (memId == IntPtr.Zero)   //This probably means you are trying to lock a system memory frame
                return;

            videoAccelerationSupport.LockFrame(memId);
        }

        public void UnlockFrame(IntPtr memId)
        {
            if (memId == IntPtr.Zero)   //This probably means you are trying to lock a system memory frame
                return;

            videoAccelerationSupport.UnlockFrame(memId);
        }
    }
}
