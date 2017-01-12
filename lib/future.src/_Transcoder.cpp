/*****************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or
nondisclosure agreement with Intel Corporation and may not be copied
or disclosed except in accordance with the terms of that agreement.
Copyright(c) 2005-2014 Intel Corporation. All Rights Reserved.

*****************************************************************************/
// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// Modifications by Cameron Elliott are distributed under open source BSD License terms:
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#include <thread>
#include <mutex>

//#include "common_utils.h"
//#include "cmd_options.h"

#include "_VideoAccelerationSupport.h"
#include "_Helpers.h"


#undef NDEBUG
#include <assert.h>

#define EXTERN_DLL_EXPORT extern "C" __declspec(dllexport)





#pragma pack(push,8)  
struct TranscoderShared {
public:
	INT32 safety = sizeof(TranscoderShared);
	mfxSession session = 0;
	//mfxBitstream mfxBS;
	mfxVideoParam mfxDecParams;
	mfxVideoParam VPPParams;
	mfxVideoParam mfxEncParams;
	mfxBitstream bitsin;
	mfxBitstream bitsout;


	char *warningFuncName[100];
	INT32 warningMfxStatus[100];
	UINT32 warningCount = 0;


	TranscoderShared() : session(), mfxDecParams(), VPPParams(), mfxEncParams(), bitsin(), bitsout()
	{
		//memset(&mfxBS, 0, sizeof(mfxBS));
	}
};
#pragma pack(pop)

class CTranscoder : public TranscoderShared
{
	bool bEnableOutput = false;
	
	mfxU16 nSurfNumDecVPP = 0;
	mfxU16 nSurfNumVPPEnc = 0;
	mfxFrameSurface1** pSurfaces = 0;
	mfxFrameSurface1** pSurfaces2 = 0;
	mfxU16 taskPoolSize = 0;
	Task* pTasks = 0;
	int nFirstSyncTask = 0;

	mfxSession session;
	bool rendering = false;
	MemType memType = SYSTEM_MEMORY;
	VideoAccelerationSupport vas;

public:
	using  TranscoderShared::operator=;
	CTranscoder() :
		session(), vas(), TranscoderShared()
	{
		assert(dynamic_cast<TranscoderShared*>(this) == this);
		this->safety = sizeof(TranscoderShared);
	}

	mfxStatus init(mfxSession s, mfxVideoParam _mfxDecParams, mfxVideoParam _VPPParams, mfxVideoParam _mfxEncParams)
	{
		mfxStatus sts = MFX_ERR_NONE;

		this->mfxDecParams = _mfxDecParams;
		this->VPPParams = _VPPParams;
		this->mfxEncParams = _mfxEncParams;
		this->session = s;
#if 0
		//mfxStatus Init(mfxSession s, bool rendering, MemType memType, int linuxLibvaBackend = 0, int linuxMonitorType = 0, bool linuxWaylandPerfMode = false);
		sts = vas.Init(session, this->rendering, memType);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
		sts = vas.CreateAllocator();
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
#endif


		// Query number required surfaces for decoder
		mfxFrameAllocRequest DecRequest;
		memset(&DecRequest, 0, sizeof(DecRequest));
		sts = MFXVideoDECODE_QueryIOSurf(session, &mfxDecParams, &DecRequest);
		MSDK_IGNORE_MFX_STS(sts, MFX_WRN_PARTIAL_ACCELERATION);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		// Query number required surfaces for encoder
		mfxFrameAllocRequest EncRequest;
		memset(&EncRequest, 0, sizeof(EncRequest));
		sts = MFXVideoENCODE_QueryIOSurf(session, &mfxEncParams, &EncRequest);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		// Query number of required surfaces for VPP
		mfxFrameAllocRequest VPPRequest[2];     // [0] - in, [1] - out
		memset(&VPPRequest, 0, sizeof(mfxFrameAllocRequest) * 2);
		sts = MFXVideoVPP_QueryIOSurf(session, &VPPParams, VPPRequest);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		// Determine the required number of surfaces for decoder output (VPP input) and for VPP output (encoder input)
		nSurfNumDecVPP = DecRequest.NumFrameSuggested + VPPRequest[0].NumFrameSuggested + VPPParams.AsyncDepth;
		nSurfNumVPPEnc = EncRequest.NumFrameSuggested + VPPRequest[1].NumFrameSuggested + VPPParams.AsyncDepth;

		// Initialize shared surfaces for decoder, VPP and encode
		// - Note that no buffer memory is allocated, for opaque memory this is handled by Media SDK internally
		// - Frame surface array keeps reference to all surfaces
		// - Opaque memory is configured with the mfxExtOpaqueSurfaceAlloc extended buffers
		pSurfaces = new mfxFrameSurface1 *[nSurfNumDecVPP];
		MSDK_CHECK_POINTER(pSurfaces, MFX_ERR_MEMORY_ALLOC);
		for (int i = 0; i < nSurfNumDecVPP; i++) {
			pSurfaces[i] = new mfxFrameSurface1;
			MSDK_CHECK_POINTER(pSurfaces[i], MFX_ERR_MEMORY_ALLOC);
			memset(pSurfaces[i], 0, sizeof(mfxFrameSurface1));
			memcpy(&(pSurfaces[i]->Info), &(DecRequest.Info), sizeof(mfxFrameInfo));
		}

		pSurfaces2 = new mfxFrameSurface1 *[nSurfNumVPPEnc];
		MSDK_CHECK_POINTER(pSurfaces2, MFX_ERR_MEMORY_ALLOC);
		for (int i = 0; i < nSurfNumVPPEnc; i++) {
			pSurfaces2[i] = new mfxFrameSurface1;
			MSDK_CHECK_POINTER(pSurfaces2[i], MFX_ERR_MEMORY_ALLOC);
			memset(pSurfaces2[i], 0, sizeof(mfxFrameSurface1));
			memcpy(&(pSurfaces2[i]->Info), &(EncRequest.Info), sizeof(mfxFrameInfo));
		}

		mfxExtOpaqueSurfaceAlloc extOpaqueAllocDec;
		memset(&extOpaqueAllocDec, 0, sizeof(extOpaqueAllocDec));
		extOpaqueAllocDec.Header.BufferId = MFX_EXTBUFF_OPAQUE_SURFACE_ALLOCATION;
		extOpaqueAllocDec.Header.BufferSz = sizeof(mfxExtOpaqueSurfaceAlloc);
		mfxExtBuffer* pExtParamsDec = (mfxExtBuffer*)& extOpaqueAllocDec;

		mfxExtOpaqueSurfaceAlloc extOpaqueAllocVPP;
		memset(&extOpaqueAllocVPP, 0, sizeof(extOpaqueAllocVPP));
		extOpaqueAllocVPP.Header.BufferId = MFX_EXTBUFF_OPAQUE_SURFACE_ALLOCATION;
		extOpaqueAllocVPP.Header.BufferSz = sizeof(mfxExtOpaqueSurfaceAlloc);
		mfxExtBuffer* pExtParamsVPP = (mfxExtBuffer*)& extOpaqueAllocVPP;

		mfxExtOpaqueSurfaceAlloc extOpaqueAllocEnc;
		memset(&extOpaqueAllocEnc, 0, sizeof(extOpaqueAllocEnc));
		extOpaqueAllocEnc.Header.BufferId = MFX_EXTBUFF_OPAQUE_SURFACE_ALLOCATION;
		extOpaqueAllocEnc.Header.BufferSz = sizeof(mfxExtOpaqueSurfaceAlloc);
		mfxExtBuffer* pExtParamsENC = (mfxExtBuffer*)& extOpaqueAllocEnc;

		extOpaqueAllocDec.Out.Surfaces = pSurfaces;
		extOpaqueAllocDec.Out.NumSurface = nSurfNumDecVPP;
		extOpaqueAllocDec.Out.Type = DecRequest.Type;

		memcpy(&extOpaqueAllocVPP.In, &extOpaqueAllocDec.Out, sizeof(extOpaqueAllocDec.Out));
		extOpaqueAllocVPP.Out.Surfaces = pSurfaces2;
		extOpaqueAllocVPP.Out.NumSurface = nSurfNumVPPEnc;
		extOpaqueAllocVPP.Out.Type = EncRequest.Type;

		memcpy(&extOpaqueAllocEnc.In, &extOpaqueAllocVPP.Out, sizeof(extOpaqueAllocVPP.Out));

		mfxDecParams.ExtParam = &pExtParamsDec;
		mfxDecParams.NumExtParam = 1;
		VPPParams.ExtParam = &pExtParamsVPP;
		VPPParams.NumExtParam = 1;
		mfxEncParams.ExtParam = &pExtParamsENC;
		mfxEncParams.NumExtParam = 1;

		// Initialize the Media SDK decoder
		sts = MFXVideoDECODE_Init(session, &mfxDecParams);
		MSDK_IGNORE_MFX_STS(sts, MFX_WRN_PARTIAL_ACCELERATION);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		// Initialize the Media SDK encoder
		sts = MFXVideoENCODE_Init(session, &mfxEncParams);
		MSDK_IGNORE_MFX_STS(sts, MFX_WRN_PARTIAL_ACCELERATION);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		// Initialize Media SDK VPP
		sts = MFXVideoVPP_Init(session, &VPPParams);
		MSDK_IGNORE_MFX_STS(sts, MFX_WRN_PARTIAL_ACCELERATION);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		// Retrieve video parameters selected by encoder.
		// - BufferSizeInKB parameter is required to set bit stream buffer size
		mfxVideoParam par;
		memset(&par, 0, sizeof(par));
		sts = MFXVideoENCODE_GetVideoParam(session, &par);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		// Create task pool to improve asynchronous performance (greater GPU utilization)
		taskPoolSize = mfxEncParams.AsyncDepth;  // number of tasks that can be submitted, before synchronizing is required
		pTasks = new Task[taskPoolSize];
		memset(pTasks, 0, sizeof(Task) * taskPoolSize);
		for (int i = 0; i < taskPoolSize; i++) {
			// Prepare Media SDK bit stream buffer
			pTasks[i].mfxBS.MaxLength = par.mfx.BufferSizeInKB * 1000;
			pTasks[i].mfxBS.Data = new mfxU8[pTasks[i].mfxBS.MaxLength];
			MSDK_CHECK_POINTER(pTasks[i].mfxBS.Data, MFX_ERR_MEMORY_ALLOC);
		}

		return sts;
}

	mfxStatus transcode1()
	{
		mfxStatus sts = MFX_ERR_NONE;

		// ===================================
		// Start transcoding the frames
		//



		mfxSyncPoint syncpD, syncpV;
		mfxFrameSurface1* pmfxOutSurface = NULL;
		
		int nIndex = 0;
		int nIndex2 = 0;

		int nTaskIdx = 0;

		sts = MFX_ERR_NONE;
		//
		// Stage 1: Main transcoding loop
		//
	   // while (MFX_ERR_NONE <= sts || MFX_ERR_MORE_DATA == sts || MFX_ERR_MORE_SURFACE == sts) {
		nTaskIdx = FindFreeTask(pTasks, taskPoolSize);      // Find free task
		if (MFX_ERR_NOT_FOUND == nTaskIdx) {
			// No more free tasks, need to sync
			sts = MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

			//sts = WriteBitStreamFrame(&pTasks[nFirstSyncTask].mfxBS, fSink);
			//MSDK_BREAK_ON_ERROR(sts);
			if (bitsout.MaxLength - bitsout.DataLength < pTasks[nFirstSyncTask].mfxBS.DataLength)
				return MFX_ERR_NOT_ENOUGH_BUFFER;
			memmove(bitsout.Data, bitsout.Data + bitsout.DataOffset, bitsout.DataLength);
			bitsout.DataOffset = 0;
			memcpy(bitsout.Data + bitsout.DataLength, pTasks[nFirstSyncTask].mfxBS.Data, pTasks[nFirstSyncTask].mfxBS.DataLength);
			bitsout.DataLength += pTasks[nFirstSyncTask].mfxBS.DataLength;
			pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
			pTasks[nFirstSyncTask].syncp = NULL;
			nFirstSyncTask = (nFirstSyncTask + 1) % taskPoolSize;

			//return MFX_ERR_NONE;
			nTaskIdx = FindFreeTask(pTasks, taskPoolSize);
		}
		

		//if (MFX_ERR_MORE_DATA == sts) {
		//    sts = ReadBitStreamData(&mfxBS, fSource);       // Read more data to input bit stream
		//    MSDK_BREAK_ON_ERROR(sts);
		//}


		nIndex = FindFreeSurface(pSurfaces, nSurfNumDecVPP);        // Find free frame surface
		MSDK_CHECK_ERROR(MFX_ERR_NOT_FOUND, nIndex, MFX_ERR_MEMORY_ALLOC);

		foo:
		// Decode a frame asychronously (returns immediately)
		sts = MFXVideoDECODE_DecodeFrameAsync(session, &bitsin, pSurfaces[nIndex], &pmfxOutSurface, &syncpD);
		if (MFX_WRN_DEVICE_BUSY == sts) {
			MSDK_SLEEP(1);  // just wait and then repeat the same call to DecodeFrameAsync
			goto foo;
		}

		if (MFX_ERR_MORE_SURFACE == sts)
		{
			return sts;
		}
		// Ignore warnings if output is available,
		// if no output and no action required just repeat the DecodeFrameAsync call
		if (MFX_ERR_NONE < sts && syncpD)
			sts = MFX_ERR_NONE;

		if (MFX_ERR_NONE == sts) {
			nIndex2 = FindFreeSurface(pSurfaces2, nSurfNumVPPEnc);      // Find free frame surface
			MSDK_CHECK_ERROR(MFX_ERR_NOT_FOUND, nIndex2, MFX_ERR_MEMORY_ALLOC);

			for (;;) {
				// Process a frame asychronously (returns immediately)
				sts = MFXVideoVPP_RunFrameVPPAsync(session, pmfxOutSurface, pSurfaces2[nIndex2], NULL, &syncpV);

				if (MFX_ERR_NONE < sts && !syncpV) {    // repeat the call if warning and no output
					if (MFX_WRN_DEVICE_BUSY == sts)
						MSDK_SLEEP(1);  // wait if device is busy
				}
				else if (MFX_ERR_NONE < sts && syncpV) {
					sts = MFX_ERR_NONE;     // ignore warnings if output is available
					break;
				}
				else
					break;  // not a warning
			}

			// VPP needs more data, let decoder decode another frame as input
			if (MFX_ERR_MORE_DATA == sts) {
				return sts;
			}
			else if (MFX_ERR_MORE_SURFACE == sts) {
				// Not relevant for the illustrated workload! Therefore not handled.
				// Relevant for cases when VPP produces more frames at output than consumes at input. E.g. framerate conversion 30 fps -> 60 fps
				return sts;
			}
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);


			for (;;) {
				// Encode a frame asychronously (returns immediately)
				sts = MFXVideoENCODE_EncodeFrameAsync(session, NULL, pSurfaces2[nIndex2], &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);

				if (MFX_ERR_NONE < sts && !pTasks[nTaskIdx].syncp) {    // repeat the call if warning and no output
					if (MFX_WRN_DEVICE_BUSY == sts)
						MSDK_SLEEP(1);  // wait if device is busy
				}
				else if (MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp) {
					sts = MFX_ERR_NONE;     // ignore warnings if output is available
					break;
				}
				else if (MFX_ERR_NOT_ENOUGH_BUFFER == sts) {
					// Allocate more bitstream buffer memory here if needed...
					break;
				}
				else
					break;
			}

			if (MFX_ERR_MORE_DATA == sts) {
				// MFX_ERR_MORE_DATA indicates encoder need more input, request more surfaces from previous operation
				//sts = MFX_ERR_NONE;
				return sts;
			}
			//}
		}
		//}

		// MFX_ERR_MORE_DATA means that file has ended, need to go to buffering loop, exit in case of other errors
		//MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		return sts;
	}

	mfxStatus transcode2()
	{
		mfxSyncPoint syncpD, syncpV;
		mfxFrameSurface1* pmfxOutSurface = NULL;
		mfxStatus sts = MFX_ERR_NONE;

		//
		// Stage 2: Retrieve the buffered decoded frames
		//
		if (MFX_ERR_NONE <= sts || MFX_ERR_MORE_SURFACE == sts) {
			int nTaskIdx = FindFreeTask(pTasks, taskPoolSize);      // Find free task
			if (MFX_ERR_NOT_FOUND == nTaskIdx) {
				// No more free tasks, need to sync
				sts = MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
				MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);



				//sts = WriteBitStreamFrame(&pTasks[nFirstSyncTask].mfxBS, fSink);
				//MSDK_BREAK_ON_ERROR(sts);
				if (bitsout.MaxLength - bitsout.DataLength < pTasks[nFirstSyncTask].mfxBS.DataLength)
					return MFX_ERR_NOT_ENOUGH_BUFFER;
				memmove(bitsout.Data, bitsout.Data + bitsout.DataOffset, bitsout.DataLength);
				bitsout.DataOffset = 0;
				memcpy(bitsout.Data + bitsout.DataLength, pTasks[nFirstSyncTask].mfxBS.Data, pTasks[nFirstSyncTask].mfxBS.DataLength);
				bitsout.DataLength += pTasks[nFirstSyncTask].mfxBS.DataLength;
				pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
				pTasks[nFirstSyncTask].syncp = NULL;
				nFirstSyncTask = (nFirstSyncTask + 1) % taskPoolSize;

				//return MFX_ERR_NONE;
				nTaskIdx = FindFreeTask(pTasks, taskPoolSize);
			}
			//else
			{
		

				int nIndex = FindFreeSurface(pSurfaces, nSurfNumDecVPP);        // Find free frame surface
				MSDK_CHECK_ERROR(MFX_ERR_NOT_FOUND, nIndex, MFX_ERR_MEMORY_ALLOC);
				foo:
				// Decode a frame asychronously (returns immediately)
				sts = MFXVideoDECODE_DecodeFrameAsync(session, NULL, pSurfaces[nIndex], &pmfxOutSurface, &syncpD);
				if (MFX_WRN_DEVICE_BUSY == sts) {
					MSDK_SLEEP(1);  // just wait and then repeat the same call to DecodeFrameAsync
					goto foo;
				}
				// Ignore warnings if output is available,
				// if no output and no action required just repeat the DecodeFrameAsync call
				if (MFX_ERR_NONE < sts && syncpD)
					sts = MFX_ERR_NONE;

				if (MFX_ERR_NONE == sts) {
					int nIndex2 = FindFreeSurface(pSurfaces2, nSurfNumVPPEnc);      // Find free frame surface
					MSDK_CHECK_ERROR(MFX_ERR_NOT_FOUND, nIndex2, MFX_ERR_MEMORY_ALLOC);

					for (;;) {
						// Process a frame asychronously (returns immediately)
						sts = MFXVideoVPP_RunFrameVPPAsync(session, pmfxOutSurface, pSurfaces2[nIndex2], NULL, &syncpV);

						if (MFX_ERR_NONE < sts && !syncpV) {    // repeat the call if warning and no output
							if (MFX_WRN_DEVICE_BUSY == sts)
								MSDK_SLEEP(1);  // wait if device is busy
						}
						else if (MFX_ERR_NONE < sts && syncpV) {
							sts = MFX_ERR_NONE;     // ignore warnings if output is available
							break;
						}
						else
							break;  // not a warning
					}

					// VPP needs more data, let decoder decode another frame as input
					if (MFX_ERR_MORE_DATA == sts) {
						return sts;
					}
					else if (MFX_ERR_MORE_SURFACE == sts) {
						// Not relevant for the illustrated workload! Therefore not handled.
						// Relevant for cases when VPP produces more frames at output than consumes at input. E.g. framerate conversion 30 fps -> 60 fps
						return sts;
					}
					//else
						//MSDK_BREAK_ON_ERROR(sts);

					for (;;) {
						// Encode a frame asychronously (returns immediately)
						sts = MFXVideoENCODE_EncodeFrameAsync(session, NULL, pSurfaces2[nIndex2], &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);

						if (MFX_ERR_NONE < sts && !pTasks[nTaskIdx].syncp) {    // repeat the call if warning and no output
							if (MFX_WRN_DEVICE_BUSY == sts)
								MSDK_SLEEP(1);  // wait if device is busy
						}
						else if (MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp) {
							sts = MFX_ERR_NONE;     // ignore warnings if output is available
							break;
						}
						else if (MFX_ERR_NOT_ENOUGH_BUFFER == sts) {
							// Allocate more bitstream buffer memory here if needed...
							break;
						}
						else
							break;
					}

					if (MFX_ERR_MORE_DATA == sts) {
						// MFX_ERR_MORE_DATA indicates encoder need more input, request more surfaces from previous operation
						//sts = MFX_ERR_NONE;
						return sts;
					}
				}
			}
		}

		// MFX_ERR_MORE_DATA indicates that all decode buffers has been fetched, exit in case of other errors
		MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		return sts;
	}

	mfxStatus transcode3()
	{
		mfxSyncPoint syncpV;
//		mfxFrameSurface1* pmfxOutSurface = NULL;
		mfxStatus sts = MFX_ERR_NONE;

		//
		// Stage 3: Retrieve buffered frames from VPP
		//
		if (MFX_ERR_NONE <= sts || MFX_ERR_MORE_DATA == sts || MFX_ERR_MORE_SURFACE == sts) {
			int nTaskIdx = FindFreeTask(pTasks, taskPoolSize);      // Find free task
			if (MFX_ERR_NOT_FOUND == nTaskIdx) {
				// No more free tasks, need to sync
				sts = MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
				MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

				//sts = WriteBitStreamFrame(&pTasks[nFirstSyncTask].mfxBS, fSink);
				//MSDK_BREAK_ON_ERROR(sts);
				if (bitsout.MaxLength - bitsout.DataLength < pTasks[nFirstSyncTask].mfxBS.DataLength)
					return MFX_ERR_NOT_ENOUGH_BUFFER;
				memmove(bitsout.Data, bitsout.Data + bitsout.DataOffset, bitsout.DataLength);
				bitsout.DataOffset = 0;
				memcpy(bitsout.Data + bitsout.DataLength, pTasks[nFirstSyncTask].mfxBS.Data, pTasks[nFirstSyncTask].mfxBS.DataLength);
				bitsout.DataLength += pTasks[nFirstSyncTask].mfxBS.DataLength;
				pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
				pTasks[nFirstSyncTask].syncp = NULL;
				nFirstSyncTask = (nFirstSyncTask + 1) % taskPoolSize;


			}
			//else
			{
				int nIndex2 = FindFreeSurface(pSurfaces2, nSurfNumVPPEnc);      // Find free frame surface
				MSDK_CHECK_ERROR(MFX_ERR_NOT_FOUND, nIndex2, MFX_ERR_MEMORY_ALLOC);

				for (;;) {
					// Process a frame asychronously (returns immediately)
					sts = MFXVideoVPP_RunFrameVPPAsync(session, NULL, pSurfaces2[nIndex2], NULL, &syncpV);

					if (MFX_ERR_NONE < sts && !syncpV) {    // repeat the call if warning and no output
						if (MFX_WRN_DEVICE_BUSY == sts)
							MSDK_SLEEP(1);  // wait if device is busy
					}
					else if (MFX_ERR_NONE < sts && syncpV) {
						sts = MFX_ERR_NONE;     // ignore warnings if output is available
						break;
					}
					else
						break;  // not a warning
				}

				if (MFX_ERR_MORE_SURFACE == sts) {
					// Not relevant for the illustrated workload! Therefore not handled.
					// Relevant for cases when VPP produces more frames at output than consumes at input. E.g. framerate conversion 30 fps -> 60 fps
					return sts;
				}
				//else
					//MSDK_BREAK_ON_ERROR(sts);

				for (;;) {
					// Encode a frame asychronously (returns immediately)
					sts = MFXVideoENCODE_EncodeFrameAsync(session, NULL, pSurfaces2[nIndex2], &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);

					if (MFX_ERR_NONE < sts && !pTasks[nTaskIdx].syncp) {    // repeat the call if warning and no output
						if (MFX_WRN_DEVICE_BUSY == sts)
							MSDK_SLEEP(1);  // wait if device is busy
					}
					else if (MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp) {
						sts = MFX_ERR_NONE;     // ignore warnings if output is available
						break;
					}
					else if (MFX_ERR_NOT_ENOUGH_BUFFER == sts) {
						// Allocate more bitstream buffer memory here if needed...
						break;
					}
					else
						break;
				}

				if (MFX_ERR_MORE_DATA == sts) {
					// MFX_ERR_MORE_DATA indicates encoder need more input, request more surfaces from previous operation
					//sts = MFX_ERR_NONE;
					return sts;
				}
			}
		}

		// MFX_ERR_MORE_DATA indicates that all VPP buffers has been fetched, exit in case of other errors
		MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		return sts;
	}
	mfxStatus transcode4()
	{
//		mfxFrameSurface1* pmfxOutSurface = NULL;
		mfxStatus sts = MFX_ERR_NONE;

		//
		// Stage 4: Retrieve the buffered encoded frames
		//
		if (MFX_ERR_NONE <= sts) {
			int nTaskIdx = FindFreeTask(pTasks, taskPoolSize);      // Find free task
			if (MFX_ERR_NOT_FOUND == nTaskIdx) {
				// No more free tasks, need to sync
				sts = MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
				MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

				//sts = WriteBitStreamFrame(&pTasks[nFirstSyncTask].mfxBS, fSink);
				//MSDK_BREAK_ON_ERROR(sts);
				if (bitsout.MaxLength - bitsout.DataLength < pTasks[nFirstSyncTask].mfxBS.DataLength)
					return MFX_ERR_NOT_ENOUGH_BUFFER;
				memmove(bitsout.Data, bitsout.Data + bitsout.DataOffset, bitsout.DataLength);
				bitsout.DataOffset = 0;
				memcpy(bitsout.Data + bitsout.DataLength, pTasks[nFirstSyncTask].mfxBS.Data, pTasks[nFirstSyncTask].mfxBS.DataLength);
				bitsout.DataLength += pTasks[nFirstSyncTask].mfxBS.DataLength;
				pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
				pTasks[nFirstSyncTask].syncp = NULL;
				nFirstSyncTask = (nFirstSyncTask + 1) % taskPoolSize;


			}
			else
			{
				for (;;) {
					// Encode a frame asychronously (returns immediately)
					sts = MFXVideoENCODE_EncodeFrameAsync(session, NULL, NULL, &pTasks[nTaskIdx].mfxBS, &pTasks[nTaskIdx].syncp);

					if (MFX_ERR_NONE < sts && !pTasks[nTaskIdx].syncp) {    // repeat the call if warning and no output
						if (MFX_WRN_DEVICE_BUSY == sts)
							MSDK_SLEEP(1);  // wait if device is busy
					}
					else if (MFX_ERR_NONE < sts && pTasks[nTaskIdx].syncp) {
						sts = MFX_ERR_NONE;     // ignore warnings if output is available
						break;
					}
					else
						break;
				}
			}
		}

		// MFX_ERR_MORE_DATA indicates that there are no more buffered frames, exit in case of other errors
		MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		//
		// Stage 5: Sync all remaining tasks in task pool
		//
		while (pTasks[nFirstSyncTask].syncp) {
			sts = MFXVideoCORE_SyncOperation(session, pTasks[nFirstSyncTask].syncp, 60000);
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

			//sts = WriteBitStreamFrame(&pTasks[nFirstSyncTask].mfxBS, fSink);
			//MSDK_BREAK_ON_ERROR(sts);
			if (bitsout.MaxLength - bitsout.DataLength < pTasks[nFirstSyncTask].mfxBS.DataLength)
				return MFX_ERR_NOT_ENOUGH_BUFFER;
			memmove(bitsout.Data, bitsout.Data + bitsout.DataOffset, bitsout.DataLength);
			bitsout.DataOffset = 0;
			memcpy(bitsout.Data + bitsout.DataLength, pTasks[nFirstSyncTask].mfxBS.Data, pTasks[nFirstSyncTask].mfxBS.DataLength);
			bitsout.DataLength += pTasks[nFirstSyncTask].mfxBS.DataLength;
			pTasks[nFirstSyncTask].mfxBS.DataLength = 0;
			pTasks[nFirstSyncTask].syncp = NULL;
			nFirstSyncTask = (nFirstSyncTask + 1) % taskPoolSize;

			// inc frame count output here
		}
	}
	~CTranscoder()
	{

		// ===================================================================
		// Clean up resources
		//  - It is recommended to close Media SDK components first, before releasing allocated surfaces, since
		//    some surfaces may still be locked by internal Media SDK resources.


		MFXVideoENCODE_Close(session);
		MFXVideoDECODE_Close(session);
		MFXVideoVPP_Close(session);

		// session closed automatically on destruction

		for (int i = 0; i < nSurfNumDecVPP; i++)
			delete pSurfaces[i];
		for (int i = 0; i < nSurfNumVPPEnc; i++)
			delete pSurfaces2[i];
		MSDK_SAFE_DELETE_ARRAY(pSurfaces);
		MSDK_SAFE_DELETE_ARRAY(pSurfaces2);
		MSDK_SAFE_DELETE_ARRAY(bitsin.Data);
		MSDK_SAFE_DELETE_ARRAY(bitsout.Data);
		for (int i = 0; i < taskPoolSize; i++)
			MSDK_SAFE_DELETE_ARRAY(pTasks[i].mfxBS.Data);
		MSDK_SAFE_DELETE_ARRAY(pTasks);


		//Release();
		vas.~VideoAccelerationSupport();

		return;
	}
};



int main(int argc, char** argv)
{
	mfxStatus sts = MFX_ERR_NONE;
	
	

	char *srcname = "BigBuckBunny_320x180.264";
	char *dstname = "BigBuckBunny_320x180.tx.264";
	bool bEnableOutput = true; // if true, removes all output bitsteam file writing and printing the progress
	mfxU16 bitrate = 1000;
	//mfxU16 width = 320;
	//mfxU16 height = 180;
//	mfxIMPL impl = MFX_IMPL_AUTO_ANY;

	


	

	// Open input H.264 elementary stream (ES) file
	FILE* fSource;
	MSDK_FOPEN(fSource, srcname, "rb");
	MSDK_CHECK_POINTER(fSource, MFX_ERR_NULL_PTR);

	// Create output elementary stream (ES) H.264 file
	FILE* fSink = NULL;
	if (bEnableOutput) {
		MSDK_FOPEN(fSink, dstname, "wb");
		MSDK_CHECK_POINTER(fSink, MFX_ERR_NULL_PTR);
	}

	// Initialize Media SDK session
	// - MFX_IMPL_AUTO_ANY selects HW acceleration if available (on any adapter)
	// - Version 1.3 is selected since the opaque memory feature was added in this API release
	//   If more recent API features are needed, change the version accordingly
	 
	//mfxVersion ver = { { 3, 1 } };      // Note: API 1.3 !
	mfxSession session;
	mfxInitParam initparam;
	memset(&initparam, 0, sizeof(initparam));
	initparam.Implementation = MFX_IMPL_HARDWARE;
	initparam.GPUCopy = MFX_GPUCOPY_ON;
	//initparam.GPUCopy = MFX_GPUCOPY_OFF;
	initparam.Version = { { 0,1 } };
	sts = MFXInitEx(initparam, &session);
	MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

	// Create Media SDK decoder & encoder
	//MFXVideoDECODE mfxDEC(session);
	//MFXVideoENCODE mfxENC(session);
	//MFXVideoVPP mfxVPP(session);

	// Set required video parameters for decode
	// - In this example we are decoding an AVC (H.264) stream
	mfxVideoParam mfxDecParams;
	memset(&mfxDecParams, 0, sizeof(mfxDecParams));
	mfxDecParams.mfx.CodecId = MFX_CODEC_AVC;
	mfxDecParams.IOPattern = MFX_IOPATTERN_OUT_OPAQUE_MEMORY;

	// Configure Media SDK to keep more operations in flight
	// - AsyncDepth represents the number of tasks that can be submitted, before synchronizing is required
	// - The choice of AsyncDepth = 4 is quite arbitrary but has proven to result in good performance
	mfxDecParams.AsyncDepth = 4;

	// Prepare Media SDK bit stream buffer for decoder
	// - Arbitrary buffer size for this example
	mfxBitstream mfxBS;
	memset(&mfxBS, 0, sizeof(mfxBS));
	mfxBS.MaxLength = 1024 * 1024;
	mfxBS.Data = new mfxU8[mfxBS.MaxLength];
	MSDK_CHECK_POINTER(mfxBS.Data, MFX_ERR_MEMORY_ALLOC);

	// Read a chunk of data from stream file into bit stream buffer
	// - Parse bit stream, searching for header and fill video parameters structure
	// - Abort if bit stream header is not found in the first bit stream buffer chunk
	sts = ReadBitStreamData(&mfxBS, fSource);
	MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

	sts = MFXVideoDECODE_DecodeHeader(session, &mfxBS, &mfxDecParams);
	MSDK_IGNORE_MFX_STS(sts, MFX_WRN_PARTIAL_ACCELERATION);
	MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

	// Initialize VPP parameters
	mfxVideoParam VPPParams;
	memset(&VPPParams, 0, sizeof(VPPParams));
	// Input data
	VPPParams.vpp.In.FourCC = MFX_FOURCC_NV12;
	VPPParams.vpp.In.ChromaFormat = MFX_CHROMAFORMAT_YUV420;
	VPPParams.vpp.In.CropX = 0;
	VPPParams.vpp.In.CropY = 0;
	VPPParams.vpp.In.CropW = mfxDecParams.mfx.FrameInfo.CropW;
	VPPParams.vpp.In.CropH = mfxDecParams.mfx.FrameInfo.CropH;
	VPPParams.vpp.In.PicStruct = MFX_PICSTRUCT_PROGRESSIVE;
	VPPParams.vpp.In.FrameRateExtN = 30;
	VPPParams.vpp.In.FrameRateExtD = 1;
	// width must be a multiple of 16
	// height must be a multiple of 16 in case of frame picture and a multiple of 32 in case of field picture
	VPPParams.vpp.In.Width = MSDK_ALIGN16(VPPParams.vpp.In.CropW);
	VPPParams.vpp.In.Height =
		(MFX_PICSTRUCT_PROGRESSIVE == VPPParams.vpp.In.PicStruct) ?
		MSDK_ALIGN16(VPPParams.vpp.In.CropH) :
		MSDK_ALIGN32(VPPParams.vpp.In.CropH);
	// Output data
	VPPParams.vpp.Out.FourCC = MFX_FOURCC_NV12;
	VPPParams.vpp.Out.ChromaFormat = MFX_CHROMAFORMAT_YUV420;
	VPPParams.vpp.Out.CropX = 0;
	VPPParams.vpp.Out.CropY = 0;
	VPPParams.vpp.Out.CropW = VPPParams.vpp.In.CropW / 4;   // 1/16th the resolution of decode stream
	VPPParams.vpp.Out.CropH = VPPParams.vpp.In.CropH / 4;
	VPPParams.vpp.Out.PicStruct = MFX_PICSTRUCT_PROGRESSIVE;
	VPPParams.vpp.Out.FrameRateExtN = 30;
	VPPParams.vpp.Out.FrameRateExtD = 1;
	// width must be a multiple of 16
	// height must be a multiple of 16 in case of frame picture and a multiple of 32 in case of field picture
	VPPParams.vpp.Out.Width = MSDK_ALIGN16(VPPParams.vpp.Out.CropW);
	VPPParams.vpp.Out.Height =
		(MFX_PICSTRUCT_PROGRESSIVE == VPPParams.vpp.Out.PicStruct) ?
		MSDK_ALIGN16(VPPParams.vpp.Out.CropH) :
		MSDK_ALIGN32(VPPParams.vpp.Out.CropH);

	VPPParams.IOPattern =
		MFX_IOPATTERN_IN_OPAQUE_MEMORY | MFX_IOPATTERN_OUT_OPAQUE_MEMORY;

	// Configure Media SDK to keep more operations in flight
	// - AsyncDepth represents the number of tasks that can be submitted, before synchronizing is required
	VPPParams.AsyncDepth = mfxDecParams.AsyncDepth;

	// Initialize encoder parameters
	// - In this example we are encoding an AVC (H.264) stream
	mfxVideoParam mfxEncParams;
	memset(&mfxEncParams, 0, sizeof(mfxEncParams));
	mfxEncParams.mfx.CodecId = MFX_CODEC_AVC;
	mfxEncParams.mfx.TargetUsage = MFX_TARGETUSAGE_BALANCED;
	mfxEncParams.mfx.TargetKbps = bitrate;
	mfxEncParams.mfx.RateControlMethod = MFX_RATECONTROL_VBR;
	mfxEncParams.mfx.FrameInfo.FrameRateExtN = 30;
	mfxEncParams.mfx.FrameInfo.FrameRateExtD = 1;
	mfxEncParams.mfx.FrameInfo.FourCC = MFX_FOURCC_NV12;
	mfxEncParams.mfx.FrameInfo.ChromaFormat = MFX_CHROMAFORMAT_YUV420;
	mfxEncParams.mfx.FrameInfo.PicStruct = MFX_PICSTRUCT_PROGRESSIVE;
	mfxEncParams.mfx.FrameInfo.CropX = 0;
	mfxEncParams.mfx.FrameInfo.CropY = 0;
	mfxEncParams.mfx.FrameInfo.CropW = VPPParams.vpp.Out.CropW;     // Half the resolution of decode stream
	mfxEncParams.mfx.FrameInfo.CropH = VPPParams.vpp.Out.CropH;
	// width must be a multiple of 16
	// height must be a multiple of 16 in case of frame picture and a multiple of 32 in case of field picture
	mfxEncParams.mfx.FrameInfo.Width = MSDK_ALIGN16(mfxEncParams.mfx.FrameInfo.CropW);
	mfxEncParams.mfx.FrameInfo.Height =
		(MFX_PICSTRUCT_PROGRESSIVE == mfxEncParams.mfx.FrameInfo.PicStruct) ?
		MSDK_ALIGN16(mfxEncParams.mfx.FrameInfo.CropH) :
		MSDK_ALIGN32(mfxEncParams.mfx.FrameInfo.CropH);

	mfxEncParams.IOPattern = MFX_IOPATTERN_IN_OPAQUE_MEMORY;

	// Configure Media SDK to keep more operations in flight
	// - AsyncDepth represents the number of tasks that can be submitted, before synchronizing is required
	mfxEncParams.AsyncDepth = mfxDecParams.AsyncDepth;



	
	CTranscoder *x = new CTranscoder();
	 


	sts = x->init(session, mfxDecParams, VPPParams, mfxEncParams);
	MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

	int nFrame = 0;


	BenchmarkClock tStart, tEnd;
	GetBenchmarkClock(&tStart);

	

	while (true)
	{
		sts = x->transcode1();
		if (sts == MFX_ERR_MORE_DATA)
		{
			sts = ReadBitStreamData(&x->bitsout, fSource); // Read more data into input bit stream
			if (sts == MFX_ERR_MORE_DATA)
				break;
			MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
		}
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
		if (x->bitsout.DataLength && fSink)
			fwrite(x->bitsout.Data, x->bitsout.DataLength, 1, fSink);

		if (sts != MFX_ERR_NONE)
			break;
	}

	MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
	MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

	printf("n=%d\n", nFrame);

	//while (true)
	//{
	//	sts = x->flush1(&frame);
	//	if (sts == MFX_ERR_MORE_DATA)
	//		break;
	//	MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
	//	if (frame) {
	//		if (fSink) WriteRawFrame(frame, fSink);
	//		nFrame++;
	//	}

	//}
	//MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
	//MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

	//printf("n=%d\n", nFrame);

	//while (true)
	//{
	//	sts = d.flush2(&frame);
	//	if (sts == MFX_ERR_MORE_DATA)
	//		break;
	//	MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
	//	if (frame)
	//		if (fSink) WriteRawFrame(frame, fSink);
	//	nFrame++;
	//}
	//MSDK_IGNORE_MFX_STS(sts, MFX_ERR_MORE_DATA);
	//MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);


	GetBenchmarkClock(&tEnd);
	double elapsed = BenchmarkClockDelta(tEnd, tStart) / 1000;
	double fps = ((double)nFrame / elapsed);
	printf("\nExecution time: %3.2f s (%3.2f fps)\n", elapsed, fps);

	printf("nframe = %d\n", nFrame);
}
