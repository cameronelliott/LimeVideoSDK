// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory


#include "_VideoAccelerationSupport.h"



// fix: warning LNK4248: unresolved typeref token (01000014) for '_mfxSession'; image may not run
struct _mfxSession { };
struct _mfxSyncPoint { };




#pragma warning(disable : 4100)





/*
The following 3 functions were compared:
mfxStatus CDecodingPipeline::CreateAllocator()
mfxStatus CEncodingPipeline::CreateAllocator()
mfxStatus CRegionEncodingPipeline::CreateAllocator()
mfxStatus CCameraPipeline::CreateAllocator()

These differences were observed:
CDecodingPipeline is based around the GeneralAllocator which supports both system memory and/or video memory
allocations.

CEncodingPipeline:: and CRegionEncodingPipeline:: do use GeneralAllocator,
and create both D3D11FrameAllocator and D3D9FrameAllocator directly.

The CRegionEncodingPipeline:: does not support HW encoding, and supports arrays of sessions,
and seems to far afield to worry about further.

CCameraPipeline:: is only supported on Windows and not of use to us.

Eliminated: CCameraPipeline, CRegionEncodingPipeline, CDecodingPipeline.
Currently choosen: CDecodingPipeline
Choosen: CEncodingPipeline::CreateAllocator()


FURTHER:
Both CDecodingPipeline::CreateHWDevice() and CEncodingPipeline::CreateHWDevice()
 were compared.
 The big difference is that CDecodingPipeline::CreateHWDevice() supports rendering.
*/


#if 0

EXTERN_DLL_EXPORT
VideoAccelerationSupport* VideoAccelerationSupport_New()
{
	return new VideoAccelerationSupport();
}

// mfxinit has been called prior to entry
EXTERN_DLL_EXPORT
mfxStatus VideoAccelerationSupport_Init(VideoAccelerationSupport* handle, mfxSession s, bool rendering, MemType memType, int linuxLibvaBackend = 0, int linuxMonitorType = 0, bool linuxWaylandPerfMode = false)
{
	mfxStatus sts;
	// controls whether certain stuff is setup during initialization to support rendering to windows on-screen
	handle->m_eWorkMode = rendering ? MODE_RENDERING : MODE_PERFORMANCE;
	// controls whether certain stuff is setup during initialization to support non-system memory allocations
	// when in doubt this should not be SYSTEM_MEMORY. If SYSTEM_MEMORY is passed, video memory will not be allocable.
	handle->m_memType = memType;
	// this flag merely reflects handle->m_memType, and is present so we can use the distribute unmodified copies
	// of CDecoderPipeline::CreateHWDevice and CDecoderPipeline::CreateAllocator
	handle->m_bDecOutSysmem = handle->m_memType == SYSTEM_MEMORY;

#if defined(LIBVA_SUPPORT)
	handle->m_libvaBackend = linuxLibvaBackend;
	handle->m_bPerfMode = linuxWaylandPerfMode;
	handle->m_monitorType = linuxMonitorType;
#endif

	handle->m_mfxSession.SetSession(s);

	// cannot do these lines before handle->CreateAllocator() on Linux!
	//sts = handle->m_mfxSession.QueryIMPL(&(handle->m_impl)); // needed inside sample_support classes
	//MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

	// linux only
	// this only controls a single thing: whether the linux device is created
	// (search for m_impl in this file)
	// it does not affect windows operation, at time of writing
	handle->m_impl = MFX_IMPL_HARDWARE;

	sts = handle->CreateAllocator();
	MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

	return MFX_ERR_NONE;
}

EXTERN_DLL_EXPORT
mfxStatus VideoAccelerationSupport_Alloc(VideoAccelerationSupport* handle, mfxFrameAllocRequest* req, mfxFrameAllocResponse* resp)
{
	return handle->m_pGeneralAllocator->Alloc(
		dynamic_cast<MFXFrameAllocator*>(handle->m_pGeneralAllocator),
		req, resp);
}

EXTERN_DLL_EXPORT
mfxStatus VideoAccelerationSupport_LockFrame(VideoAccelerationSupport* handle, mfxMemId memid, mfxFrameData* ptr)
{
	return handle->m_pGeneralAllocator->Lock(
		dynamic_cast<MFXFrameAllocator*>(handle->m_pGeneralAllocator), memid, ptr);
}

EXTERN_DLL_EXPORT
mfxStatus VideoAccelerationSupport_UnlockFrame(VideoAccelerationSupport* handle, mfxMemId memid, mfxFrameData* ptr)
{
	return handle->m_pGeneralAllocator->Unlock(
		dynamic_cast<MFXFrameAllocator*>(handle->m_pGeneralAllocator), memid, ptr);
}


EXTERN_DLL_EXPORT
void VideoAccelerationSupport_Release(VideoAccelerationSupport* handle)
{
	delete handle;
}

EXTERN_DLL_EXPORT
mfxStatus VideoAccelerationSupport_GetFrameHDL(VideoAccelerationSupport* handle, mfxMemId mid, mfxHDL *hdlhandle)
{
	return handle->m_pGeneralAllocator->GetHDL(
		dynamic_cast<MFXFrameAllocator*>(handle->m_pGeneralAllocator), mid, hdlhandle);
}


EXTERN_DLL_EXPORT
mfxStatus VideoAccelerationSupport_DeviceGetHandle(VideoAccelerationSupport* handle, mfxHandleType type, mfxHDL *hdlhandle)
{
	return handle->m_hwdev->GetHandle(type, hdlhandle);
}
#endif