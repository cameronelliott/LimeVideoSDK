// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma once


#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <assert.h>

#define _USE_MATH_DEFINES
#include <math.h>
#include <string.h>
//#include <errno.h>
#include <vector>
#include <algorithm>
//#include <tchar.h>
#if defined(_WIN32) || defined(_WIN64)
#include <windows.h>
#else
//#include <sys/param.h>
#define __min min
#define __max max
using std::min;
using std::max;
#endif


#include "_resampler.h"
#include "_ColorConversions.h"
 









typedef struct {
	int width;
	int height;
} StructWidthHeight;

typedef struct {
	int x;
	int y;
} StructXY;

namespace NV12ConvertOrResize {

	extern "C" {

		int NV12ToBGR3(u_char* pSrcY, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstStep, StructWidthHeight roiSize);
		int NV12ToBGR4(u_char* pSrcY, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstStep, StructWidthHeight roiSize, int aval);
		int NV12ToRGB3(u_char* pSrcY, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstStep, StructWidthHeight roiSize);
		int NV12ToRGB4(u_char* pSrcY, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstStep, StructWidthHeight roiSize, int aval);
		int NV12ToYUY2(u_char* pSrc, int srcStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstYStep, StructWidthHeight roiSize);
		int NV12ToYV12(u_char* pSrc, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char** pDst, int* dstStep, StructWidthHeight roiSize);
		int NV12ToUYVY(u_char* pSrc, int srcStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstYStep, StructWidthHeight roiSize);
		int NV12ToIYUV(u_char* pSrc, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char** pDst, int* dstStep, StructWidthHeight roiSize);
		int NV12ToP411(const u_char* pSrcY, int srcYStep, const u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst[3], int dstStep[3], StructWidthHeight roiSize);
		int NV12ToP422(const u_char* pSrcY, int srcYStep, const u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst[3], int dstStep[3], StructWidthHeight roiSize);

		int BGR3ToNV12(const u_char* pRGB, int rgbStep, u_char* pY, int yStep, u_char* pCbCr, int cbCrStep, StructWidthHeight roiSize);
		int BGR4ToNV12(const u_char* pRGB, int rgbStep, u_char* pY, int yStep, u_char* pCbCr, int cbCrStep, StructWidthHeight roiSize);
		int RGB3ToNV12(const u_char* pRGB, int rgbStep, u_char* pY, int yStep, u_char* pCbCr, int cbCrStep, StructWidthHeight roiSize);
		int RGB4ToNV12(const u_char* pRGB, int rgbStep, u_char* pY, int yStep, u_char* pCbCr, int cbCrStep, StructWidthHeight roiSize);
		int P411ToNV12(const u_char* pSrc[3], int srcStep[3], u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize);
		int P422ToNV12(const u_char* pSrc[3], int srcStep[3], u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize);
		int UYVYToNV12(const u_char* pSrc, int srcStep, u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize);
		int IYUVToNV12(const u_char* pSrc[3], int srcStep[3], u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize);
		int YV12ToNV12(const u_char* pSrc[3], int srcStep[3], u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize);
		int YUY2ToNV12(const u_char* pSrc, int srcStep, u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize);


		int BGR4ToBGR3(const u_char* pSrc, int srcStep, u_char* pDst, int dstStep, StructWidthHeight roiSize);
		int RGB3ToRGB4(const u_char* pSrc, int srcStep, u_char* pDst, int dstStep, StructWidthHeight roiSize);

		int Lanczos3Resample(u_char* input, int src_width, int src_height, u_char* output, int dst_width, int dst_height);

	}

	struct ResizeYUV420Spec
	{
		StructWidthHeight srcSize;
		StructWidthHeight dstSize;
		uint32_t numLobes;
	};

	

	int ResizeYUV420GetSize(StructWidthHeight srcSize, StructWidthHeight dstSize, int interpolation, uint32_t antialiasing, int32_t* pSpecSize, int32_t* pInitBufSize);
	int ResizeYUVGetBufferSize(const ResizeYUV420Spec* pSpec, StructWidthHeight dstSize, int32_t* pBufSize);

	int ResizeYUV420LanczosInit(StructWidthHeight srcSize, StructWidthHeight dstSize, uint32_t numLobes, ResizeYUV420Spec*  pSpec, u_char* pInitBuf);

	int ResizeYUV420Lanczos(const u_char* pSrcY, int32_t srcYStep, const u_char* pSrcUV, int32_t srcUVStep, u_char* pDstY, int32_t dstYStep, u_char* pDstUV, int32_t dstUVStep, StructXY dstOffset, StructWidthHeight dstSize, int border, const u_char* pBorderValue, const ResizeYUV420Spec* pSpec, u_char* pBuffer);


}