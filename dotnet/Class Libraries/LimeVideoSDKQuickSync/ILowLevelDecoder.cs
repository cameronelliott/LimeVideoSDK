// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace LimeVideoSDKQuickSync
{
    public interface ILowLevelDecoder : IDisposable
    {
        int GetInternalBitstreamBufferFree();
        void ClearBitstream();
        bool DecodeFrame(out mfxFrameSurface1? frame);

        bool Flush1(out mfxFrameSurface1? frame);  // 'out frame' is null if no frame is available!
        bool Flush2(out mfxFrameSurface1? frame);  // 'out frame' is null if no frame is available!

        void PutBitstream(byte[] inbuf, int offset, int length);
        void Reset(mfxVideoParam p);

        void LockFrame(IntPtr memId);
        void UnlockFrame(IntPtr memId);

         mfxSession session { get; }

        VideoAccelerationSupport videoAccelerationSupport { get; }
    }

}