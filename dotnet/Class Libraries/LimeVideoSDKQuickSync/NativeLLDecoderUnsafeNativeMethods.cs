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
    [SuppressUnmanagedCodeSecurity]
    public unsafe static class NativeLLDecoderUnsafeNativeMethods
    {


        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern IntPtr NativeDecoder_New();

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern void NativeDecoder_Delete(IntPtr handle);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeDecoder_Init(IntPtr handle, mfxSession session, mfxVideoParam* mfxDecParamsX,
  mfxVideoParam* VPPParamsX);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern int NativeDecoder_GetInternalBitstreamBufferFree(IntPtr handle);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeDecoder_ClearBitstream(IntPtr handle);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeDecoder_DecodeFrame(IntPtr handle, mfxFrameSurface1** frame);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeDecoder_Flush1(IntPtr handle, mfxFrameSurface1** frame);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeDecoder_Flush2(IntPtr handle, mfxFrameSurface1** frame);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeDecoder_PutBitstream(IntPtr handle, byte[] inbuf, int offset, int length);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeDecoder_Reset(IntPtr handle, mfxVideoParam* p);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeDecoder_LockFrame(IntPtr h, IntPtr frame);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeDecoder_UnlockFrame(IntPtr h, IntPtr frame);
    }


}
