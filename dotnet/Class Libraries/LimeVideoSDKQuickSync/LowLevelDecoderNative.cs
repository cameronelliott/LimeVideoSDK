// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment warnings


//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LimeVideoSDKQuickSync
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct DecoderShared
    {
        [FieldOffset(0)]
        public Int32 safety;
        [FieldOffset(8)]
        public mfxBitstream mfxBS;
        [FieldOffset(8 + 72)]
        public fixed UInt64 warningFuncName[100];   // out only
        [FieldOffset(8 + 72 + 8 * 100)]
        public fixed Int32 warningMfxStatus[100];   // out only
        [FieldOffset(8 + 72 + 8 * 100 + 4 * 100)]
        public UInt32 warningCount;                  // out only
      
    };

    unsafe public class LowLevelDecoderNative : ILowLevelDecoder
    {
        IntPtr h;
        // public DeviceSetup deviceSetup;
        public mfxSession session;

        public bool disableWarningsDump = false;
        public List<Tuple<string, mfxStatus>> warnings = new List<Tuple<string, mfxStatus>>();

        DecoderShared* shared;


        public LowLevelDecoderNative(mfxVideoParam mfxDecParamsX,
          mfxVideoParam? VPPParamsX = null,
          mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO)
        {


            mfxVideoParam tmpMfxVideoParam;

            if (VPPParamsX.HasValue)
            {
                tmpMfxVideoParam = VPPParamsX.Value;
            }
            else
            {
                tmpMfxVideoParam.AsyncDepth = 1;
                tmpMfxVideoParam.IOPattern = IOPattern.MFX_IOPATTERN_IN_VIDEO_MEMORY | IOPattern.MFX_IOPATTERN_OUT_VIDEO_MEMORY                ;
                tmpMfxVideoParam.vpp.In = mfxDecParamsX.mfx.FrameInfo;
                tmpMfxVideoParam.vpp.Out = mfxDecParamsX.mfx.FrameInfo;
            }


            mfxStatus sts;

            session = new mfxSession();
            var ver = new mfxVersion() { Major = 1, Minor = 3 };
            fixed (mfxSession* s = &session)
                sts = UnsafeNativeMethods.MFXInit(impl, &ver, s);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXInit");
            //deviceSetup = new DeviceSetup(session, false);

            h = NativeLLDecoderUnsafeNativeMethods.NativeDecoder_New();
            Trace.Assert(h != IntPtr.Zero);


            shared = (DecoderShared*)h;
            //Console.WriteLine("mfxbs offset in C# {0}", (UInt64)(&(shared->mfxBS)) - (UInt64)shared);
            //Console.WriteLine("warningCount offset in C# {0}", (UInt64)(&(shared->warningCount)) - (UInt64)shared);
            //Console.WriteLine("sizeof(mfxBitstream) {0}", sizeof(mfxBitstream));
            //Console.WriteLine("sizeof(DecoderShared) {0}", sizeof(DecoderShared));
            //Console.WriteLine("shared->safety {0}", shared->safety);

            Trace.Assert(shared->safety == sizeof(DecoderShared));

            shared->mfxBS.MaxLength = 1000000;
            shared->mfxBS.Data = Marshal.AllocHGlobal((int)shared->mfxBS.MaxLength);
            shared->mfxBS.DataLength = 0;
            shared->mfxBS.DataOffset = 0;





            sts = NativeLLDecoderUnsafeNativeMethods.NativeDecoder_Init(h, session, &mfxDecParamsX, &tmpMfxVideoParam);
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLDecoderUnsafeNativeMethods.NativeDecoder_Init));
        

            //mfxFrameSurface1 aaa = *shared->foo1[0];
            //aaa.Data = new mfxFrameData();
            //File.WriteAllText("\\x\\a", Newtonsoft.Json.JsonConvert.SerializeObject(aaa,Formatting.Indented));

            // aaa = *shared->foo2[0];
            //aaa.Data = new mfxFrameData();
            //File.WriteAllText("\\x\\b", Newtonsoft.Json.JsonConvert.SerializeObject(aaa, Formatting.Indented));

            //aaa = *shared->foo3[0];
            //aaa.Data = new mfxFrameData();
            //File.WriteAllText("\\x\\c", Newtonsoft.Json.JsonConvert.SerializeObject(aaa, Formatting.Indented));

            //aaa = *shared->foo4[0];
            //aaa.Data = new mfxFrameData();
            //File.WriteAllText("\\x\\d", Newtonsoft.Json.JsonConvert.SerializeObject(aaa, Formatting.Indented));



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

        public void ClearBitstream()
        {
            var sts = NativeLLDecoderUnsafeNativeMethods.NativeDecoder_ClearBitstream(h);
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLDecoderUnsafeNativeMethods.NativeDecoder_ClearBitstream));
        }

        public bool DecodeFrame(out mfxFrameSurface1? frame)
        {
            mfxFrameSurface1* p = null;
            frame = null;
            var sts = NativeLLDecoderUnsafeNativeMethods.NativeDecoder_DecodeFrame(h, &p);
            if (sts == mfxStatus.MFX_ERR_MORE_SURFACE) // decoder needs to be called again, it is eating memory.SWmode
                return true;
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLDecoderUnsafeNativeMethods.NativeDecoder_DecodeFrame));
            if (p != null)
                frame = *p;
            return true;

        }



        public bool Flush1(out mfxFrameSurface1? frame)
        {
            mfxFrameSurface1* p = null;
            frame = null;
            var sts = NativeLLDecoderUnsafeNativeMethods.NativeDecoder_Flush1(h, &p);
            if (sts == mfxStatus.MFX_ERR_MORE_SURFACE) // decoder needs to be called again, it is eating memory.SWmode
                return true;
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLDecoderUnsafeNativeMethods.NativeDecoder_Flush1));
            if (p != null)
                frame = *p;
            return true;
        }

        public bool Flush2(out mfxFrameSurface1? frame)
        {
            mfxFrameSurface1* p = null;
            frame = null;
            var sts = NativeLLDecoderUnsafeNativeMethods.NativeDecoder_Flush2(h, &p);
            if (sts == mfxStatus.MFX_ERR_MORE_SURFACE) // decoder needs to be called again, it is eating memory.SWmode
                return true;
            if (sts == mfxStatus.MFX_ERR_MORE_DATA)
                return false;
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLDecoderUnsafeNativeMethods.NativeDecoder_Flush2));
            if (p != null)
                frame = *p;
            return true;
        }

        public int GetInternalBitstreamBufferFree()
        {
            return (int)shared->mfxBS.MaxLength - (int)shared->mfxBS.DataLength;
            //return NativeLLDecoderUnsafeNativeMethods.NativeDecoder_GetInternalBitstreamBufferFree(h);
        }


        //public void PutBitstream(byte[] inbuf, int offset, int length)
        //{
        //    var sts = NativeLLDecoderUnsafeNativeMethods.NativeDecoder_PutBitstream(h, inbuf, offset, length);
        //    QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLDecoderUnsafeNativeMethods.NativeDecoder_PutBitstream));
        //}
        public void PutBitstream(byte[] inbuf, int offset, int length)
        {

            FastMemcpyMemmove.memmove(shared->mfxBS.Data, shared->mfxBS.Data + (int)shared->mfxBS.DataOffset, (int)shared->mfxBS.DataLength);
            shared->mfxBS.DataOffset = 0;

            int free = (int)(shared->mfxBS.MaxLength - shared->mfxBS.DataLength);
            //Trace.Assert(length <= free);
            if (free < length)
                throw new LimeVideoSDKQuickSyncException("insufficient space in buffer");

            Marshal.Copy(inbuf, offset, shared->mfxBS.Data + (int)shared->mfxBS.DataLength, length);

            shared->mfxBS.DataLength += (uint)length;
        }

        public void ClearBitstreamBuffer()
        {
            shared->mfxBS.DataLength = 0;
            shared->mfxBS.DataOffset = 0;

        }



        public void Reset(mfxVideoParam p)
        {
            var sts = NativeLLDecoderUnsafeNativeMethods.NativeDecoder_Reset(h, &p);
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLDecoderUnsafeNativeMethods.NativeDecoder_Reset));
        }


        public void LockFrame(IntPtr frame)
        {
            var sts = NativeLLDecoderUnsafeNativeMethods.NativeDecoder_LockFrame(h, frame);
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLEncoderUnsafeNativeMethods.NativeEncoder_LockFrame));
        }

        public void UnlockFrame(IntPtr frame)
        {
            var sts = NativeLLDecoderUnsafeNativeMethods.NativeDecoder_UnlockFrame(h, frame);
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(NativeLLEncoderUnsafeNativeMethods.NativeEncoder_UnlockFrame));
        }



        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        mfxSession ILowLevelDecoder.session
        {
            get
            {
                return session;
            }
        }

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

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NativeLLDecoder() {
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
}
