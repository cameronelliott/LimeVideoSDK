// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;

namespace LimeVideoSDKQuickSync
{
    public interface ILowLevelEncoder : IDisposable
    {

        void EncodeFrame(int frameIndex, ref BitStreamChunk bitStreamChunk);
        bool Flush(ref BitStreamChunk bitstreamChunk);
        int GetFreeFrameIndex();

        mfxSession session { get; }

        void LockFrame(IntPtr frame);
        void UnlockFrame(IntPtr frame);

        IntPtr[] Frames { get; }
        //  object deviceSetup { get; }
    }
}