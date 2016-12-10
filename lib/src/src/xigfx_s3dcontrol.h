#pragma once

#undef NDEBUG
#include <assert.h>

#include "d3d_device.h" 

typedef long HRESULT;


typedef struct _IGFX_DISPLAY_MODE
{} IGFX_DISPLAY_MODE;

typedef struct _IGFX_S3DCAPS
{} IGFX_S3DCAPS;

class IGFXS3DControl
{
public:
	 ~IGFXS3DControl() {};
	 HRESULT GetS3DCaps(IGFX_S3DCAPS *pCaps) { assert(0); }
	 HRESULT SwitchTo3D(IGFX_DISPLAY_MODE *pMode) { assert(0); }
	 HRESULT SwitchTo2D(IGFX_DISPLAY_MODE *pMode) { assert(0); }
	 HRESULT SetDevice(IDirect3DDeviceManager9 *pDeviceManager) { assert(0); }
	 HRESULT SelectRightView() { assert(0); }
};

//#ifdef _WIN32
//#define IGFX_CDECL __cdecl
//#define IGFX_STDCALL __stdcall
//#else
//#define IGFX_CDECL
//#define IGFX_STDCALL
//#endif /* _WIN32 */
//
//extern IGFXS3DControl * IGFX_CDECL CreateIGFXS3DControl();
//
//#undef IGFX_CDECL
//#undef IGFX_STDCALL
