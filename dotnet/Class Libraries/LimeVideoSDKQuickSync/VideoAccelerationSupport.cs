// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace LimeVideoSDKQuickSync
{


    //if (m_memType != SYSTEM_MEMORY || !m_bDecOutSysmem)

    public class VideoAccelerationSupport : IDisposable
    {
        public enum FrameMemType
        {
            SYSTEM_MEMORY = 0x00,
            D3D9_MEMORY = 0x01,
            D3D11_MEMORY = 0x02,
            VAAPI_MEMORY = 0x80,        // CSE added
            AUTO_MEMORY = 0x81,         // CSE added
        };

        public bool isDirectX11; // I do not like this either.

        public FrameMemType memType = FrameMemType.SYSTEM_MEMORY;


        IntPtr acceleratorHandle;

        public unsafe VideoAccelerationSupport(mfxSession session, bool forceSystemMemory = false)
        {
            mfxStatus sts;
            mfxVersion versionMinimum = new mfxVersion() { Major = 1, Minor = 3 };

            acceleratorHandle = VideoAccelerationSupportPInvoke.VideoAccelerationSupport_New();
            Trace.Assert(acceleratorHandle != IntPtr.Zero);

            if (sizeof(IntPtr) != 8)
                throw new Exception("only x64 supported at this time");

            mfxIMPL ii;
            sts = UnsafeNativeMethods.MFXQueryIMPL(session, &ii);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXQueryIMPL");

            //  if (Environment.OSVersion.Platform == PlatformID.Win32NT)


            mfxIMPL viaMask = (mfxIMPL.MFX_IMPL_VIA_D3D9 | mfxIMPL.MFX_IMPL_VIA_D3D11 | mfxIMPL.MFX_IMPL_VIA_VAAPI);
            if ((ii & viaMask) == mfxIMPL.MFX_IMPL_VIA_D3D11)
            {
                isDirectX11 = true;
                memType = FrameMemType.D3D11_MEMORY;
            }
            else if ((ii & viaMask) == mfxIMPL.MFX_IMPL_VIA_D3D9)
            {
                memType = FrameMemType.D3D9_MEMORY;

            }
            else if ((ii & viaMask) == mfxIMPL.MFX_IMPL_VIA_VAAPI)
            {
                memType = FrameMemType.VAAPI_MEMORY;
            }




            //if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            //{
            //    if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 2)
            //        memType = MemType.D3D11_MEMORY;
            //    else
            //        memType = MemType.D3D9_MEMORY;
            //}
            //else
            //{
            //    memType = MemType.VAAPI_MEMORY;
            //}






            if (forceSystemMemory)
                memType = FrameMemType.SYSTEM_MEMORY;

            sts = VideoAccelerationSupportPInvoke.VideoAccelerationSupport_Init(acceleratorHandle, session, false, memType);
            QuickSyncStatic.ThrowOnBadStatus(sts, "VideoAccelerationSupport_Init");

        }


        public unsafe void AllocFrames(mfxFrameAllocRequest* req, mfxFrameAllocResponse* resp)
        {
            // if ( req->Type)
            var sts = VideoAccelerationSupportPInvoke.VideoAccelerationSupport_Alloc(acceleratorHandle, req, resp);
            QuickSyncStatic.ThrowOnBadStatus(sts, "VideoAccelerationSupport_Alloc");
        }

        public unsafe void LockFrame(IntPtr memid, mfxFrameData* ptr = null)
        {
            if (ptr == null)
                ptr = &(((mfxFrameSurface1*)memid)->Data);

            mfxStatus sts;
            // fixed (mfxFrameData* p = &ptr)
            sts = VideoAccelerationSupportPInvoke.VideoAccelerationSupport_LockFrame(acceleratorHandle, memid, ptr);
            QuickSyncStatic.ThrowOnBadStatus(sts, "VideoAccelerationSupport_LockFrame");
        }
        public unsafe void UnlockFrame(IntPtr memid, mfxFrameData* ptr =null)
        {
            if (ptr == null)
                ptr = &(((mfxFrameSurface1*)memid)->Data);

            mfxStatus sts;
            // fixed (mfxFrameData* p = &ptr)
            sts = VideoAccelerationSupportPInvoke.VideoAccelerationSupport_UnlockFrame(acceleratorHandle, memid, ptr);
            QuickSyncStatic.ThrowOnBadStatus(sts, "VideoAccelerationSupport_UnlockFrame");
        }

        public unsafe IntPtr DeviceGetHandle(mfxHandleType type)
        {
            mfxStatus sts;
            IntPtr handle;
            sts = VideoAccelerationSupportPInvoke.VideoAccelerationSupport_DeviceGetHandle(acceleratorHandle, type, &handle);
            QuickSyncStatic.ThrowOnBadStatus(sts, "VideoAccelerationSupport_DeviceGetHandle");
            return handle;
        }

        public unsafe IntPtr FrameGetHandle(IntPtr mid)
        {
            IntPtr handle;
            mfxStatus sts;
            sts = VideoAccelerationSupportPInvoke.VideoAccelerationSupport_GetFrameHDL(acceleratorHandle, mid, &handle);
            QuickSyncStatic.ThrowOnBadStatus(sts, "VideoAccelerationSupport_GetFrameHDL");
            return handle;
        }






        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

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
                VideoAccelerationSupportPInvoke.VideoAccelerationSupport_Release(acceleratorHandle);
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~VideoAccelerationSupport() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
    class VideoAccelerationSupportPInvoke
    {

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern IntPtr VideoAccelerationSupport_New();

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus VideoAccelerationSupport_Init(IntPtr handle, mfxSession s, bool rendering, VideoAccelerationSupport.FrameMemType memType, int linuxLibvaBackend = 0, int linuxMonitorType = 0, bool linuxWaylandPerfMode = false);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static unsafe extern mfxStatus VideoAccelerationSupport_Alloc(IntPtr handle, mfxFrameAllocRequest* req, mfxFrameAllocResponse* resp);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static unsafe extern mfxStatus VideoAccelerationSupport_LockFrame(IntPtr handle, IntPtr memid, mfxFrameData* ptr);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static unsafe extern mfxStatus VideoAccelerationSupport_UnlockFrame(IntPtr handle, IntPtr memid, mfxFrameData* ptr);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static unsafe extern void VideoAccelerationSupport_Release(IntPtr handle);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static unsafe extern mfxStatus VideoAccelerationSupport_GetFrameHDL(IntPtr handle, IntPtr mid, IntPtr* hdlhandle);


        [DllImport(UnsafeNativeMethods.dllname)]
        public static unsafe extern mfxStatus VideoAccelerationSupport_DeviceGetHandle(IntPtr handle, mfxHandleType type, IntPtr* hdlhandle);

    }
}
