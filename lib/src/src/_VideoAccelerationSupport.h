#pragma region  Open Source Software License
// Copyright 2012-2016 Cameron Elliott. BSD License Open Source Software. 
// BSD License terms found in the file LICENSE.txt at the top-level directory of this distribution.

#pragma endregion


#pragma once


#include "mfxvideo.h"
#include "sample_defs.h"
#include "general_allocator.h"
#include "decode_render.h"






#if defined(_WIN32) || defined(_WIN64)
#include "comdef.h"
#include "d3d_allocator.h"
#include "d3d11_allocator.h"
#include "d3d_device.h"
#include "d3d11_device.h"
#define EXTERN_DLL_EXPORT extern "C" __declspec(dllexport)

#else
#define EXTERN_DLL_EXPORT extern "C"
#endif

#if defined(LIBVA_SUPPORT)
#include "vaapi_allocator.h"
#include "vaapi_device.h"
#include "vaapi_utils.h"
#endif

#if defined(LIBVA_WAYLAND_SUPPORT)
#include "class_wayland.h"
#endif

#pragma warning(disable : 4100)





enum MemType {
	SYSTEM_MEMORY = 0x00,
	D3D9_MEMORY = 0x01,
	D3D11_MEMORY = 0x02,
	VAAPI_MEMORY = 0x88,        // CSE added
};
enum eWorkMode {
	MODE_PERFORMANCE, // different from below two
	MODE_RENDERING, // same as file dump
	MODE_FILE_DUMP // same as rendering
};








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


class XMFXVideoSession : public MFXVideoSession
{
public:
	void SetSession(mfxSession s) {
		m_session = s;
	}
};
// derived from mfxStatus CDecodingPipeline::CreateHWDevice(). version 3 / untouched, except class
class VideoAccelerationSupport {
public:
	MemType					m_memType = SYSTEM_MEMORY;
	eWorkMode				m_eWorkMode = MODE_PERFORMANCE;
#if D3D_SURFACES_SUPPORT
	IGFXS3DControl          *m_pS3DControl = NULL;
	CDecodeD3DRender         m_d3dRender;
#endif
	GeneralAllocator*       m_pGeneralAllocator = NULL;
	CHWDevice               *m_hwdev = NULL;
	mfxAllocatorParams*     m_pmfxAllocatorParams = NULL;


	bool m_bIsMVC = false;
	bool m_bExternalAlloc = false;
	bool m_bDecOutSysmem = false;

	XMFXVideoSession         m_mfxSession;

#if defined(LIBVA_SUPPORT)
	mfxI32                  m_libvaBackend; // is input [cse]  controls 
											// see     MFX_LIBVA_AUTO, MFX_LIBVA_DRM, MFX_LIBVA_WAYLAND, others

	bool                    m_bPerfMode;	// is input [cse]  appears to be wayland related performance flag
	mfxU32                  m_export_mode;	// is not input
	mfxI32                  m_monitorType;	// is input, see -rdrm in readme-decode_linux.pdf and sample_decode.cpp, see transcode pdf/cpp too.
											// appears to control on-screen rendering in linux 
											// can control render connector: DisplayPort, HDMIA, HDMIB, VGA, DVII, DVID, DVIA, eDP and others

#endif // defined(MFX_LIBVA_SUPPORT)
	mfxIMPL					m_impl;

	VideoAccelerationSupport()
	{
	}

	~VideoAccelerationSupport()
	{
#if D3D_SURFACES_SUPPORT
		MSDK_SAFE_DELETE(m_pS3DControl);
#endif

		MSDK_SAFE_DELETE(m_pGeneralAllocator);
		MSDK_SAFE_DELETE(m_hwdev);
		MSDK_SAFE_DELETE(m_pmfxAllocatorParams);
	}

	mfxStatus Init(mfxSession s, bool rendering, MemType memType, int linuxLibvaBackend = 0, int linuxMonitorType = 0, bool linuxWaylandPerfMode = false)
	{

		mfxStatus sts;
		// controls whether certain stuff is setup during initialization to support rendering to windows on-screen
		this->m_eWorkMode = rendering ? MODE_RENDERING : MODE_PERFORMANCE;
		// controls whether certain stuff is setup during initialization to support non-system memory allocations
		// when in doubt this should not be SYSTEM_MEMORY. If SYSTEM_MEMORY is passed, video memory will not be allocable.
		this->m_memType = memType;
		// this flag merely reflects this->m_memType, and is present so we can use the distribute unmodified copies
		// of CDecoderPipeline::CreateHWDevice and CDecoderPipeline::CreateAllocator
		this->m_bDecOutSysmem = this->m_memType == SYSTEM_MEMORY;

#if defined(LIBVA_SUPPORT)
		this->m_libvaBackend = linuxLibvaBackend;
		this->m_bPerfMode = linuxWaylandPerfMode;
		this->m_monitorType = linuxMonitorType;
#endif

		this->m_mfxSession.SetSession(s);

		// cannot do these lines before this->CreateAllocator() on Linux!
		//sts = this->m_mfxSession.QueryIMPL(&(this->m_impl)); // needed inside sample_support classes
		//MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		// linux only
		// this only controls a single thing: whether the linux device is created
		// (search for m_impl in this file)
		// it does not affect windows operation, at time of writing
		this->m_impl = MFX_IMPL_HARDWARE;

		sts = this->CreateAllocator();
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		return MFX_ERR_NONE;
	}



	// was CDecodingPipeline::
	mfxStatus CreateAllocator()
	{


		mfxStatus sts = MFX_ERR_NONE;

		m_pGeneralAllocator = new GeneralAllocator();
		if (m_memType != SYSTEM_MEMORY || !m_bDecOutSysmem)
		{
#if D3D_SURFACES_SUPPORT
			sts = CreateHWDevice();
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

			// provide device manager to MediaSDK
			mfxHDL hdl = NULL;
			mfxHandleType hdl_t =
#if MFX_D3D11_SUPPORT
				D3D11_MEMORY == m_memType ? MFX_HANDLE_D3D11_DEVICE :
#endif // #if MFX_D3D11_SUPPORT
				MFX_HANDLE_D3D9_DEVICE_MANAGER;

			sts = m_hwdev->GetHandle(hdl_t, &hdl);
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
			sts = m_mfxSession.SetHandle(hdl_t, hdl);
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

			// create D3D allocator
#if MFX_D3D11_SUPPORT
			if (D3D11_MEMORY == m_memType)
			{
				D3D11AllocatorParams *pd3dAllocParams = new D3D11AllocatorParams;
				MSDK_CHECK_POINTER(pd3dAllocParams, MFX_ERR_MEMORY_ALLOC);
				pd3dAllocParams->pDevice = reinterpret_cast<ID3D11Device *>(hdl);

				m_pmfxAllocatorParams = pd3dAllocParams;
			}
			else
#endif // #if MFX_D3D11_SUPPORT
			{
				D3DAllocatorParams *pd3dAllocParams = new D3DAllocatorParams;
				MSDK_CHECK_POINTER(pd3dAllocParams, MFX_ERR_MEMORY_ALLOC);
				pd3dAllocParams->pManager = reinterpret_cast<IDirect3DDeviceManager9 *>(hdl);

				m_pmfxAllocatorParams = pd3dAllocParams;
			}

			/* In case of video memory we must provide MediaSDK with external allocator
			thus we demonstrate "external allocator" usage model.
			Call SetAllocator to pass allocator to mediasdk */
			sts = m_mfxSession.SetFrameAllocator(m_pGeneralAllocator);
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

			m_bExternalAlloc = true;
#elif LIBVA_SUPPORT
			sts = CreateHWDevice();
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
			/* It's possible to skip failed result here and switch to SW implementation,
			but we don't process this way */

			// provide device manager to MediaSDK
			VADisplay va_dpy = NULL;
			sts = m_hwdev->GetHandle(MFX_HANDLE_VA_DISPLAY, (mfxHDL *)&va_dpy);
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
			sts = m_mfxSession.SetHandle(MFX_HANDLE_VA_DISPLAY, va_dpy);
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

			vaapiAllocatorParams *p_vaapiAllocParams = new vaapiAllocatorParams;
			MSDK_CHECK_POINTER(p_vaapiAllocParams, MFX_ERR_MEMORY_ALLOC);

			p_vaapiAllocParams->m_dpy = va_dpy;
			if (m_eWorkMode == MODE_RENDERING) {
				if (m_libvaBackend == MFX_LIBVA_DRM_MODESET) {
					CVAAPIDeviceDRM* drmdev = dynamic_cast<CVAAPIDeviceDRM*>(m_hwdev);
					p_vaapiAllocParams->m_export_mode = vaapiAllocatorParams::CUSTOM_FLINK;
					p_vaapiAllocParams->m_exporter = dynamic_cast<vaapiAllocatorParams::Exporter*>(drmdev->getRenderer());
				}
				else if (m_libvaBackend == MFX_LIBVA_WAYLAND) {
					p_vaapiAllocParams->m_export_mode = vaapiAllocatorParams::PRIME;
				}
			}
			m_export_mode = p_vaapiAllocParams->m_export_mode;
			m_pmfxAllocatorParams = p_vaapiAllocParams;

			/* In case of video memory we must provide MediaSDK with external allocator
			thus we demonstrate "external allocator" usage model.
			Call SetAllocator to pass allocator to mediasdk */
			sts = m_mfxSession.SetFrameAllocator(m_pGeneralAllocator);
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

			m_bExternalAlloc = true;
#endif
		}
		else
		{
#ifdef LIBVA_SUPPORT
			//in case of system memory allocator we also have to pass MFX_HANDLE_VA_DISPLAY to HW library

			if (MFX_IMPL_HARDWARE == MFX_IMPL_BASETYPE(m_impl))
			{
				sts = CreateHWDevice();
				MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

				// provide device manager to MediaSDK
				VADisplay va_dpy = NULL;
				sts = m_hwdev->GetHandle(MFX_HANDLE_VA_DISPLAY, (mfxHDL *)&va_dpy);
				MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
				sts = m_mfxSession.SetHandle(MFX_HANDLE_VA_DISPLAY, va_dpy);
				MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
			}
#endif
			// create system memory allocator
			//m_pGeneralAllocator = new SysMemFrameAllocator;
			//MSDK_CHECK_POINTER(m_pGeneralAllocator, MFX_ERR_MEMORY_ALLOC);

			/* In case of system memory we demonstrate "no external allocator" usage model.
			We don't call SetAllocator, MediaSDK uses internal allocator.
			We use system memory allocator simply as a memory manager for application*/
		}

		// initialize memory allocator
		sts = m_pGeneralAllocator->Init(m_pmfxAllocatorParams);
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

		return MFX_ERR_NONE;
	}
protected:

	// was CDecodingPipeline::
	mfxStatus CreateHWDevice()
	{
#if D3D_SURFACES_SUPPORT
		mfxStatus sts = MFX_ERR_NONE;

		HWND window = NULL;
		bool render = (m_eWorkMode == MODE_RENDERING);

		if (render) {
			window = (D3D11_MEMORY == m_memType) ? NULL : m_d3dRender.GetWindowHandle();
		}

#if MFX_D3D11_SUPPORT
		if (D3D11_MEMORY == m_memType)
			m_hwdev = new CD3D11Device();
		else
#endif // #if MFX_D3D11_SUPPORT
			m_hwdev = new CD3D9Device();

		if (NULL == m_hwdev)
			return MFX_ERR_MEMORY_ALLOC;

		if (render && m_bIsMVC && m_memType == D3D9_MEMORY) {
			sts = m_hwdev->SetHandle((mfxHandleType)MFX_HANDLE_GFXS3DCONTROL, m_pS3DControl);
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
		}
		sts = m_hwdev->Init(
			window,
			render ? (m_bIsMVC ? 2 : 1) : 0,
			MSDKAdapter::GetNumber(m_mfxSession));
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);



		if (render)
			m_d3dRender.SetHWDevice(m_hwdev);

#elif LIBVA_SUPPORT
		//printf("m_libvaBackend=%d\n", m_libvaBackend);

		mfxStatus sts = MFX_ERR_NONE;
		m_hwdev = CreateVAAPIDevice(m_libvaBackend);

		if (NULL == m_hwdev) {
			printf("Error: got NULL from CreateVAAPIDevice  m_libvaBackend=%d\n", m_libvaBackend);
			return MFX_ERR_MEMORY_ALLOC;
		}

		sts = m_hwdev->Init(&m_monitorType, (m_eWorkMode == MODE_RENDERING) ? 1 : 0, MSDKAdapter::GetNumber(m_mfxSession));
		MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);

#if defined(LIBVA_WAYLAND_SUPPORT)
		if (m_eWorkMode == MODE_RENDERING) {
			mfxHDL hdl = NULL;
			mfxHandleType hdlw_t = (mfxHandleType)HANDLE_WAYLAND_DRIVER;
			Wayland *wld;
			sts = m_hwdev->GetHandle(hdlw_t, &hdl);
			MSDK_CHECK_RESULT(sts, MFX_ERR_NONE, sts);
			wld = (Wayland*)hdl;
			wld->SetRenderWinPos(m_nRenderWinX, m_nRenderWinY);
			wld->SetPerfMode(m_bPerfMode);
		}
#endif //LIBVA_WAYLAND_SUPPORT

#endif
		return MFX_ERR_NONE;
	}
};

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