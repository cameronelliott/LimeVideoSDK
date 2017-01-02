// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using LimeVideoSDK.Benchmark;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LimeVideoSDK.QuickSync
{


    /// <summary>
    /// Class PreReadLargeMemoryStream.
    /// This class can be used to pre-read an entire stream into memory
    /// for benchmarking purposes.
    /// It uses unmanaged memory in order to exceed the 2GB array limit of .NET runtimes.
    /// </summary>
    /// <seealso cref="System.IO.UnmanagedMemoryStream" />
    public class PreReadLargeMemoryStream : UnmanagedMemoryStream
    {
               /// <summary>
        /// Initializes a new instance of the <see cref="PreReadLargeMemoryStream"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public PreReadLargeMemoryStream(Stream stream)
           : base(new BigMemSafeBuffer(stream.Length), 0, stream.Length, FileAccess.ReadWrite)
        {
            Trace.Assert(stream.CanRead);
            stream.CopyTo(this);
            this.Position = 0;
        }     

        /// <summary>
        /// Initializes a new instance of the <see cref="PreReadLargeMemoryStream"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="maximumOutputLength">Maximum length of the output.</param>
        public PreReadLargeMemoryStream(Stream stream, long maximumOutputLength)
            : base(new BigMemSafeBuffer(maximumOutputLength), 0, maximumOutputLength, FileAccess.ReadWrite)
        {
            Trace.Assert(stream.CanWrite);
            Trace.Assert(stream.Position == 0);

            this.Position = 0;
            
            if (stream.CanRead)
            {
                MyCopyTo(stream, this, maximumOutputLength);
                stream.Position = 0;
            }
            else if (stream.CanWrite)
            {
               
                MyCopyTo(stream, this, maximumOutputLength);
                stream.Position = 0;
            }

            this.Position = 0;
        }

        static void MyCopyTo(Stream source, Stream destination,  long max,int bufferSize=256*256)
        {
            byte[] buffer = new byte[bufferSize];
            int read;
            long totalread = 0;
            while (true)
            {
                read = source.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                    break;
                int n = (int)Math.Min((long)read, max - totalread);
                destination.Write(buffer, 0, n);
                totalread += n;
                if (totalread >= max)
                    break;
            }
        }


        static long FileLen(string path)
        {
            return (new FileInfo(path)).Length;
        }

#if false
        // this is ifdef'd out because I believe after some use of this
        // class, it shouldn't open the files, but only take Streams as constructors.
        //
        /// <summary>
        /// Load file from path into memory
        /// </summary>
        public PreReadLargeMemoryStream(string path)
            : base(new BigMemSafeBuffer(FileLen(path)), 0, FileLen(path), FileAccess.ReadWrite)
        {
            using (var s = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                s.CopyTo(this);
            }
            this.Position = 0;
        }
#endif
    }

}
