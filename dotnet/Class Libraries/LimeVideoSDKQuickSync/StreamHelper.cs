// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LimeVideoSDK.QuickSync
{
    /// <summary>
    /// Helper methods for System.IO.Stream
    /// </summary>
    public static class StreamHelper
    {
        /// <summary>
        /// Turn a Stream into an IEnumerable for bytes
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IEnumerable<byte> StreamAsIEnumerable(Stream stream)
        {
            while (true)
            {
                int i = stream.ReadByte();
                if (i == -1)
                    break;

                yield return (byte)i;
            }
        }

       /// <summary>
        /// Turn a stream into an IEnumerable for raw frames
       /// </summary>
       /// <param name="stream"></param>
       /// <param name="bytesPerFrame"></param>
       /// <param name="reuseOutputBuffer"></param>
       /// <returns></returns>
        public static IEnumerable<byte[]> StreamAsFrameSource(Stream stream, int bytesPerFrame, bool reuseOutputBuffer=false)
        {
            var buf = new byte[bytesPerFrame];

            while (true)
            {
                int i = stream.Read(buf, 0, bytesPerFrame);
                if (i != bytesPerFrame)
                    break;



                if (reuseOutputBuffer)
                    yield return buf;
                else
                    yield return (byte[])buf.Clone();
            }
          
        }
    }
}
