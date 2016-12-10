// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#include "_ColorConversions.h"

namespace NV12ConvertOrResize {

	static int GetR(int Y, int Cb, int Cr)
	{
		return 76293 * Y - 117 * Cb + 104581 * Cr;
	}


	static int GetG(int Y, int Cb, int Cr)
	{
		return 76293 * Y - 25654 * Cb - 53312 * Cr;
	}


	static int GetB(int Y, int Cb, int Cr)
	{
		return 76293 * Y + 132240 * Cb - 82 * Cr;
	}


	static int GetY(int R, int G, int B)
	{
		return ((16843 * R + 33030 * G + 6423 * B + (16 << 16)) + 32768) >> 16;
	}


	static int GetCb(int R, int G, int B)
	{
		return (-9699 * R - 19071 * G + 28770 * B + 32768) >> 16;
	}

	static int GetCr(int R, int G, int B)
	{
		return (28770 * R - 24117 * G - 4653 * B + 32768) >> 16;
	}

	static int BoundValue(int value, int minValue, int maxValue)
	{
		return __min(__max(value, minValue), maxValue);
	}
	

	static double sinc(double x)
	{
		x = (x * M_PI);

		if ((x < 0.01f) && (x > -0.01f))
			return 1.0f + x*x*(-1.0f / 6.0f + x*x*1.0f / 120.0f);

		return sin(x) / x;
	}

	static double clean(double t)
	{
		const double EPSILON = .0000125f;
		if (fabs(t) < EPSILON)
			return 0.0f;
		return (double)t;
	}

	static double lanczos2_filter(double t)
	{
		if (t < 0.0f)
			t = -t;

		if (t < 2.0f)
			return clean(sinc(t) * sinc(t / 2.0f));
		else
			return (0.0f);
	}

	static double lanczos3_filter(double t)
	{
		if (t < 0.0f)
			t = -t;

		if (t < 3.0f)
			return clean(sinc(t) * sinc(t / 3.0f));
		else
			return (0.0f);
	}

	int NV12ToBGR3(u_char* pSrcY, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstStep, StructWidthHeight roiSize)
	{
		const int outputBytesPerPixel = 3;
		int width = roiSize.width, height = roiSize.height;
//		int outputLineSize = dstStep;
		int outputCurrentByte = 0;
		int lumaIndex = 0;
		int chromaIndex = 0;

		int outputIndex = 0;

		for (int i = 0; i < height >> 1; i++)
		{
			for (int k = 0; k < 2; ++k)
			{
				for (int j = 0; j < width >> 1; ++j)
				{
					int Cb = (int)pSrcCbCr[chromaIndex + j * 2] - 128;
					int Cr = (int)pSrcCbCr[chromaIndex + j * 2 + 1] - 128;
					for (int l = 0; l < 2; ++l)
					{
						int Y = (int)pSrcY[lumaIndex + j * 2 + l] - 16;
						int R = GetR(Y, Cb, Cr);
						int G = GetG(Y, Cb, Cr);
						int B = GetB(Y, Cb, Cr);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel + 2] = (u_char)BoundValue(R >> 16, 0, 255);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel + 1] = (u_char)BoundValue(G >> 16, 0, 255);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel] = (u_char)BoundValue(B >> 16, 0, 255);
						++outputIndex;
					}
				}
				lumaIndex += srcYStep;
				outputCurrentByte += dstStep;
				outputIndex = 0;
			}
			chromaIndex += srcCbCrStep;
		}
		return 0;
	}


	int NV12ToRGB3(u_char* pSrcY, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstStep, StructWidthHeight roiSize)
	{
		const int outputBytesPerPixel = 3;
		int width = roiSize.width, height = roiSize.height;
//		int outputLineSize = dstStep;
		int outputCurrentByte = 0;
		int lumaIndex = 0;
		int chromaIndex = 0;

		int outputIndex = 0;

		for (int i = 0; i < height >> 1; i++)
		{
			for (int k = 0; k < 2; ++k)
			{
				for (int j = 0; j < width >> 1; ++j)
				{
					int Cb = (int)pSrcCbCr[chromaIndex + j * 2] - 128;
					int Cr = (int)pSrcCbCr[chromaIndex + j * 2 + 1] - 128;
					for (int l = 0; l < 2; ++l)
					{
						int Y = (int)pSrcY[lumaIndex + j * 2 + l] - 16;
						int R = GetR(Y, Cb, Cr);
						int G = GetG(Y, Cb, Cr);
						int B = GetB(Y, Cb, Cr);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel + 2] = (u_char)BoundValue(B >> 16, 0, 255);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel + 1] = (u_char)BoundValue(G >> 16, 0, 255);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel] = (u_char)BoundValue(R >> 16, 0, 255);
						++outputIndex;
					}
				}
				lumaIndex += srcYStep;
				outputCurrentByte += dstStep;
				outputIndex = 0;
			}
			chromaIndex += srcCbCrStep;
		}
		return 0;
	}


	int NV12ToBGR4(u_char* pSrcY, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstStep, StructWidthHeight roiSize, int aval)
	{
		const int outputBytesPerPixel = 4;
		int width = roiSize.width, height = roiSize.height;
//		int outputLineSize = dstStep;
		int outputCurrentByte = 0;
		int lumaIndex = 0;
		int chromaIndex = 0;

		int outputIndex = 0;

		for (int i = 0; i < height >> 1; i++)
		{
			for (int k = 0; k < 2; ++k)
			{
				for (int j = 0; j < width >> 1; ++j)
				{
					int Cb = (int)pSrcCbCr[chromaIndex + j * 2] - 128;
					int Cr = (int)pSrcCbCr[chromaIndex + j * 2 + 1] - 128;
					for (int l = 0; l < 2; ++l)
					{
						int Y = (int)pSrcY[lumaIndex + j * 2 + l] - 16;
						int R = GetR(Y, Cb, Cr);
						int G = GetG(Y, Cb, Cr);
						int B = GetB(Y, Cb, Cr);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel + 3] = (u_char)aval;
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel + 2] = (u_char)BoundValue(R >> 16, 0, 255);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel + 1] = (u_char)BoundValue(G >> 16, 0, 255);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel] = (u_char)BoundValue(B >> 16, 0, 255);
						++outputIndex;
					}
				}
				lumaIndex += srcYStep;
				outputCurrentByte += dstStep;
				outputIndex = 0;
			}
			chromaIndex += srcCbCrStep;
		}
		return 0;
	}


	int NV12ToRGB4(u_char* pSrcY, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstStep, StructWidthHeight roiSize, int aval)
	{
		const int outputBytesPerPixel = 4;
		int width = roiSize.width, height = roiSize.height;
//		int outputLineSize = dstStep;
		int outputCurrentByte = 0;
		int lumaIndex = 0;
		int chromaIndex = 0;

		int outputIndex = 0;

		for (int i = 0; i < height >> 1; i++)
		{
			for (int k = 0; k < 2; ++k)
			{
				for (int j = 0; j < width >> 1; ++j)
				{
					int Cb = (int)pSrcCbCr[chromaIndex + j * 2] - 128;
					int Cr = (int)pSrcCbCr[chromaIndex + j * 2 + 1] - 128;
					for (int l = 0; l < 2; ++l)
					{
						int Y = (int)pSrcY[lumaIndex + j * 2 + l] - 16;
						int R = GetR(Y, Cb, Cr);
						int G = GetG(Y, Cb, Cr);
						int B = GetB(Y, Cb, Cr);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel + 3] = (u_char)aval;
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel + 2] = (u_char)BoundValue(B >> 16, 0, 255);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel + 1] = (u_char)BoundValue(G >> 16, 0, 255);
						pDst[outputCurrentByte + outputIndex*outputBytesPerPixel] = (u_char)BoundValue(R >> 16, 0, 255);
						++outputIndex;
					}
				}
				lumaIndex += srcYStep;
				outputCurrentByte += dstStep;
				outputIndex = 0;
			}
			chromaIndex += srcCbCrStep;
		}
		return 0;
	}

	
	int NV12ToYUY2(u_char* pSrc, int srcStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstYStep, StructWidthHeight roiSize)
	{
		int width = roiSize.width, height = roiSize.height;

		int inputLumaByte = 0;
		int inputChromaByte = 0; // width * height;
		int outputByte = 0;


		u_char* Cb = new u_char[width >> 1];
		u_char* Cr = new u_char[width >> 1];
		int i = 0, j = 0;

		u_char* pOutput = pDst;

		//int lumaAddition = srcStep - width * 2;

		int outputAddition = dstYStep - width * 2;
		for (i = 0; i < height >> 1; ++i)
		{
			for (j = 0; j < width >> 1; ++j)
			{
				pOutput[outputByte++] = pSrc[inputLumaByte + (j << 1)];
				Cb[j] = pSrcCbCr[inputChromaByte + (j << 1)];
				pOutput[outputByte++] = Cb[j];
				pOutput[outputByte++] = pSrc[inputLumaByte + (j << 1) + 1];
				Cr[j] = pSrcCbCr[inputChromaByte + (j << 1) + 1];
				pOutput[outputByte++] = Cr[j];
			}
			inputLumaByte += srcStep;
			inputChromaByte += srcCbCrStep;
			outputByte += outputAddition;

			for (j = 0; j < width >> 1; ++j)
			{
				pOutput[outputByte++] = pSrc[inputLumaByte + (j << 1)];
				pOutput[outputByte++] = Cb[j];
				pOutput[outputByte++] = pSrc[inputLumaByte + (j << 1) + 1];
				pOutput[outputByte++] = Cr[j];
			}

			inputLumaByte += srcStep;
			outputByte += outputAddition;
		}

		delete[] Cb;
		delete[] Cr;
		return 0;
	}


	int NV12ToUYVY(u_char* pSrc, int srcStep, u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst, int dstYStep, StructWidthHeight roiSize)
	{
		int width = roiSize.width, height = roiSize.height;

		int inputLumaByte = 0;
		int inputChromaByte = 0; // width * height;
		int outputByte = 0;


		u_char* Cb = new u_char[width >> 1];
		u_char* Cr = new u_char[width >> 1];
		int i = 0, j = 0;

		u_char* pOutput = pDst;

		//int lumaAddition = srcStep - width * 2;

		int outputAddition = dstYStep - width * 2;
		for (i = 0; i < height >> 1; ++i)
		{
			for (j = 0; j < width >> 1; ++j)
			{
				Cb[j] = pSrcCbCr[inputChromaByte + (j << 1)];
				pOutput[outputByte++] = Cb[j];
				pOutput[outputByte++] = pSrc[inputLumaByte + (j << 1)];
				Cr[j] = pSrcCbCr[inputChromaByte + (j << 1) + 1];
				pOutput[outputByte++] = Cr[j];
				pOutput[outputByte++] = pSrc[inputLumaByte + (j << 1) + 1];
			}
			inputLumaByte += srcStep;
			inputChromaByte += srcCbCrStep;
			outputByte += outputAddition;

			for (j = 0; j < width >> 1; ++j)
			{
				pOutput[outputByte++] = Cb[j];
				pOutput[outputByte++] = pSrc[inputLumaByte + (j << 1)];
				pOutput[outputByte++] = Cr[j];
				pOutput[outputByte++] = pSrc[inputLumaByte + (j << 1) + 1];
			}

			inputLumaByte += srcStep;
			outputByte += outputAddition;
		}

		delete[] Cb;
		delete[] Cr;

		return 0;
	}


	int NV12ToYV12(u_char* pSrc, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char** pDst, int* dstStep, StructWidthHeight roiSize)
	{
		int outputLumaByte = 0;
		int outputChromaByte = 0;

		int width = roiSize.width, height = roiSize.height;
		int inputLumaByte = 0;
		int inputChromaByte = 0;

		for (int i = 0; i < height >> 1; ++i)
		{
			for (int j = 0; j < width; ++j)
			{
				pDst[0][outputLumaByte + j] = pSrc[inputLumaByte + j];
			}
			inputLumaByte += srcYStep;
			outputLumaByte += dstStep[0];

			for (int j = 0; j < width; ++j)
			{
				pDst[0][outputLumaByte + j] = pSrc[inputLumaByte + j];
			}
			inputLumaByte += srcYStep;
			outputLumaByte += dstStep[0];


			for (int j = 0; j < width; j += 2)
			{
				pDst[1][outputChromaByte + (j >> 1)] = pSrcCbCr[inputChromaByte + j];
				pDst[2][outputChromaByte + (j >> 1)] = pSrcCbCr[inputChromaByte + j + 1];
			}
			inputChromaByte += srcCbCrStep;
			outputChromaByte += dstStep[2];

		}
		return 0;
	}

	int NV12ToIYUV(u_char* pSrcY, int srcYStep, u_char* pSrcCbCr, int srcCbCrStep, u_char** pDst, int* dstStep, StructWidthHeight roiSize)
	{
		//int outputLumaByte = 0;
		int outputChromaByte = 0;

		int width = roiSize.width, height = roiSize.height;
//		int inputByte = 0;
		//int inputLumaByte = 0;
		int inputChromaByte = 0;

		for (int i = 0; i < height >> 1; ++i)
		{
			// copy luminance of two lines
			memcpy(pDst[0] + 2*i*dstStep[0], pSrcY + 2*i*srcYStep, width);
			memcpy(pDst[0] + (2 * i + 1)* dstStep[0], pSrcY + (2 * i + 1)*srcYStep, width);

			// copy chrominance 
			for (int j = 0; j < width >> 1; ++j)
			{
				pDst[1][outputChromaByte + j] = pSrcCbCr[inputChromaByte + (j << 1)];
				pDst[2][outputChromaByte + j] = pSrcCbCr[inputChromaByte + (j << 1)+1];

			}
			//inputLumaByte += 2 * srcYStep;
			inputChromaByte += srcCbCrStep;

			//outputLumaByte += dstStep[0];
			outputChromaByte += dstStep[2];

		}
		return 0;
	}


	int P411ToNV12(const u_char* pSrc[3], int srcStep[3], u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize)
	{
		int inputLumaByte = 0;
		int inputChromaByte = 0;
		int outputLumaByte = 0;
		int outputChromaByte = 0;
		
		int width = roiSize.width, height = roiSize.height;

		unsigned char* Cb = new unsigned char[width >> 1];
		unsigned char* Cr = new unsigned char[width >> 1];

		//// copy luminance
		for (int i = 0; i < height; ++i)
		{
			memcpy(pDstY + outputLumaByte, pSrc[0] + inputLumaByte, width);
			//pDstY[i] = pSrc[0][i];
			inputLumaByte += srcStep[0];
			outputLumaByte += dstYStep;
		}

		for (int i = 0; i < height >> 1; ++i)
		{
			if (i == 3)
			{
				//int a = 0;
			}
			
			// prepare chrominance values
			for (int j = 0; j < width >> 2; ++j)
			{
				//Cb[j] = (unsigned char)(((unsigned int)pSrc[1][inputChromaByte + (j>>1)] + (unsigned int)pSrc[1][inputChromaByte + (j>>1) + srcStep[1]] + 1) >> 1);
				//Cr[j] = (unsigned char)(((unsigned int)pSrc[2][inputChromaByte + (j>>1)] + (unsigned int)pSrc[2][inputChromaByte + (j>>1) + srcStep[1]] + 1) >> 1);

				Cb[(j<<1)] = pSrc[1][inputChromaByte + j];
				Cb[(j<<1) + 1] = pSrc[1][inputChromaByte + j + srcStep[1]];

				Cr[(j<<1)] = pSrc[2][inputChromaByte + j];
				Cr[(j<<1) + 1] = pSrc[2][inputChromaByte + j + srcStep[1]];

				
				//int a = 0;
			}
			//inputChromaByte += srcStep[1];

			for (int j = 0; j < width >> 1; ++j) // each row is divided by four elements
			{
				// copy chrominance values for two elements ( NV12 is 4:2:0 system)
				pDstCbCr[outputChromaByte + (j<<1)] = Cb[j];
				pDstCbCr[outputChromaByte + (j<<1) + 1] = Cr[j];
			}
			inputChromaByte += 2 * srcStep[1];
			outputChromaByte += dstCbCrStep;

		}

		delete[] Cb;
		delete[] Cr;
		return 0;
	}


	int P422ToNV12(const u_char* pSrc[3], int srcStep[3], u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize)
	{
		int inputLumaByte = 0;
		int inputChromaByte = 0;
		int outputLumaByte = 0;
		int outputChromaByte = 0;

		int width = roiSize.width, height = roiSize.height;

		//// copy luminance
		for (int i = 0; i < height; ++i)
		{
			memcpy(pDstY + outputLumaByte, pSrc[0] + inputLumaByte, width);
			//pDstY[i] = pSrc[0][i];
			inputLumaByte += srcStep[0];
			outputLumaByte += dstYStep;
		}

		for (int i = 0; i < height >> 1; ++i)
		{
			if (i == 3)
			{
				//int a = 0;
			}


			for (int j = 0; j < width >> 1; ++j)
			{
				// copy chrominance values for two elements ( NV12 is 4:2:0 system)
				pDstCbCr[outputChromaByte + (j << 1)] = pSrc[1][inputChromaByte + j];
				pDstCbCr[outputChromaByte + (j << 1) + 1] = pSrc[2][inputChromaByte + j];
			}
			inputChromaByte += 2*srcStep[1];
			outputChromaByte += dstCbCrStep;

		}
		return 0;
	}


	int NV12ToP411(const u_char* pSrcY, int srcYStep, const u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst[3], int dstStep[3], StructWidthHeight roiSize)
	{
		int inputChromaByte = 0;
		int outputChromaByte = 0;
		int width = roiSize.width, height = roiSize.height;

		// copy luminance
		for (int i = 0; i < height; ++i)
		{
			memcpy(pDst[0] + i*dstStep[0], pSrcY + i*srcYStep, width);
		}

		// copy chrmominance
		for (int i = 0; i < height >> 1; ++i)
		{
			for (int j = 0; j < width >> 2; ++j)
			{
				pDst[1][outputChromaByte + j] = pSrcCbCr[inputChromaByte + 4*j];
				pDst[1][outputChromaByte + j + dstStep[1]] = pSrcCbCr[inputChromaByte + 4*j+2];

				pDst[2][outputChromaByte + j] = pSrcCbCr[inputChromaByte + 4*j+1];
				pDst[2][outputChromaByte + j + dstStep[1]] = pSrcCbCr[inputChromaByte + 4*j + 2 + 1];
			}

			inputChromaByte += srcCbCrStep;
			outputChromaByte += dstStep[1] * 2;

		}
		return 0;
	}


	int NV12ToP422(const u_char* pSrcY, int srcYStep, const u_char* pSrcCbCr, int srcCbCrStep, u_char* pDst[3], int dstStep[3], StructWidthHeight roiSize)
	{
		int inputChromaByte = 0;
		int outputChromaByte = 0;
		int width = roiSize.width, height = roiSize.height;

		// copy luminance
		for (int i = 0; i < height; ++i)
		{
			memcpy(pDst[0] + i*dstStep[0], pSrcY + i*srcYStep, width);
		}

		// copy chrmominance
		for (int i = 0; i < height >> 1; ++i)
		{
			for (int j = 0; j < width >> 1; ++j)
			{
				pDst[1][outputChromaByte + j] = pDst[1][outputChromaByte + j + dstStep[1]] = pSrcCbCr[inputChromaByte + 2*j];
				pDst[2][outputChromaByte + j] = pDst[2][outputChromaByte + j + dstStep[1]] = pSrcCbCr[inputChromaByte + 2*j + 1];
			}

			inputChromaByte += srcCbCrStep;
			outputChromaByte += dstStep[1] * 2;
		}
		return 0;
	}

	int BGRToNV12(const u_char* pSrc, int srcStep, u_char* pY, int yStep, u_char* pCbCr, int cbCrStep, StructWidthHeight roiSize, int inputBytesPerPixel)
	{
		int RightUp = inputBytesPerPixel;
		int inputLineSize = srcStep;
		int LeftDown = (int)inputLineSize;
		int RightDown = LeftDown + inputBytesPerPixel;

		int chromaIndex = 0;
		int lumaIndex = 0;
		int inputCurrentByte = 0;

		int width = roiSize.width, height = roiSize.height;

		for ( int i = 0; i < height; i += 2)
		{
			for ( int j = 0; j < width >> 1; ++j )
			{
				int B_LeftUp = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 0];
				int G_LeftUp = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 1];
				int R_LeftUp = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 2];

				int B_RightUp = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 +0 + RightUp];
				int G_RightUp = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 1 + RightUp];
				int R_RightUp = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 2 + RightUp];

				int B_LeftDown = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 0 + LeftDown];
				int G_LeftDown = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 1 + LeftDown];
				int R_LeftDown = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 2 + LeftDown];

				int B_RightDown = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 0 + RightDown];
				int G_RightDown = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 1 + RightDown];
				int R_RightDown = pSrc[inputCurrentByte + inputBytesPerPixel*j*2 + 2 + RightDown];

				int Y_LeftUp = GetY(R_LeftUp, G_LeftUp, B_LeftUp);
				int Cb_LeftUp = GetCb(R_LeftUp, G_LeftUp, B_LeftUp);
				int Cr_LeftUp = GetCr(R_LeftUp, G_LeftUp, B_LeftUp);

				int Y_RightUp = GetY(R_RightUp, G_RightUp, B_RightUp);
				int Cb_RightUp = GetCb(R_RightUp, G_RightUp, B_RightUp);
				int Cr_RightUp = GetCr(R_RightUp, G_RightUp, B_RightUp);

				int Y_LeftDown = GetY(R_LeftDown, G_LeftDown, B_LeftDown);
				int Cb_LeftDown = GetCb(R_LeftDown, G_LeftDown, B_LeftDown);
				int Cr_LeftDown = GetCr(R_LeftDown, G_LeftDown, B_LeftDown);

				int Y_RightDown = GetY(R_RightDown, G_RightDown, B_RightDown);
				int Cb_RightDown = GetCb(R_RightDown, G_RightDown, B_RightDown);
				int Cr_RightDown = GetCr(R_RightDown, G_RightDown, B_RightDown);

				pY[lumaIndex + (j<<1)] = (u_char)BoundValue(Y_LeftUp, 16, 235);
				pY[lumaIndex + (j<<1)+1] = (u_char)BoundValue(Y_RightUp, 16, 235);
				pY[lumaIndex + yStep + (j << 1)] = (u_char)BoundValue(Y_LeftDown, 16, 235);
				pY[lumaIndex + yStep + (j << 1) + 1] = (u_char)BoundValue(Y_RightDown, 16, 235);

				int Cb = ((Cb_LeftUp + Cb_RightUp + Cb_LeftDown + Cb_RightDown) >> 2) + 128;
				int Cr = ((Cr_LeftUp + Cr_RightUp + Cr_LeftDown + Cr_RightDown) >> 2) + 128;
				pCbCr[chromaIndex + (j << 1)] = (u_char)BoundValue( Cb, 16, 240);
				pCbCr[chromaIndex + (j << 1) + 1] = (u_char)BoundValue( Cr, 16, 240);
			}
			inputCurrentByte += 2 * srcStep;

			lumaIndex += 2 * yStep;
			chromaIndex += cbCrStep;
		}
		return 0;
	}


	int BGR3ToNV12(const u_char* pSrc, int srcStep, u_char* pY, int yStep, u_char* pCbCr, int cbCrStep, StructWidthHeight roiSize)
	{
		return BGRToNV12(pSrc, srcStep, pY, yStep, pCbCr, cbCrStep, roiSize, 3);
	}

	int BGR4ToNV12(const u_char* pSrc, int srcStep, u_char* pY, int yStep, u_char* pCbCr, int cbCrStep, StructWidthHeight roiSize)
	{
		return BGRToNV12(pSrc, srcStep, pY, yStep, pCbCr, cbCrStep, roiSize, 4);
	}


	int RGBToNV12(const u_char* pSrc, int srcStep, u_char* pY, int yStep, u_char* pCbCr, int cbCrStep, StructWidthHeight roiSize, int inputBytesPerPixel)
	{
		int RightUp = inputBytesPerPixel;
		int inputLineSize = srcStep;
		int LeftDown = (int)inputLineSize;
		int RightDown = LeftDown + inputBytesPerPixel;

		int chromaIndex = 0;
		int lumaIndex = 0;
		int inputCurrentByte = 0;

		int width = roiSize.width, height = roiSize.height;

		for (int i = 0; i < height; i += 2)
		{
			for (int j = 0; j < width >> 1; ++j)
			{
				int R_LeftUp = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 0];
				int G_LeftUp = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 1];
				int B_LeftUp = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 2];

				int R_RightUp = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 0 + RightUp];
				int G_RightUp = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 1 + RightUp];
				int B_RightUp = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 2 + RightUp];

				int R_LeftDown = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 0 + LeftDown];
				int G_LeftDown = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 1 + LeftDown];
				int B_LeftDown = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 2 + LeftDown];

				int R_RightDown = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 0 + RightDown];
				int G_RightDown = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 1 + RightDown];
				int B_RightDown = pSrc[inputCurrentByte + inputBytesPerPixel*j * 2 + 2 + RightDown];

				int Y_LeftUp = GetY(R_LeftUp, G_LeftUp, B_LeftUp);
				int Cb_LeftUp = GetCb(R_LeftUp, G_LeftUp, B_LeftUp);
				int Cr_LeftUp = GetCr(R_LeftUp, G_LeftUp, B_LeftUp);

				int Y_RightUp = GetY(R_RightUp, G_RightUp, B_RightUp);
				int Cb_RightUp = GetCb(R_RightUp, G_RightUp, B_RightUp);
				int Cr_RightUp = GetCr(R_RightUp, G_RightUp, B_RightUp);

				int Y_LeftDown = GetY(R_LeftDown, G_LeftDown, B_LeftDown);
				int Cb_LeftDown = GetCb(R_LeftDown, G_LeftDown, B_LeftDown);
				int Cr_LeftDown = GetCr(R_LeftDown, G_LeftDown, B_LeftDown);

				int Y_RightDown = GetY(R_RightDown, G_RightDown, B_RightDown);
				int Cb_RightDown = GetCb(R_RightDown, G_RightDown, B_RightDown);
				int Cr_RightDown = GetCr(R_RightDown, G_RightDown, B_RightDown);

				pY[lumaIndex + (j << 1)] = (u_char)BoundValue(Y_LeftUp, 16, 235);
				pY[lumaIndex + (j << 1) + 1] = (u_char)BoundValue(Y_RightUp, 16, 235);
				pY[lumaIndex + yStep + (j << 1)] = (u_char)BoundValue(Y_LeftDown, 16, 235);
				pY[lumaIndex + yStep + (j << 1) + 1] = (u_char)BoundValue(Y_RightDown, 16, 235);

				int Cb = ((Cb_LeftUp + Cb_RightUp + Cb_LeftDown + Cb_RightDown) >> 2) + 128;
				int Cr = ((Cr_LeftUp + Cr_RightUp + Cr_LeftDown + Cr_RightDown) >> 2) + 128;
				pCbCr[chromaIndex + (j << 1)] = (u_char)BoundValue(Cb, 16, 240);
				pCbCr[chromaIndex + (j << 1) + 1] = (u_char)BoundValue(Cr, 16, 240);
			}
			inputCurrentByte += 2 * srcStep;

			lumaIndex += 2 * yStep;
			chromaIndex += cbCrStep;
		}
		return 0;
	}


	int RGB3ToNV12(const u_char* pSrc, int srcStep, u_char* pY, int yStep, u_char* pCbCr, int cbCrStep, StructWidthHeight roiSize)
	{
		return RGBToNV12(pSrc, srcStep, pY, yStep, pCbCr, cbCrStep, roiSize, 3);
	}

	int RGB4ToNV12(const u_char* pSrc, int srcStep, u_char* pY, int yStep, u_char* pCbCr, int cbCrStep, StructWidthHeight roiSize)
	{
		return RGBToNV12(pSrc, srcStep, pY, yStep, pCbCr, cbCrStep, roiSize, 4);
	}


	int UYVYToNV12(const u_char* pSrc, int srcStep, u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize)
	{
		int inputCurrentByte = 0;
		int outputLumaByte = 0;
		int outputChromaByte = 0;

		int width = roiSize.width, height = roiSize.height;

		u_char* Cb = new u_char[width >> 1];
		u_char* Cr = new u_char[width >> 1];

		int additionInput = srcStep - width * 2;
		int additionOutputLuma = dstYStep - width;
		int additionOutputChroma = dstCbCrStep - width;

		for (int i = 0; i < height >> 1; ++i)
		{
			for (int j = 0; j < width >> 1; ++j)
			{
				Cb[j] = pSrc[inputCurrentByte++];
				pDstY[outputLumaByte++] = pSrc[inputCurrentByte++];
				Cr[j] = pSrc[inputCurrentByte++];
				pDstY[outputLumaByte++] = pSrc[inputCurrentByte++];
			}
			inputCurrentByte += additionInput;
			outputLumaByte += additionOutputLuma;

			for (int j = 0; j < width >> 1; ++j)
			{
				pDstCbCr[outputChromaByte++] = Cb[j];// (u_char)(((int)Cb[j] + (int)pSrc[inputCurrentByte++]) >> 1);
				++inputCurrentByte; //skip Cb 
				pDstY[outputLumaByte++] = pSrc[inputCurrentByte++];
				pDstCbCr[outputChromaByte++] = Cr[j]; //(u_char)(((int)Cr[j] + (int)pSrc[inputCurrentByte++]) >> 1);
				++inputCurrentByte; //skip Cr 
				pDstY[outputLumaByte++] = pSrc[inputCurrentByte++];
			}

			inputCurrentByte += additionInput;
			outputLumaByte += additionOutputLuma;
			outputChromaByte += additionOutputChroma;
		}
		return 0;
	}


	int YUY2ToNV12(const u_char* pSrc, int srcStep, u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize)
	{
		int inputCurrentByte = 0;
		int outputLumaByte = 0;
		int outputChromaByte = 0;

		int width = roiSize.width, height = roiSize.height;

		u_char *Cb = new u_char[width >> 1];
		u_char *Cr = new u_char[width >> 1];

		int additionSource = srcStep - width * 2;
		int additionDstY = dstYStep - width;
		int additionDstCbCr = dstCbCrStep - width;

		for (int i = 0; i < height >> 1; ++i)
		{
			for (int j = 0; j < width >> 1; ++j)
			{
				pDstY[outputLumaByte++] = pSrc[inputCurrentByte++];
				Cb[j] = pSrc[inputCurrentByte++];
				pDstY[outputLumaByte++] = pSrc[inputCurrentByte++];
				Cr[j] = pSrc[inputCurrentByte++];
			}
			inputCurrentByte += additionSource;
			outputLumaByte += additionDstY;

			for (int j = 0; j < width >> 1; ++j)
			{
				pDstY[outputLumaByte++] = pSrc[inputCurrentByte++];
				//pDstCbCr[outputChromaByte++] = (u_char)(((int)Cb[j] + (int)pSrc[inputCurrentByte++]) >> 1);
				++inputCurrentByte;
				pDstCbCr[outputChromaByte++] = Cb[j];
				pDstY[outputLumaByte++] = pSrc[inputCurrentByte++];
				//pDstCbCr[outputChromaByte++] = (u_char)(((int)Cr[j] + (int)pSrc[inputCurrentByte++]) >> 1);
				pDstCbCr[outputChromaByte++] = Cr[j];
				++inputCurrentByte;
			}

			inputCurrentByte += additionSource;
			outputLumaByte += additionDstY;
			outputChromaByte += additionDstCbCr;
		}
		return 0;
	}


	int IYUVToNV12(const u_char* pSrc[3], int srcStep[3], u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize)
	{
//		int inputLumaByte = 0;
		int inputChromaByte = 0;
//		int outputLumaByte = 0;
		int outputChromaByte = 0;
		int width = roiSize.width, height = roiSize.height;

//		u_char* Cb = new u_char[width >> 1];
//		u_char*  Cr = new u_char[width >> 1];

		int additionInputChroma = srcStep[1] - width/2;
		for (int i = 0; i < height >> 1; ++i)
		{
			// copy luminance values
			memcpy(pDstY + 2 * i*dstYStep, pSrc[0] + 2 * i*srcStep[0], width);
			memcpy(pDstY + (2*i+1)*dstYStep, pSrc[0] + (2 * i + 1)*srcStep[0], width);

			// copy chrominance values
			for (int j = 0; j < width >> 1; ++j)
			{
				pDstCbCr[outputChromaByte + (j<<1)] = pSrc[1][inputChromaByte];
				pDstCbCr[outputChromaByte + (j<<1)+1] = pSrc[2][inputChromaByte];
				++inputChromaByte;
			}
			inputChromaByte += additionInputChroma;
			outputChromaByte += dstCbCrStep;
		}
		return 0;
	}


	int YV12ToNV12(const u_char* pSrc[3], int srcStep[3], u_char* pDstY, int dstYStep, u_char* pDstCbCr, int dstCbCrStep, StructWidthHeight roiSize)
	{
//		int inputLumaByte = 0;
		int inputChromaByte = 0;
//		int outputLumaByte = 0;
		int outputChromaByte = 0;
		int width = roiSize.width, height = roiSize.height;

//		u_char* Cb = new u_char[width >> 1];
//		u_char*  Cr = new u_char[width >> 1];

		int additionInputChroma = srcStep[1] - width / 2;
		for (int i = 0; i < height >> 1; ++i)
		{
			// copy luminance values
			memcpy(pDstY + 2 * i*dstYStep, pSrc[0] + 2 * i*srcStep[0], width);
			memcpy(pDstY + (2 * i + 1)*dstYStep, pSrc[0] + (2 * i + 1)*srcStep[0], width);

			// copy chrominance values
			for (int j = 0; j < width >> 1; ++j)
			{
				pDstCbCr[outputChromaByte + (j << 1)] = pSrc[2][inputChromaByte];
				pDstCbCr[outputChromaByte + (j << 1) + 1] = pSrc[1][inputChromaByte];
				++inputChromaByte;
			}
			inputChromaByte += additionInputChroma;
			outputChromaByte += dstCbCrStep;
		}
		return 0;
	}


	int BGR4ToBGR3(const u_char* pSrc, int srcStep, u_char* pDst, int dstStep, StructWidthHeight roiSize)
	{
		int width = roiSize.width, height = roiSize.height;

		int additionInput = srcStep - 4 * width;
		int additionOutput = dstStep - 3 * width;
	
		int inputIndex = 0;
		int outputIndex = 0;
		for (int i = 0; i < height; ++i)
		{
			for (int j = 0; j < width; ++j)
			{
				pDst[outputIndex] = pSrc[inputIndex];
				pDst[outputIndex + 1] = pSrc[inputIndex+1];
				pDst[outputIndex + 2] = pSrc[inputIndex+2];
				inputIndex += 4;
				outputIndex += 3;
			}
			inputIndex += additionInput;
			outputIndex += additionOutput;
		}
		return 0;
	}


	int RGB3ToRGB4(const u_char* pSrc, int srcStep, u_char* pDst, int dstStep, StructWidthHeight roiSize)
	{
		int width = roiSize.width, height = roiSize.height;

		int additionInput = srcStep - 3 * width;
		int additionOutput = dstStep - 4 * width;

		int inputIndex = 0;
		int outputIndex = 0;
		for (int i = 0; i < height; ++i)
		{
			for (int j = 0; j < width; ++j)
			{
				pDst[outputIndex] = pSrc[inputIndex];
				pDst[outputIndex + 1] = pSrc[inputIndex + 1];
				pDst[outputIndex + 2] = pSrc[inputIndex + 2];
				pDst[outputIndex + 3] = 0;
				inputIndex += 3;
				outputIndex += 4;
			}
			inputIndex += additionInput;
			outputIndex += additionOutput;
		}
		return 0;
	}



	int Lanczos3Resample(u_char* input, int src_width, int src_height, u_char* output, int dst_width, int dst_height)
	{
		const int max_components = 4;
		int n = 3;
		if ((max(src_width, src_height) > RESAMPLER_MAX_DIMENSION) || (n > max_components))
		{
			printf("Image is too large!\n");
			return EXIT_FAILURE;
		}

		// Partial gamma correction looks better on mips. Set to 1.0 to disable gamma correction. 
		//const float source_gamma = 1.75f;
		const float source_gamma = 1.0f;

		// Filter scale - values < 1.0 cause aliasing, but create sharper looking mips.
		const float filter_scale = 1.0f;//.75f;

		//const char* pFilter = "blackman";//RESAMPLER_DEFAULT_FILTER;

		const char* pFilter = "lanczos3";

		float srgb_to_linear[256];
		for (int i = 0; i < 256; ++i)
			srgb_to_linear[i] = (float)pow(i * 1.0f / 255.0f, source_gamma);

		const int linear_to_srgb_table_size = 4096;
		unsigned char linear_to_srgb[linear_to_srgb_table_size];

		const float inv_linear_to_srgb_table_size = 1.0f / linear_to_srgb_table_size;
		const float inv_source_gamma = 1.0f / source_gamma;

		for (int i = 0; i < linear_to_srgb_table_size; ++i)
		{
			int k = (int)(255.0f * pow(i * inv_linear_to_srgb_table_size, inv_source_gamma) + .5f);
			if (k < 0) k = 0; else if (k > 255) k = 255;
			linear_to_srgb[i] = (unsigned char)k;
		}

		Resampler* resamplers[max_components];
		std::vector<float> samples[max_components];

		// Now create a Resampler instance for each component to process. The first instance will create new contributor tables, which are shared by the resamplers 
		// used for the other components (a memory and slight cache efficiency optimization).
		resamplers[0] = new Resampler(src_width, src_height, dst_width, dst_height, Resampler::BOUNDARY_CLAMP, 0.0f, 1.0f, pFilter, NULL, NULL, filter_scale, filter_scale);
		samples[0].resize(src_width);
		for (int i = 1; i < n; i++)
		{
			resamplers[i] = new Resampler(src_width, src_height, dst_width, dst_height, Resampler::BOUNDARY_CLAMP, 0.0f, 1.0f, pFilter, resamplers[0]->get_clist_x(), resamplers[0]->get_clist_y(), filter_scale, filter_scale);
			samples[i].resize(src_width);
		}


		//std::vector<unsigned char> dst_image(dst_width * n * dst_height);

		const int src_pitch = src_width * n;
		const int dst_pitch = dst_width * n;
		int dst_y = 0;

		printf("Resampling to %ux%u\n", dst_width, dst_height);

		for (int src_y = 0; src_y < src_height; src_y++)
		{
			const unsigned char* pSrc = &input[src_y * src_pitch];

			for (int x = 0; x < src_width; x++)
			{
				for (int c = 0; c < n; c++)
				{
					if ((c == 3) || ((n == 2) && (c == 1)))
						samples[c][x] = *pSrc++ * (1.0f / 255.0f);
					else
						samples[c][x] = srgb_to_linear[*pSrc++];
				}
			}

			for (int c = 0; c < n; c++)
			{
				if (!resamplers[c]->put_line(&samples[c][0]))
				{
					printf("Out of memory!\n");
					return EXIT_FAILURE;
				}
			}

			for (;;)
			{
				int comp_index;
				for (comp_index = 0; comp_index < n; comp_index++)
				{
					const float* pOutput_samples = resamplers[comp_index]->get_line();
					if (!pOutput_samples)
						break;

					const bool alpha_channel = (comp_index == 3) || ((n == 2) && (comp_index == 1));
					assert(dst_y < dst_height);
					unsigned char* pDst = &output[dst_y * dst_pitch + comp_index];

					for (int x = 0; x < dst_width; x++)
					{
						if (alpha_channel)
						{
							int c = (int)(255.0f * pOutput_samples[x] + .5f);
							if (c < 0) c = 0; else if (c > 255) c = 255;
							*pDst = (unsigned char)c;
						}
						else
						{
							int j = (int)(linear_to_srgb_table_size * pOutput_samples[x] + .5f);
							if (j < 0) j = 0; else if (j >= linear_to_srgb_table_size) j = linear_to_srgb_table_size - 1;
							*pDst = linear_to_srgb[j];
						}

						pDst += n;
					}
				}
				if (comp_index < n)
					break;

				dst_y++;
			}
		}

		//printf("Writing TGA file: %s\n", pDst_filename);

		//if (!stbi_write_tga(pDst_filename, dst_width, dst_height, n, &dst_image[0]))
		//{
		//	printf("Failed writing output image!\n");
		//	return EXIT_FAILURE;
		//}

		//stbi_image_free(pSrc_image);

		// Delete the resamplers.
		for (int i = 0; i < n; i++)
			delete resamplers[i];

		return EXIT_SUCCESS;

	}

#pragma warning( disable : 4100 )  


	int ResizeYUV420GetSize(StructWidthHeight srcSize, StructWidthHeight dstSize, int interpolation, uint32_t antialiasing, int32_t* pSpecSize, int32_t* pInitBufSize)
	{
		int status = 0;

		*pSpecSize = 0;
		*pInitBufSize = 0;
		if (interpolation == 16) // 16 specifies the type lanczos
		{
			*pSpecSize = 1;
			*pInitBufSize = 1;

		}
		return status;

	}


	int ResizeYUVGetBufferSize(const ResizeYUV420Spec* pSpec, StructWidthHeight dstSize, int32_t* pBufSize)
	{
		*pBufSize = 1;
		return 0;
	}


	int ResizeYUV420LanczosInit(StructWidthHeight srcSize, StructWidthHeight dstSize, uint32_t numLobes, ResizeYUV420Spec*  pSpec, u_char* pInitBuf)
	{
		int status = 0;
		if (pSpec)
		{
			memcpy(&pSpec->srcSize, &srcSize, sizeof(StructWidthHeight));
			memcpy(&pSpec->dstSize, &dstSize, sizeof(StructWidthHeight));
			pSpec->numLobes = numLobes;
		}
		else
			status = -17; /* Context parameter does not match the operation. */

		return status;
	}



	struct ResizeYUV420LanczosLuminanceParam
	{
		int src_width;
		int src_height;
		int dst_width;
		int dst_height;
		u_char *pSrcY;
		u_char *pDstY;
		int32_t srcYStep;
		int32_t dstYStep;
		std::vector<float> *samples;
		Resampler **resamplers;
		unsigned char *linear_to_srgb;
		float *srgb_to_linear;
		int linear_to_srgb_table_size;
		int defaultRadius;
		double(*pFilter)(double);
		int specialMultiplier;

	};

	struct ResizeYUV420LanczosChrominanceParam
	{
		int src_width;
		int src_height;
		int dst_width;
		int dst_height;
		u_char *pSrcCbCr;
		u_char *pDstCbCr;
		int32_t srcCbCrStep;
		int32_t dstCbCrStep;
		std::vector<float> *samples;
		Resampler **resamplers;
		unsigned char *linear_to_srgb;
		float *srgb_to_linear;
		int linear_to_srgb_table_size;
		int defaultRadius;
		double(*pFilter)(double);
		int specialMultiplier;
	};

	void CreateWeightsTable(const int steps, double xradius, double yradius, double **&weights_table, 
		double *&xsteps_table, double *&ysteps_table, double(*pFilter)(double))
	{
		weights_table = new double*[steps];
		for (int i = 0; i < steps; ++i)
			weights_table[i] = new double[steps];
		xsteps_table = new double[steps];
		ysteps_table = new double[steps];
		double xstep = 2.0 * xradius / (double)steps;
		double ystep = 2.0 * yradius / (double)steps;
		double xvalue = -xradius;
		double yvalue = -yradius;
		for (int i = 0; i < steps; ++i)
		{
			xsteps_table[i] = xvalue;
			xvalue += xstep;
			ysteps_table[i] = yvalue;
			yvalue += ystep;
		}

		for (int i = 0; i < steps; ++i)
			for (int j = 0; j < steps;++j)
				weights_table[i][j] = pFilter(ysteps_table[i]) * pFilter(xsteps_table[j]);

	}

	void ResizeYUV420LanczosLuminance(ResizeYUV420LanczosLuminanceParam& p)
	{
		double scale_x = (double)p.dst_width / (double)p.src_width;
		double scale_y = (double)p.dst_height / (double)p.src_height;
//		double filter_factor = 1.0;

		int radius = p.defaultRadius;
		double xRadius = (double)radius;
		double yRadius = (double)radius;

		if ( scale_x < 1.0 )
			xRadius = (double)radius / scale_x;
		if (scale_y < 1.0)
			yRadius = (double)radius / scale_y;

		int steps = p.specialMultiplier * p.defaultRadius;
		
		double **weights_table = NULL;
		double *xsteps_table = NULL, *ysteps_table = NULL;
		CreateWeightsTable ( steps, xRadius, yRadius, weights_table, xsteps_table, ysteps_table, p.pFilter);

		for (int i = 0; i < p.dst_height; ++i)
			for (int j = 0; j < p.dst_width; ++j)
			{
				double color = 0.0;
				double total_weight = 0.0;
				double src_y = ((double)(i)) * (double)p.src_height / (double)p.dst_height;
				
				for (int k = 0; k < steps; ++k)
				{
					int y = (int)(src_y+ysteps_table[k]);
					if (y >= 0 && y < p.src_height)
					{
//						double L1 = p.pFilter(k);
						for (int q = 0; q < steps; ++q)
						{
							double src_x = ((double)(j)) * (double)p.src_width / (double)p.dst_width;
							int x = (int)(src_x+xsteps_table[q]);
							if (x >= 0 && x < p.src_width)
							{
								//double L2 = p.pFilter(q);
								//double weight = L1 * L2;
								double weight = weights_table[k][q];
								total_weight += weight;
								
								int c = p.pSrcY[(int)y * p.srcYStep + (int)x];
								color += (double)c * weight;
							}
						}
					}
				}

				p.pDstY[i*p.dstYStep + j] = (u_char)(color / total_weight);
			}


		delete[] xsteps_table;
		delete[] ysteps_table;
		for (int i = 0; i < steps; ++i)
			delete[] weights_table[i];
		delete[] weights_table;
	}


	void ResizeYUV420LanczosChrominance(ResizeYUV420LanczosChrominanceParam& p)
	{
		double scale_x = (double)p.dst_width / (double)p.src_width;
		double scale_y = (double)p.dst_height / (double)p.src_height;
//		double filter_factor = 1.0;

		int radius = p.defaultRadius;
		double xRadius = radius;
		double yRadius = radius;

		if ( scale_x < 1.0 )
			xRadius = scale_x * (double)radius;
		if (scale_y < 1.0)
			yRadius = scale_y * (double)radius;


		int steps = p.specialMultiplier * p.defaultRadius;

		double **weights_table = NULL;
		double *xsteps_table = NULL, *ysteps_table = NULL;
		CreateWeightsTable(steps, xRadius, yRadius, weights_table, xsteps_table, ysteps_table, p.pFilter);


		for (int i = 0; i < p.dst_height; ++i)
			for (int j = 0; j < p.dst_width; ++j)
			{
				double colorCb = 0.0, colorCr = 0.0;
				double total_weight = 0.0;

				double src_y = ((double)(i)) * (double)p.src_height / (double)p.dst_height;
				double src_x = ((double)(j)) * (double)p.src_width / (double)p.dst_width;

				
				for (int k = 0; k < steps; ++k)
				{
					//int y = (int)(src_y + k);
					int y = (int)(src_y + ysteps_table[k]);
					if (y >= 0 && y < p.src_height)
					{
//						double L1 = p.pFilter((Resample_Real)k);
						for (int q = 0; q < steps; ++q)
						{
							int x = (int)(src_x + xsteps_table[q]);
							if (x >= 0 && x < p.src_width)
							{
								//double L2 = p.pFilter((Resample_Real)q);
								//double weight = L1 * L2;
								double weight = weights_table[k][q];
								total_weight += weight;
								
								int c1 = p.pSrcCbCr[(int)y * p.srcCbCrStep + 2*(int)x];
								colorCb += (double)c1 * weight;
								int c2 = p.pSrcCbCr[(int)y * p.srcCbCrStep + 2*(int)x+1];
								colorCr += (double)c2* weight;
							}
						}
					}
				}
				p.pDstCbCr[i*p.dstCbCrStep + 2 * j] = (u_char)(colorCb / total_weight);
				p.pDstCbCr[i*p.dstCbCrStep + 2 * j+1] = (u_char)(colorCr / total_weight);

			}
		

		delete[] xsteps_table;
		delete[] ysteps_table;
		for (int i = 0; i < steps; ++i)
			delete[] weights_table[i];
		delete[] weights_table;

	}

	int ResizeYUV420Lanczos(const u_char* pSrcY, int32_t srcYStep, const u_char* pSrcUV, int32_t srcUVStep, u_char* pDstY, int32_t dstYStep, u_char* pDstUV, int32_t dstUVStep, StructXY dstOffset, StructWidthHeight dstSize, int border, const u_char* pBorderValue, const ResizeYUV420Spec* pSpec, u_char* pBuffer)
	{
		const int max_components = 4;
		int n = 3;
		const int src_width = pSpec->srcSize.width;
		const int src_height = pSpec->srcSize.height;
		
		const int dst_width = pSpec->dstSize.width;
		const int dst_height = pSpec->dstSize.height;

		if ((max(src_width, src_height) > RESAMPLER_MAX_DIMENSION) || (n > max_components))
		{
			printf("Image is too large!\n");
			return -150; /* AAC: Internal error.  */
		}


		const int specialMultiplier = 6; // takes part in computation of steps to compute Lanczos kernel
										 // number of points will be radius * specialMultiplier
										 // it should be 2 or abowe
										 // higher values give better quality, but lower computation speed


		///// process luminance ///////////////////
		ResizeYUV420LanczosLuminanceParam p1;
		p1.src_width = src_width;
		p1.src_height = src_height;
		p1.dst_width = dst_width;
		p1.dst_height = dst_height;
		p1.pSrcY = (u_char*)pSrcY;
		p1.pDstY = (u_char*)pDstY;
		p1.srcYStep = srcYStep;
		p1.dstYStep = dstYStep;
		p1.specialMultiplier = specialMultiplier;
		if (pSpec->numLobes == 2)
		{
			p1.defaultRadius = 2;
			p1.pFilter = &lanczos2_filter;
		}
		else
		{
			p1.defaultRadius = 3;
			p1.pFilter = lanczos3_filter;
		}
		ResizeYUV420LanczosLuminance(p1);


		///// process chrominance ///////////////////
		ResizeYUV420LanczosChrominanceParam p2;
		p2.src_width = src_width / 2;
		p2.src_height = src_height / 2;
		p2.dst_width = dst_width / 2;
		p2.dst_height = dst_height / 2;
		p2.pSrcCbCr = (u_char*)pSrcUV;
		p2.pDstCbCr = (u_char*)pDstUV;
		p2.srcCbCrStep = srcUVStep;
		p2.dstCbCrStep = dstUVStep;
		p2.specialMultiplier = specialMultiplier;
		if (pSpec->numLobes == 2)
		{
			p2.defaultRadius = 2;
			p2.pFilter = &lanczos2_filter;
		}
		else
		{
			p2.defaultRadius = 3;
			p2.pFilter = lanczos3_filter;
		}
		ResizeYUV420LanczosChrominance(p2);

		//// Delete the resamplers.
		//for (int i = 0; i < n; i++)
		//	delete resamplers[i];

		//return EXIT_SUCCESS;

		return 0;
	}
}



