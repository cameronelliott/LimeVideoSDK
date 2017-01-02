// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LimeVideoSDK.QuickSyncTypes
{

    /// <summary>
    /// These define a number of four byte codes that define video frame color space and
    /// memory layout characteristics. Otherwise know as FOURCC codes.
    /// </summary>
    public enum FourCC : uint
    {
        NV12 = 842094158u,
        UYVY = 1498831189u,
        YV12 = 842094169u,
        P411 = 842150992u,
        P422 = 825308240u,
        I420_IYUV = 808596553u,
        I420 = 808596553u,
        IYUV = 808596553u,
        YUY2 = 844715353u,
        RGB3 = 859981650u,
        RGB4 = 876758866u,
        BGR4 = 877807426u,
        BGR3 = 861030210u,
        //  NV16 = 909203022u,
        //P8 = 41u,
        //P8_TEXTURE = 1112356944u,
        //P010 = 808530000u,
        //P210 = 808530512u,
        //A2RGB10 = 808535890u,
        //ARGB16 = 909199186u,
        //R16 = 1429614930u,
    }
}
