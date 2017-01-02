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
    /// This class is a 'do-nothing' write-only System.IO.Stream implementation.
    /// It is used simply for benchmarking purposes only.
    /// </summary>
    public class NullStream : Stream
    {
        long p = 0;

        public override void Close() { }


        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                return p;
            }

            set
            {
                p = value;
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            p += count;
        }
    }
}
