// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using LimeVideoSDK.QuickSyncTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LimeVideoSDK.QuickSync
{
    /// <summary>
    /// This can be used to write mfxFrameSurface1 in NV12 format to disk.
    /// </summary>
    public class NV12FrameWriter
    {
        byte[] row;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width"></param>
        public NV12FrameWriter(int width)
        {
            row = new byte[width];
        }


        /// <summary>
        /// If a valid frame is in frame, the raw data (Y+UV) will be written to outbs
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="outbs"></param>
        public void WriteFrameNV12(mfxFrameSurface1 frame, Stream outbs)
        {
            var y = frame.Data.Y;
            for (int i = 0; i < frame.Info.CropH; i++)
            {
                Marshal.Copy(y + i * frame.Data.Pitch, row, 0, row.Length);
                outbs.Write(row, 0, row.Length);
            }
            //   IntPtr o = (IntPtr)frame.ptr + frame.Info.Height * frame.Data.Pitch;
            IntPtr uv = frame.Data.UV;
            for (int i = 0; i < frame.Info.CropH / 2; i++)
            {
                Marshal.Copy(uv + i * frame.Data.Pitch, row, 0, row.Length);
                outbs.Write(row, 0, row.Length);
            }
        }
    }
}
