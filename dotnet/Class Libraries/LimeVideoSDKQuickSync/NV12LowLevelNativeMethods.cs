// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace LimeVideoSDKQuickSync
{

    [SuppressUnmanagedCodeSecurity]
    public unsafe static class NV12UnsafeNativeMethods
    {
        public const string dllname = LimeVideoSDKQuickSync.UnsafeNativeMethods.dllname;

        [DllImport(dllname)]
        internal static extern NativeResizeConvertStatus ResizeYUV420GetSize(
            NativeWidthHeight srcSize, NativeWidthHeight dstSize,
            int interpolation, UInt32 antialiasing,
            Int32* pSpecSize, Int32* pInitBufSize);


        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus ResizeYUV420LanczosInit(NativeWidthHeight srcSize, NativeWidthHeight dstSize, UInt32 numLobes, byte* pSpec, byte* pInitBuf);

        [DllImport(dllname)]
        internal static extern
        NativeResizeConvertStatus ResizeYUV420GetBufferSize(byte* pSpec, NativeWidthHeight dstSize, Int32* pBufSize);



        [DllImport(dllname)]
        internal static extern
        NativeResizeConvertStatus ResizeYUV420Lanczos_8u_P2R(
        byte* pSrcY, Int32 srcYStep, byte* pSrcUV, Int32 srcUVStep,
        byte* pDstY, Int32 dstYStep, byte* pDstUV, Int32 dstUVStep,
        NativeXY dstOffset, NativeWidthHeight dstSize,
        UInt32 border, byte* borderValue,
        byte* pSpec, byte* pBuffer);



        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus UYVYToNV12(byte* pSrc, int srcStep, byte* pDstY, int dstYStep, byte* pDstCbCr, int dstCbCrStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus RGB3ToNV12(byte* pRGB, int rgbStep, byte* pY, int YStep, byte* pCbCr, int CbCrStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus RGB4ToNV12(byte* pRGB, int rgbStep, byte* pY, int YStep, byte* pCbCr, int CbCrStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus BGR3ToNV12(byte* pRGB, int rgbStep, byte* pY, int YStep, byte* pCbCr, int CbCrStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus BGR4ToNV12(byte* pRGB, int rgbStep, byte* pY, int YStep, byte* pCbCr, int CbCrStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus P411ToNV12(byte** pSrc, int* srcStep, byte* pDstY, int dstYStep, byte* pDstCbCr, int dstCbCrStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus IYUVToNV12(byte** pSrc, int* srcStep, byte* pDstY, int dstYStep, byte* pDstCbCr, int dstCbCrStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus YUY2ToNV12(byte* pSrc, int srcStep, byte* pDstY, int dstYStep, byte* pDstCbCr, int dstCbCrStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus P422ToNV12(byte** pSrc, int* srcStep, byte* pDstY, int dstYStep, byte* pDstCbCr, int dstCbCrStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus YV12ToNV12(byte** pSrc, int* srcStep, byte* pDstY, int dstYStep, byte* pDstCbCr, int dstCbCrStep, NativeWidthHeight roiSize);


        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus NV12ToUYVY(byte* pSrcY, int srcYStep, byte* pSrcCbCr, int srcCbCrStep, byte* pDst, int dstStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus NV12ToRGB3(byte* pY, int pYStep, byte* pCbCr, int CbCrStep, byte* pRGB, int rgbStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus NV12ToBGR4(byte* pY, int pYStep, byte* pCbCr, int CbCrStep, byte* pBGR, int rgbStep, NativeWidthHeight roiSize, byte aval);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus NV12ToBGR3(byte* pY, int pYStep, byte* pCbCr, int CbCrStep, byte* pRGB, int rgbStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus NV12ToRGB4(byte* pY, int pYStep, byte* pCbCr, int CbCrStep, byte* pRGB, int rgbStep, NativeWidthHeight roiSize, byte aval);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus NV12ToP411(byte* pSrcY, int srcYStep, byte* pSrcCbCr, int srcCbCrStep, byte** pDst, int* dstStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus NV12ToYUY2(byte* pSrcY, int srcYStep, byte* pSrcCbCr, int srcCbCrStep, byte* pDst, int dstStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus NV12ToP422(byte* pSrcY, int srcYStep, byte* pSrcCbCr, int srcCbCrStep, byte** pDst, int* dstStep, NativeWidthHeight roiSize);

        [DllImport(dllname)]
        public static extern
        NativeResizeConvertStatus NV12ToYV12(byte* pSrcY, int srcYStep, byte* pSrcCbCr, int srcCbCrStep, byte** pDst, int* dstStep, NativeWidthHeight roiSize);
    }
}

