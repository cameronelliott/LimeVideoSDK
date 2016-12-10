// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory


using System.IO;

namespace LimeVideoSDKQuickSync
{

    /// <summary>
    /// This is used fully specify a transcoder configuration
    /// </summary>
    public class TranscoderConfiguration
    {
        /// <summary>Decoder parameters</summary>
        public mfxVideoParam decParams;
        /// <summary>The VPP parameters</summary>
        public mfxVideoParam vppParams;
        /// <summary>The encoder parameters</summary>
        public mfxVideoParam encParams;

        /// <summary>Builds the transcoder configuration from stream.</summary>
        /// <param name="inStream">The in stream.</param>
        /// <param name="inputCodecId">The input codec identifier.</param>
        /// <param name="outputCodecId">The output codec identifier.</param>
        /// <param name="implementation">The implementation.</param>
        /// <param name="useOpaqueSurfaces">if set to <c>true</c> [use opaque surfaces].</param>
        /// <returns></returns>
        public static TranscoderConfiguration BuildTranscoderConfigurationFromStream(Stream inStream, CodecId inputCodecId, CodecId outputCodecId, mfxIMPL implementation = mfxIMPL.MFX_IMPL_AUTO, bool useOpaqueSurfaces = true)
        {
            TranscoderConfiguration config = new TranscoderConfiguration();

            long oldposition = inStream.Position;
            config.decParams = QuickSyncStatic.DecodeHeader(inStream, inputCodecId, implementation);
            inStream.Position = oldposition;

            //config.decParams.mfx.CodecId  was set in last function
            //config.encParams.mfx.CodecId  will get set below in a func

            int width = config.decParams.mfx.FrameInfo.CropW;
            int height = config.decParams.mfx.FrameInfo.CropH;
            config.vppParams = TranscoderSetupVPPParameters(width, height);
            config.encParams = TranscoderSetupEncoderParameters(width, height, outputCodecId);


            config.decParams.IOPattern = IOPattern.MFX_IOPATTERN_OUT_SYSTEM_MEMORY;
            config.vppParams.IOPattern = IOPattern.MFX_IOPATTERN_IN_SYSTEM_MEMORY | IOPattern.MFX_IOPATTERN_OUT_SYSTEM_MEMORY;
            config.encParams.IOPattern = IOPattern.MFX_IOPATTERN_IN_SYSTEM_MEMORY;

            // Configure Media SDK to keep more operations in flight
            // - AsyncDepth represents the number of tasks that can be submitted, before synchronizing is required
            ushort asyncdepth = 4;
            config.decParams.AsyncDepth = asyncdepth;
            config.encParams.AsyncDepth = asyncdepth;
            config.vppParams.AsyncDepth = asyncdepth;

            return config;
        }



        static mfxVideoParam TranscoderSetupEncoderParameters(int width, int height, CodecId codec)
        {
            //  mfxVideoParam mfxEncParams;
            mfxVideoParam mfxEncParams = new mfxVideoParam();
            mfxEncParams.mfx.CodecId = codec;
            mfxEncParams.mfx.TargetUsage = TargetUsage.MFX_TARGETUSAGE_BALANCED;
            mfxEncParams.mfx.TargetKbps = 1000;
            mfxEncParams.mfx.RateControlMethod = RateControlMethod.MFX_RATECONTROL_VBR;
            mfxEncParams.mfx.FrameInfo.FrameRateExtN = 30;
            mfxEncParams.mfx.FrameInfo.FrameRateExtD = 1;
            mfxEncParams.mfx.FrameInfo.FourCC = FourCC.NV12;
            mfxEncParams.mfx.FrameInfo.ChromaFormat = ChromaFormat.MFX_CHROMAFORMAT_YUV420;
            mfxEncParams.mfx.FrameInfo.PicStruct = PicStruct.MFX_PICSTRUCT_PROGRESSIVE;
            mfxEncParams.mfx.FrameInfo.CropX = 0;
            mfxEncParams.mfx.FrameInfo.CropY = 0;
            mfxEncParams.mfx.FrameInfo.CropW = (ushort)width;     // Half the resolution of decode stream
            mfxEncParams.mfx.FrameInfo.CropH = (ushort)height;
            // width must be a multiple of 16
            // height must be a multiple of 16 in case of frame picture and a multiple of 32 in case of field picture
            mfxEncParams.mfx.FrameInfo.Width = ALIGN16(mfxEncParams.mfx.FrameInfo.CropW);
            mfxEncParams.mfx.FrameInfo.Height =
                (PicStruct.MFX_PICSTRUCT_PROGRESSIVE == mfxEncParams.mfx.FrameInfo.PicStruct) ?
                ALIGN16(mfxEncParams.mfx.FrameInfo.CropH) :
                ALIGN32(mfxEncParams.mfx.FrameInfo.CropH);

            return mfxEncParams;
        }
        static mfxVideoParam TranscoderSetupVPPParameters(int width, int height)
        {
            mfxVideoParam VPPParams = new mfxVideoParam();

            // Input data
            VPPParams.vpp.In.FourCC = FourCC.NV12;
            VPPParams.vpp.In.ChromaFormat = ChromaFormat.MFX_CHROMAFORMAT_YUV420;
            VPPParams.vpp.In.CropX = 0;
            VPPParams.vpp.In.CropY = 0;
            VPPParams.vpp.In.CropW = (ushort)width;
            VPPParams.vpp.In.CropH = (ushort)height;
            VPPParams.vpp.In.PicStruct = PicStruct.MFX_PICSTRUCT_PROGRESSIVE;
            VPPParams.vpp.In.FrameRateExtN = 30;
            VPPParams.vpp.In.FrameRateExtD = 1;
            // width must be a multiple of 16
            // height must be a multiple of 16 in case of frame picture and a multiple of 32 in case of field picture
            VPPParams.vpp.In.Width = ALIGN16(VPPParams.vpp.In.CropW);
            VPPParams.vpp.In.Height =
                (PicStruct.MFX_PICSTRUCT_PROGRESSIVE == VPPParams.vpp.In.PicStruct) ?
                ALIGN16(VPPParams.vpp.In.CropH) :
                ALIGN32(VPPParams.vpp.In.CropH);
            // Output data
            VPPParams.vpp.Out.FourCC = FourCC.NV12;
            VPPParams.vpp.Out.ChromaFormat = ChromaFormat.MFX_CHROMAFORMAT_YUV420;
            VPPParams.vpp.Out.CropX = 0;
            VPPParams.vpp.Out.CropY = 0;
            VPPParams.vpp.Out.CropW = (ushort)(VPPParams.vpp.In.CropW / 1);   // 1/16th the resolution of decode stream
            VPPParams.vpp.Out.CropH = (ushort)(VPPParams.vpp.In.CropH / 1);
            VPPParams.vpp.Out.PicStruct = PicStruct.MFX_PICSTRUCT_PROGRESSIVE;
            VPPParams.vpp.Out.FrameRateExtN = 30;
            VPPParams.vpp.Out.FrameRateExtD = 1;
            // width must be a multiple of 16
            // height must be a multiple of 16 in case of frame picture and a multiple of 32 in case of field picture
            VPPParams.vpp.Out.Width = ALIGN16(VPPParams.vpp.Out.CropW);
            VPPParams.vpp.Out.Height =
                (PicStruct.MFX_PICSTRUCT_PROGRESSIVE == VPPParams.vpp.Out.PicStruct) ?
                ALIGN16(VPPParams.vpp.Out.CropH) :
                ALIGN32(VPPParams.vpp.Out.CropH);

            return VPPParams;
        }

        static ushort ALIGN16(int x)
        {
            return (ushort)((x + 15) / 16 * 16);
        }

        static ushort ALIGN32(int x)
        {
            return (ushort)((x + 31) / 32 * 32);
        }

    }
}
