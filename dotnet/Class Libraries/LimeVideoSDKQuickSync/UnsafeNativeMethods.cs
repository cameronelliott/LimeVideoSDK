// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace LimeVideoSDKQuickSync
{
    /// <summary>
    /// Platform independent memory copy routines.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class FastMemcpyMemmove
    {
        // public const string dllname = "LimeVideoSDK.QuickSync.Native.dll";
        static bool isUnix = Environment.OSVersion.Platform == PlatformID.Unix;


        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr memcpyWin(IntPtr dest, IntPtr src, UIntPtr count);

        [DllImport("libc", EntryPoint = "memcpy")]
        static extern IntPtr memcpyUnix(IntPtr dest, IntPtr src, UIntPtr count);


        public static IntPtr memcpy(IntPtr dest, IntPtr src, int count)
        {
            if (isUnix)
                return memcpyUnix(dest, src, (UIntPtr)count);
            else
                return memcpyWin(dest, src, (UIntPtr)count);
        }

        public static IntPtr memcpy(byte* dest, byte* src, int count)
        {
            return memcpy((IntPtr)dest, (IntPtr)src, count);
        }


        [DllImport("msvcrt.dll", EntryPoint = "memmove", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr memmoveWin(IntPtr dest, IntPtr src, UIntPtr count);

        [DllImport("libc", EntryPoint = "memmove")]
        static extern IntPtr memmoveUnix(IntPtr dest, IntPtr src, UIntPtr count);

        public static IntPtr memmove(IntPtr dest, IntPtr src, int count)
        {
            if (isUnix)
                return memmoveUnix(dest, src, (UIntPtr)count);
            else
                return memmoveWin(dest, src, (UIntPtr)count);
        }

        public static IntPtr memmove(byte* dest, byte* src, int count)
        {
            return memmove((IntPtr)dest, (IntPtr)src, count);
        }
    }



    [SuppressUnmanagedCodeSecurity]
    public unsafe static class UnsafeNativeMethods
    {
        public const string dllname = "libLimeVideoSDKNativex64"; // must have lib prefix for Linux, keep it!

        [DllImport(dllname, EntryPoint = "prefix_MFXInit")]
        public static extern mfxStatus MFXInit(mfxIMPL impl, mfxVersion* ver, mfxSession* session);

        [DllImport(dllname, EntryPoint = "prefix_MFXQueryIMPL")]
        public static extern mfxStatus MFXQueryIMPL(mfxSession session, mfxIMPL* impl);

        [DllImport(dllname, EntryPoint = "prefix_MFXQueryVersion")]
        public static extern mfxStatus MFXQueryVersion(mfxSession session, mfxVersion* version);

        [DllImport(dllname, EntryPoint = "prefix_MFXClose")]
        public static extern mfxStatus MFXClose(mfxSession session);
        [DllImport(dllname, EntryPoint = "prefix_MFXVideoENCODE_Init")]
        public static extern mfxStatus MFXVideoENCODE_Init(mfxSession session, mfxVideoParam* version);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoENCODE_Query")]
        public static extern mfxStatus MFXVideoENCODE_Query(mfxSession session, mfxVideoParam* inin, mfxVideoParam* outout);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoDECODE_Reset")]
        public static extern mfxStatus MFXVideoDECODE_Reset(mfxSession session, mfxVideoParam* version);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoDECODE_Init")]
        public static extern mfxStatus MFXVideoDECODE_Init(mfxSession session, mfxVideoParam* version);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoDECODE_DecodeHeader")]
        public static extern mfxStatus MFXVideoDECODE_DecodeHeader(mfxSession session, mfxBitstream* bs, mfxVideoParam* par);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoDECODE_QueryIOSurf")]
        public static extern mfxStatus MFXVideoDECODE_QueryIOSurf(mfxSession session, mfxVideoParam* par, mfxFrameAllocRequest* request);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoDECODE_DecodeFrameAsync")]
        public static extern mfxStatus MFXVideoDECODE_DecodeFrameAsync(mfxSession session, mfxBitstream* bs, mfxFrameSurface1* surface_work, mfxFrameSurface1** surface_out, mfxSyncPoint* syncp);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoCORE_SyncOperation")]
        public static extern mfxStatus MFXVideoCORE_SyncOperation(mfxSession session, mfxSyncPoint syncp, UInt32 wait);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoVPP_Init")]
        public static extern mfxStatus MFXVideoVPP_Init(mfxSession session, mfxVideoParam* version);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoENCODE_GetVideoParam")]
        public static extern mfxStatus MFXVideoENCODE_GetVideoParam(mfxSession session, mfxVideoParam* version);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoVPP_RunFrameVPPAsync")]
        public static extern mfxStatus MFXVideoVPP_RunFrameVPPAsync(mfxSession session, mfxFrameSurface1* insurf, mfxFrameSurface1* outsurf, mfxExtVppAuxData* aux, mfxSyncPoint* syncp);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoVPP_RunFrameVPPAsyncEx")]
        public static extern mfxStatus MFXVideoVPP_RunFrameVPPAsyncEx(mfxSession session, mfxFrameSurface1* insurf, mfxFrameSurface1* work, mfxFrameSurface1** outsurf, mfxSyncPoint* syncp);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoENCODE_EncodeFrameAsync")]
        public static extern mfxStatus MFXVideoENCODE_EncodeFrameAsync(mfxSession session, mfxEncodeCtrl* ctrl, mfxFrameSurface1* surface, mfxBitstream* bs, mfxSyncPoint* syncp);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoENCODE_QueryIOSurf")]
        public static extern mfxStatus MFXVideoENCODE_QueryIOSurf(mfxSession session, mfxVideoParam* par, mfxFrameAllocRequest* request);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoVPP_QueryIOSurf")]
        public static extern mfxStatus MFXVideoVPP_QueryIOSurf(mfxSession session, mfxVideoParam* par, mfxFrameAllocRequest* request);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoVPP_GetVideoParam")]
        public static extern mfxStatus MFXVideoVPP_GetVideoParam(mfxSession session, mfxVideoParam* par);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoCORE_GetHandle")]
        public static extern mfxStatus MFXVideoCORE_GetHandle(mfxSession session, mfxHandleType type, IntPtr* hdl);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoCORE_SetHandle")]
        public static extern mfxStatus MFXVideoCORE_SetHandle(mfxSession session, mfxHandleType type, IntPtr hdl);

        [DllImport(dllname, EntryPoint = "prefix_MFXVideoCORE_SetFrameAllocator")]
        public static extern mfxStatus MFXVideoCORE_SetFrameAllocator(mfxSession session, mfxFrameAllocator* allocator);
    }
}
