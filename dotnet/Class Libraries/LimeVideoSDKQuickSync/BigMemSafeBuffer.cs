// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LimeVideoSDKQuickSync
{
    /// <summary>
    /// This is a helper class to enable <see cref="System.IO.UnmanagedMemoryStream"/>  to read from 
    /// files larger than 2GB
    /// </summary>
    public class BigMemSafeBuffer : SafeBuffer, IDisposable
    {
        /// <summary/>
        /// <param name="length">Number of bytes to allocate.</param>
        public BigMemSafeBuffer(long length)
            : base(true)
        {
            handle = Marshal.AllocHGlobal((IntPtr)length);
            Trace.Assert(handle != IntPtr.Zero);
            Initialize((ulong)length);
        }

        /// <summary>
        /// Necessary for SafeBuffer
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero)
                Marshal.FreeHGlobal(handle);
            return true;
        }
    }
}
