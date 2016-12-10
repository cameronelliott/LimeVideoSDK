// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Collections.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace LimeVideoSDKQuickSync
{





    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public unsafe struct EncoderShared
    {
        public Int32 safety;
        public mfxSession session;              // input only
        public mfxVideoParam mfxEncParams;      // input only
        public mfxFrameSurface1** pmfxSurfaces; // out only
        public UInt16 nEncSurfNum;              // out only
        public void* buf;                       // out only
        public Int32 buflen;                      // out only
        public Int32 maxbuflen;                     // out only
        public fixed UInt64 warningFuncName[100];   // out only
        public fixed Int32 warningMfxStatus[100];   // out only
        public UInt32 warningCount;                  // out only
        //public bool rendering;
        //public bool useVideoMemory;        
    }


    public unsafe class LowLevelEncoderNative: ILowLevelEncoder
    {
        public bool disableWarningsDump = false;
        public List<Tuple<string, mfxStatus>> warnings = new List<Tuple<string, mfxStatus>>();


        IntPtr h;
        mfxSession _session;
        IntPtr[] frameIntPtrs;
        EncoderShared* shared;
        bool flush1state = true;

        

        unsafe public LowLevelEncoderNative(mfxVideoParam mfxEncParams, mfxIMPL impl)
        {
            mfxStatus sts;
            this._session = new mfxSession();
            var ver = new mfxVersion() { Major = 1, Minor = 3 };
            fixed (mfxSession* s = &_session)
                sts = UnsafeNativeMethods.MFXInit(impl, &ver, s);
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(UnsafeNativeMethods.MFXInit));




            h = NativeLLEncoderUnsafeNativeMethods.NativeEncoder_New();
            Trace.Assert(h != IntPtr.Zero);
            shared = (EncoderShared*)h;
            shared->session = _session;
            shared->mfxEncParams = mfxEncParams;
            Trace.Assert(shared->safety == sizeof(EncoderShared));





            sts = NativeLLEncoderUnsafeNativeMethods.NativeEncoder_Init(h);


            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLEncoderUnsafeNativeMethods.NativeEncoder_Init));

            frameIntPtrs = new IntPtr[shared->nEncSurfNum];
            for (int i = 0; i < frameIntPtrs.Length; i++)
            {
                frameIntPtrs[i] = (IntPtr)shared->pmfxSurfaces[i];
            }

            GetAndPrintWarnings();

        }


        private void GetAndPrintWarnings()
        {
            if (disableWarningsDump)
                return;

            for (int i = 0; i < shared->warningCount; i++)
            {
                string s = Marshal.PtrToStringAnsi((IntPtr)shared->warningFuncName[i]);
                var sts = (mfxStatus)shared->warningMfxStatus[i];
                var t = new Tuple<string, mfxStatus>(s, sts);
                warnings.Add(t);
            }
            shared->warningCount = 0;

            warnings.ForEach(z =>
           Console.WriteLine("Warning: {0} {1} {2}", this.GetType().FullName, z.Item1, z.Item2)
           );

            warnings.Clear();
        }

        public IntPtr[] Frames
        {
            get
            {
                return frameIntPtrs;
            }
        }

        public mfxSession session
        {
            get
            {
                return _session;
            }
        }



        public unsafe void EncodeFrame(int frameIndex, ref BitStreamChunk bitstreamChunk)
        {
            bitstreamChunk.bytesAvailable = 0;
            var a = frameIntPtrs[frameIndex];
            var sts = NativeLLEncoderUnsafeNativeMethods.NativeEncoder_EncodeFrame(h, (mfxFrameSurface1*)a);
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLEncoderUnsafeNativeMethods.NativeEncoder_EncodeFrame));

            CopyOutBitstream(ref bitstreamChunk);
        }

        private void CopyOutBitstream(ref BitStreamChunk bitstreamChunk)
        {
            if (bitstreamChunk.bitstream == null)
                bitstreamChunk.bitstream = new byte[shared->maxbuflen];

            if (shared->buflen > 0)
                Marshal.Copy((IntPtr)shared->buf, bitstreamChunk.bitstream, 0, shared->buflen);
            bitstreamChunk.bytesAvailable = shared->buflen;
        }

        bool Flush1(ref BitStreamChunk bitstreamChunk)
        {
            bitstreamChunk.bytesAvailable = 0;
            var sts = NativeLLEncoderUnsafeNativeMethods.NativeEncoder_Flush1(h);
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLEncoderUnsafeNativeMethods.NativeEncoder_Flush1));

            CopyOutBitstream(ref bitstreamChunk);
            return true;
        }

        bool Flush2(ref BitStreamChunk bitstreamChunk)
        {
            bitstreamChunk.bytesAvailable = 0;
            var sts = NativeLLEncoderUnsafeNativeMethods.NativeEncoder_Flush2(h);
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLEncoderUnsafeNativeMethods.NativeEncoder_Flush2));

            CopyOutBitstream(ref bitstreamChunk);
            return true;
        }
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

        public int GetFreeFrameIndex()
        {
            int i = NativeLLEncoderUnsafeNativeMethods.NativeEncoder_GetFreeFrameIndex(h);
            if (i < 0)
                QuickSyncStatic.ThrowOnBadStatus((mfxStatus)i, nameof(NativeLLEncoderUnsafeNativeMethods.NativeEncoder_GetFreeFrameIndex));
            return i;
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

                NativeLLEncoderUnsafeNativeMethods.NativeEncoder_Delete(h);
                h = IntPtr.Zero;
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LowLevelEncoder2() {
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



        public void LockFrame(IntPtr frame)
        {
            var sts = NativeLLEncoderUnsafeNativeMethods.NativeEncoder_LockFrame(h, frame);
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLEncoderUnsafeNativeMethods.NativeEncoder_LockFrame));
        }

        public void UnlockFrame(IntPtr frame)
        {
            var sts = NativeLLEncoderUnsafeNativeMethods.NativeEncoder_UnlockFrame(h, frame);
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLEncoderUnsafeNativeMethods.NativeEncoder_UnlockFrame));
        }
        #endregion
    }


    [SuppressUnmanagedCodeSecurity]
    public unsafe static class NativeLLEncoderUnsafeNativeMethods
    {


        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern IntPtr NativeEncoder_New();

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern void NativeEncoder_Delete(IntPtr handle);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeEncoder_Flush1(IntPtr handle);
        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeEncoder_Flush2(IntPtr handle);
        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeEncoder_EncodeFrame(IntPtr handle, mfxFrameSurface1* frame);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeEncoder_LockFrame(IntPtr handle, IntPtr frame);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeEncoder_UnlockFrame(IntPtr handle, IntPtr frame);


        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern mfxStatus NativeEncoder_Init(IntPtr handle);

        [DllImport(UnsafeNativeMethods.dllname)]
        public static extern int NativeEncoder_GetFreeFrameIndex(IntPtr handle);




    }
}