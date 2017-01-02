// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using LimeVideoSDK.QuickSyncTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


namespace LimeVideoSDK.QuickSync
{
    /// <summary>
    /// Video helper methods.
    /// </summary>
    static public class VideoUtility
    {

        /// <summary>
        /// Find the number of bits per pixel of a FourCC
        /// </summary>
        /// <param name="fourcc">Fourcc in question</param>
        /// <returns>Number of bits per pixel</returns>
        public static int GetBitsPerPixel(FourCC fourcc)
        {
            switch (fourcc)
            {
                case FourCC.UYVY: // siblings
                case FourCC.YUY2: // siblings
                case FourCC.P422:
                    return 16;
                case FourCC.NV12:
                case FourCC.YV12:
                case FourCC.I420_IYUV:
                case FourCC.P411:
                    return 12;
                case FourCC.RGB4:
                case FourCC.BGR4:
                    return 32;
                case FourCC.RGB3:
                case FourCC.BGR3:
                    return 24;
            }

            throw new Exception("Contact support: BitsPerPixel not available for " + fourcc.ToString());
        }

        /// <summary>This function can be used to compute the pitch for a FourCC.</summary>
        /// <param name="fourCC">The FourCC</param>
        /// <returns></returns>
        /// <exception cref="Exception">Contact support: PackedPitchmultiplier() not available for " + fourCC.ToString()</exception>
        public static int GetPackedPitchMultiplier(FourCC fourCC)
        {
            switch (fourCC)
            {
                case FourCC.UYVY: return 2;
                case FourCC.RGB4: return 4;
                case FourCC.RGB3: return 3;
                case FourCC.BGR4: return 4;
                case FourCC.BGR3: return 3;
                case FourCC.YUY2: return 2;
                case FourCC.YV12: return 1;
                case FourCC.I420_IYUV: return 1;
                case FourCC.P411: return 1;
                case FourCC.P422: return 1;
                case FourCC.NV12: return 1;
            }

            throw new Exception("Contact support: PackedPitchmultiplier() not available for " + fourCC.ToString());
        }
    }
}
