// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;



namespace LimeVideoSDKQuickSync
{


    /// <summary>
    /// Stream based transcoder
    /// </summary>
    public class StreamTranscoder : IDisposable
    {
        /// <summary>The low level transcoder</summary>
        public LowLevelTranscoderCSharp lowLevelTranscoder;


        Stream inStream;    
        TranscoderConfiguration config;


        /// <summary>Initializes a new instance of the <see cref="StreamTranscoder"/> class.</summary>
        /// <param name="inStream">The in stream.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="impl">The implementation.</param>
        /// <param name="forceSystemMemory">if set to <c>true</c> [force system memory].</param>
        public StreamTranscoder(Stream inStream, TranscoderConfiguration config, mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO, bool forceSystemMemory = false)
        {
            this.config = config;
            this.inStream = inStream;
            lowLevelTranscoder = new LowLevelTranscoderCSharp(config, impl, forceSystemMemory);
            //lowLevelTranscoder = new LowLevelTranscoderVidMemSysMem(config, impl, forceSystemMemory);
        }

        /// <summary>Return the next frame available</summary>
        public IEnumerable<BitStreamChunk> GetFrames()
        {


            var inbuf = new byte[lowLevelTranscoder.BufferSize];
            BitStreamChunk bsc = new BitStreamChunk();

            while (true)
            {
                if (lowLevelTranscoder.BufferFreeCount > lowLevelTranscoder.BufferSize / 2)
                {
                    int r = inStream.Read(inbuf, 0, Math.Min((int)inbuf.Length, lowLevelTranscoder.BufferFreeCount));
                    if (r <= 0)
                        break;

                    lowLevelTranscoder.PutBitstream(inbuf, 0, r);

                }

                lowLevelTranscoder.GetNextFrame(ref bsc);
                if (bsc.bytesAvailable > 0)
                    yield return bsc;
            }

            while (lowLevelTranscoder.GetNextFrame(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                    yield return bsc;
            }

            while (lowLevelTranscoder.Flush1(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                    yield return bsc;
            }

            while (lowLevelTranscoder.Flush2(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                    yield return bsc;
            }

            while (lowLevelTranscoder.Flush3(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                    yield return bsc;
            }

            while (lowLevelTranscoder.Flush4(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                    yield return bsc;
            }
        }

#region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    lowLevelTranscoder.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~StreamTranscoder() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
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
