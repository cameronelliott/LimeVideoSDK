// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using LimeVideoSDK.QuickSyncTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// see the docs about the two decoder types
using LowLevelDecoder = LimeVideoSDK.QuickSync.LowLevelDecoderCSharp;
//using LowLevelDecoder = LimeVideoSDK.QuickSync.LowLevelDecoderNative;


namespace LimeVideoSDK.QuickSync
{
    /// <summary>
    /// Stream based decoder
    /// </summary>
    unsafe public class StreamDecoder : IDisposable
    {
        /// <summary>The low level decoder</summary>
        public ILowLevelDecoder lowLevelDecoder;
        /// <summary>Gets the crop width of frames returned</summary>
        public int width { get { return decoderParameters.mfx.FrameInfo.CropW; } }
        /// <summary>Gets the crop height of frames returned</summary>
        public int height { get { return decoderParameters.mfx.FrameInfo.CropH; } }
        /// <summary>The benchmark never stop mode</summary>
        public bool benchmarkNeverStopMode = false;


        byte[] inbuf;
        mfxVideoParam decoderParameters;
        Stream stream;

        //static public IEnumerable<byte[]> DecodeStream(Stream s, FourCC fourcc = FourCC.NV12, AccelerationLevel acceleration = AccelerationLevel.BestAvailableAccelerationUseGPUorCPU)
        //{
        //    return null;
        //}


        /// <summary>
        /// Construct the decoder.
        /// </summary>
        /// <param name="stream">Stream be read from</param>
        /// <param name="codecId">What format the bitstream is in: AVC, HEVC, MJPEG, ...</param>
        /// <param name="impl">implementation to use</param>
        /// <param name="outIOPattern">memory type for decoding</param>
        public StreamDecoder(Stream stream, CodecId codecId, mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO, IOPattern outIOPattern = IOPattern.MFX_IOPATTERN_OUT_SYSTEM_MEMORY)
        {
            long oldposition = stream.Position;

            var buf = new byte[65536]; //avail after init
            int n = stream.Read(buf, 0, buf.Length);
            if (n < buf.Length)
                Array.Resize(ref buf, n);

            stream.Position = oldposition;
            this.decoderParameters = QuickSyncStatic.DecodeHeader(buf, codecId, impl);
            this.decoderParameters.IOPattern = outIOPattern;
            
            lowLevelDecoder = new LowLevelDecoder(decoderParameters, null, impl);
            Init(stream);
        }

        /// <summary>Initializes a new instance of the <see cref="StreamDecoder"/> class.
        /// Fully specify decode params, and optionally VPP params</summary>
        /// <param name="stream">The stream.</param>
        /// <param name="decodeParameters">The decode parameters.</param>
        /// <param name="mfxVPPParams">The MFX VPP parameters.</param>
        /// <param name="impl">The implementation.</param>
        public StreamDecoder(Stream stream, mfxVideoParam decodeParameters, mfxVideoParam? mfxVPPParams = null, mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO)
        {
            this.decoderParameters = decodeParameters;
            lowLevelDecoder = new LowLevelDecoder(decodeParameters, mfxVPPParams, impl);
            Init(stream);
        }

        void Init(Stream stream)
        {
            this.stream = stream;

            inbuf = new byte[lowLevelDecoder.GetInternalBitstreamBufferFree()];
        }

        /// <summary>
        /// This resets the decoder, usually used after seeking or re-positioning the bitstream
        ///  Decoder will enter seek for PPS/SPS
        /// </summary>
        public void Reset()
        {
            lowLevelDecoder.Reset(decoderParameters);
        }



        /// <summary>
        /// Get frames from the decoder
        /// </summary>
        /// <returns>Frames enumerable</returns>
        public IEnumerable<mfxFrameSurface1> GetFrames()
        {
            long maximumToRead = -1;
            // int framelen = converter.outbuf.Length;
            mfxFrameSurface1? frame;

            long nread = 0;
            while (true)
            {
                bool notMoreData = lowLevelDecoder.DecodeFrame(out frame);
                if (frame.HasValue)
                    yield return frame.Value;

                if (!notMoreData)
                {
                    int free = lowLevelDecoder.GetInternalBitstreamBufferFree();
                    //int free = (int)(lowLevelDecoder.bitstream.MaxLength - lowLevelDecoder.bitstream.DataLength);
                    int toread = Math.Min(free, inbuf.Length);

                    if (maximumToRead != -1 && maximumToRead - nread < toread)
                        toread = (int)(maximumToRead - nread);

                    int r = stream.Read(inbuf, 0, toread);
                    if (r == 0)// Eof??
                    {
                        if (benchmarkNeverStopMode)
                        {
                            stream.Position = 0;
                            lowLevelDecoder.ClearBitstream();
                        }
                        else
                            break;
                    }

                    nread += r;

                    lowLevelDecoder.PutBitstream(inbuf, 0, r);
                }
            }


            while (lowLevelDecoder.DecodeFrame(out frame))
                if (frame.HasValue)
                    yield return frame.Value;

            while (lowLevelDecoder.Flush1(out frame))
                if (frame.HasValue)
                    yield return frame.Value;

            while (lowLevelDecoder.Flush2(out frame))
                if (frame.HasValue)
                    yield return frame.Value;
        }



        /// <summary>
        /// dispose
        /// </summary>
        public void Dispose()
        {
            if (lowLevelDecoder != null)
            {
                lowLevelDecoder.Dispose();
                lowLevelDecoder = null;
            }
        }
    }
}
