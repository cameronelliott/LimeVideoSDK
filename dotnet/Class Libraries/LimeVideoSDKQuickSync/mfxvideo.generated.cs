// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System;
using System.Runtime.InteropServices;


namespace LimeVideoSDK.QuickSyncTypes
{
    /// <summary>
    /// This describes the tradeoff between speed and quality to select.
    /// 1 = 100% quality over speed
    /// 7 = 100% speed over quality
    /// </summary>
    public enum TargetUsage : ushort
    {

        MFX_TARGETUSAGE_1 = 1,
        MFX_TARGETUSAGE_2 = 2,
        MFX_TARGETUSAGE_3 = 3,
        MFX_TARGETUSAGE_4 = 4,
        MFX_TARGETUSAGE_5 = 5,
        MFX_TARGETUSAGE_6 = 6,
        MFX_TARGETUSAGE_7 = 7,
        MFX_TARGETUSAGE_UNKNOWN = 0,
        MFX_TARGETUSAGE_BEST_QUALITY = 1,
        MFX_TARGETUSAGE_BALANCED = 4,
        MFX_TARGETUSAGE_BEST_SPEED = 7,
    }


    public enum RateControlMethod : ushort
    {
        MFX_RATECONTROL_CBR = 1,
        MFX_RATECONTROL_VBR = 2,
        MFX_RATECONTROL_CQP = 3,
        MFX_RATECONTROL_AVBR = 4,
        MFX_RATECONTROL_RESERVED1 = 5,
        MFX_RATECONTROL_RESERVED2 = 6,
        MFX_RATECONTROL_RESERVED3 = 100,
        MFX_RATECONTROL_RESERVED4 = 7,
        MFX_RATECONTROL_LA = 8,
        MFX_RATECONTROL_ICQ = 9,
        MFX_RATECONTROL_VCM = 10,
        MFX_RATECONTROL_LA_ICQ = 11,
        MFX_RATECONTROL_LA_EXT = 12,
        MFX_RATECONTROL_LA_HRD = 13,
        MFX_RATECONTROL_QVBR = 14,
    }
    public enum PicStruct : ushort
    {
        MFX_PICSTRUCT_UNKNOWN = 0,
        MFX_PICSTRUCT_PROGRESSIVE = 1,
        MFX_PICSTRUCT_FIELD_TFF = 2,
        MFX_PICSTRUCT_FIELD_BFF = 4,
        MFX_PICSTRUCT_FIELD_REPEATED = 16,
        MFX_PICSTRUCT_FRAME_DOUBLING = 32,
        MFX_PICSTRUCT_FRAME_TRIPLING = 64,
    }
    public enum ChromaFormat : ushort
    {
        MFX_CHROMAFORMAT_MONOCHROME = 0,
        MFX_CHROMAFORMAT_YUV420 = 1,
        MFX_CHROMAFORMAT_YUV422 = 2,
        MFX_CHROMAFORMAT_YUV444 = 3,
        MFX_CHROMAFORMAT_YUV400 = 0,
        MFX_CHROMAFORMAT_YUV411 = 4,
        MFX_CHROMAFORMAT_YUV422H = 2,
        MFX_CHROMAFORMAT_YUV422V = 5,
    }
    public enum mfxStatus : int
    {
        MFX_ERR_NONE = 0,
        MFX_ERR_UNKNOWN = -1,
        MFX_ERR_NULL_PTR = -2,
        MFX_ERR_UNSUPPORTED = -3,
        MFX_ERR_MEMORY_ALLOC = -4,
        MFX_ERR_NOT_ENOUGH_BUFFER = -5,
        MFX_ERR_INVALID_HANDLE = -6,
        MFX_ERR_LOCK_MEMORY = -7,
        MFX_ERR_NOT_INITIALIZED = -8,
        MFX_ERR_NOT_FOUND = -9,
        MFX_ERR_MORE_DATA = -10,
        MFX_ERR_MORE_SURFACE = -11,
        MFX_ERR_ABORTED = -12,
        MFX_ERR_DEVICE_LOST = -13,
        MFX_ERR_INCOMPATIBLE_VIDEO_PARAM = -14,
        MFX_ERR_INVALID_VIDEO_PARAM = -15,
        MFX_ERR_UNDEFINED_BEHAVIOR = -16,
        MFX_ERR_DEVICE_FAILED = -17,
        MFX_ERR_MORE_BITSTREAM = -18,
        MFX_ERR_INCOMPATIBLE_AUDIO_PARAM = -19,
        MFX_ERR_INVALID_AUDIO_PARAM = -20,
        MFX_WRN_IN_EXECUTION = 1,
        MFX_WRN_DEVICE_BUSY = 2,
        MFX_WRN_VIDEO_PARAM_CHANGED = 3,
        MFX_WRN_PARTIAL_ACCELERATION = 4,
        MFX_WRN_INCOMPATIBLE_VIDEO_PARAM = 5,
        MFX_WRN_VALUE_NOT_CHANGED = 6,
        MFX_WRN_OUT_OF_RANGE = 7,
        MFX_WRN_FILTER_SKIPPED = 10,
        MFX_WRN_INCOMPATIBLE_AUDIO_PARAM = 11,
        MFX_TASK_DONE = 0,
        MFX_TASK_WORKING = 8,
        MFX_TASK_BUSY = 9,
    }
    public enum mfxIMPL : uint
    {
        MFX_IMPL_AUTO = 0,
        MFX_IMPL_SOFTWARE = 1,
        MFX_IMPL_HARDWARE = 2,
        MFX_IMPL_AUTO_ANY = 3,
        MFX_IMPL_HARDWARE_ANY = 4,
        MFX_IMPL_HARDWARE2 = 5,
        MFX_IMPL_HARDWARE3 = 6,
        MFX_IMPL_HARDWARE4 = 7,
        MFX_IMPL_RUNTIME = 8,
        MFX_IMPL_VIA_ANY = 256,
        MFX_IMPL_VIA_D3D9 = 512,
        MFX_IMPL_VIA_D3D11 = 768,
        MFX_IMPL_VIA_VAAPI = 1024,
        MFX_IMPL_AUDIO = 32768,
        MFX_IMPL_UNSUPPORTED = 0,
    }




    public enum CodecId : uint
    {
        MFX_CODEC_AVC = 541283905,
        MFX_CODEC_HEVC = 1129727304,
        MFX_CODEC_MPEG2 = 843534413,
        MFX_CODEC_VC1 = 540099414,
        MFX_CODEC_CAPTURE = 1414545731,
        MFX_CODEC_JPEG = 'J' | 'P' << 8 | 'E' << 16 | 'G' << 24,
    }
    public enum IOPattern : ushort
    {
        MFX_IOPATTERN_IN_VIDEO_MEMORY = 1,
        MFX_IOPATTERN_IN_SYSTEM_MEMORY = 2,
        MFX_IOPATTERN_IN_OPAQUE_MEMORY = 4,
        MFX_IOPATTERN_OUT_VIDEO_MEMORY = 16,
        MFX_IOPATTERN_OUT_SYSTEM_MEMORY = 32,
        MFX_IOPATTERN_OUT_OPAQUE_MEMORY = 64,
    }
    public enum BufferId : uint
    {
        MFX_EXTBUFF_CODING_OPTION = 1347372099,
        MFX_EXTBUFF_CODING_OPTION_SPSPPS = 1347637059,
        MFX_EXTBUFF_VPP_DONOTUSE = 1163089230,
        MFX_EXTBUFF_VPP_AUXDATA = 1146639681,
        MFX_EXTBUFF_VPP_DENOISE = 1397313092,
        MFX_EXTBUFF_VPP_SCENE_ANALYSIS = 1498170195,
        MFX_EXTBUFF_VPP_SCENE_CHANGE = 1498170195,
        MFX_EXTBUFF_VPP_PROCAMP = 1347240272,
        MFX_EXTBUFF_VPP_DETAIL = 542393668,
        MFX_EXTBUFF_VIDEO_SIGNAL_INFO = 1313428310,
        MFX_EXTBUFF_VPP_DOUSE = 1163089220,
        MFX_EXTBUFF_OPAQUE_SURFACE_ALLOCATION = 1397837903,
        MFX_EXTBUFF_AVC_REFLIST_CTRL = 1414745170,
        MFX_EXTBUFF_VPP_FRAME_RATE_CONVERSION = 541282886,
        MFX_EXTBUFF_PICTURE_TIMING_SEI = 1163088976,
        MFX_EXTBUFF_AVC_TEMPORAL_LAYERS = 1280136257,
        MFX_EXTBUFF_CODING_OPTION2 = 844055619,
        MFX_EXTBUFF_VPP_IMAGE_STABILIZATION = 1112822601,
        MFX_EXTBUFF_VPP_PICSTRUCT_DETECTION = 1413825609,
        MFX_EXTBUFF_ENCODER_CAPABILITY = 1346588229,
        MFX_EXTBUFF_ENCODER_RESET_OPTION = 1330794053,
        MFX_EXTBUFF_ENCODED_FRAME_INFO = 1229344325,
        MFX_EXTBUFF_VPP_COMPOSITE = 1347240790,
        MFX_EXTBUFF_VPP_VIDEO_SIGNAL_INFO = 1230198358,
        MFX_EXTBUFF_ENCODER_ROI = 1229935173,
        MFX_EXTBUFF_VPP_DEINTERLACING = 1229213782,
        MFX_EXTBUFF_AVC_REFLISTS = 1398033490,
        MFX_EXTBUFF_VPP_FIELD_PROCESSING = 1330794566,
        MFX_EXTBUFF_CODING_OPTION3 = 860832835,
        MFX_EXTBUFF_CHROMA_LOC_INFO = 1313426499,
        MFX_EXTBUFF_MBQP = 1347502669,
        MFX_EXTBUFF_HEVC_TILES = 1412773426,
        MFX_EXTBUFF_MB_DISABLE_SKIP_MAP = 1297302605,
        MFX_EXTBUFF_HEVC_PARAM = 1345664562,
        MFX_EXTBUFF_DECODED_FRAME_INFO = 1229342020,
        MFX_EXTBUFF_TIME_CODE = 1145261396,
        MFX_EXTBUFF_HEVC_REGION = 1379218994,
    }


    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxEncodeCtrl
    {
        [FieldOffset(0)]
        public mfxExtBuffer Header;
        //WARN: array reserved of type UInt32 not included;
        [FieldOffset(28)]
        public UInt16 SkipFrame;
        [FieldOffset(30)]
        public UInt16 QP;
        [FieldOffset(32)]
        public UInt16 FrameType;
        [FieldOffset(34)]
        public UInt16 NumExtParam;
        [FieldOffset(36)]
        public UInt16 NumPayload;
        [FieldOffset(38)]
        public UInt16 reserved2;
    }


    public unsafe partial struct mfxFrameData
    {
        [FieldOffset(0)]
        public UInt64 reserved2;
        [FieldOffset(8)]
        public UInt16 NumExtParam;
        //WARN: array reserved of type UInt16 not included;
        [FieldOffset(30)]
        public UInt16 PitchHigh;
        [FieldOffset(32)]
        public UInt64 TimeStamp;
        [FieldOffset(40)]
        public UInt32 FrameOrder;
        [FieldOffset(44)]
        public UInt16 Locked;
        [FieldOffset(46)]
        public UInt16 Pitch;
        [FieldOffset(46)]
        public UInt16 PitchLow;
        [FieldOffset(48)]
        public IntPtr Y;
        [FieldOffset(48)]
        public byte* Y_ptr;
        [FieldOffset(48)]
        public IntPtr Y16;
        [FieldOffset(48)]
        public UInt16* Y16_ptr;
        [FieldOffset(48)]
        public IntPtr R;
        [FieldOffset(48)]
        public byte* R_ptr;
        [FieldOffset(56)]
        public IntPtr UV;
        [FieldOffset(56)]
        public byte* UV_ptr;
        [FieldOffset(56)]
        public IntPtr VU;
        [FieldOffset(56)]
        public byte* VU_ptr;
        [FieldOffset(56)]
        public IntPtr CbCr;
        [FieldOffset(56)]
        public byte* CbCr_ptr;
        [FieldOffset(56)]
        public IntPtr CrCb;
        [FieldOffset(56)]
        public byte* CrCb_ptr;
        [FieldOffset(56)]
        public IntPtr Cb;
        [FieldOffset(56)]
        public byte* Cb_ptr;
        [FieldOffset(56)]
        public IntPtr U;
        [FieldOffset(56)]
        public byte* U_ptr;
        [FieldOffset(56)]
        public IntPtr U16;
        [FieldOffset(56)]
        public UInt16* U16_ptr;
        [FieldOffset(56)]
        public IntPtr G;
        [FieldOffset(56)]
        public byte* G_ptr;
        [FieldOffset(64)]
        public IntPtr Cr;
        [FieldOffset(64)]
        public byte* Cr_ptr;
        [FieldOffset(64)]
        public IntPtr V;
        [FieldOffset(64)]
        public byte* V_ptr;
        [FieldOffset(64)]
        public IntPtr V16;
        [FieldOffset(64)]
        public UInt16* V16_ptr;
        [FieldOffset(64)]
        public IntPtr B;
        [FieldOffset(64)]
        public byte* B_ptr;
        [FieldOffset(72)]
        public IntPtr A;
        [FieldOffset(72)]
        public byte* A_ptr;
        [FieldOffset(88)]
        public UInt16 Corrupted;
        [FieldOffset(90)]
        public UInt16 DataFlag;
    }


    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxExtVPPComposite
    {
        [FieldOffset(0)]
        public mfxExtBuffer Header;
        [FieldOffset(8)]
        public UInt16 Y;
        [FieldOffset(8)]
        public UInt16 R;
        [FieldOffset(10)]
        public UInt16 U;
        [FieldOffset(10)]
        public UInt16 G;
        [FieldOffset(12)]
        public UInt16 V;
        [FieldOffset(12)]
        public UInt16 B;
        //WARN: array reserved1 of type UInt16 not included;
        [FieldOffset(62)]
        public UInt16 NumInputStream;
        [FieldOffset(64)]
        public IntPtr InputStream;
        [FieldOffset(64)]
        public mfxVPPCompInputStream* InputStream_ptr;

    }


    [StructLayout(LayoutKind.Explicit, Size = 64, Pack = 1)]
    public unsafe struct mfxVPPCompInputStream
    {
        [FieldOffset(0)]
        public UInt32 DstX;
        [FieldOffset(4)]
        public UInt32 DstY;
        [FieldOffset(8)]
        public UInt32 DstW;
        [FieldOffset(12)]
        public UInt32 DstH;
        [FieldOffset(16)]
        public UInt16 LumaKeyEnable;
        [FieldOffset(18)]
        public UInt16 LumaKeyMin;
        [FieldOffset(20)]
        public UInt16 LumaKeyMax;
        [FieldOffset(22)]
        public UInt16 GlobalAlphaEnable;
        [FieldOffset(24)]
        public UInt16 GlobalAlpha;
        [FieldOffset(26)]
        public UInt16 PixelAlphaEnable;
        // [FieldOffset(28)]
        // public unsigned short [18] reserved2;
    }



    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxExtVppAuxData
    {
        [FieldOffset(0)]
        public mfxExtBuffer Header;
        [FieldOffset(8)]
        public UInt32 SpatialComplexity;
        [FieldOffset(12)]
        public UInt32 TemporalComplexity;
        [FieldOffset(8)]
        public PicStruct PicStruct;
        //WARN: array reserved of type UInt16 not included;
        [FieldOffset(16)]
        public UInt16 SceneChangeRate;
        [FieldOffset(18)]
        public UInt16 RepeatedFrame;
    }
    [StructLayout(LayoutKind.Explicit, Size=72, Pack = 1)]
    public unsafe struct mfxBitstream
    {
        [FieldOffset(0)]
        public IntPtr EncryptedData;
        [FieldOffset(0)]
        public byte* EncryptedData_ptr;
        //unfixed
        ///Typedef/Struct/Field/Union/Field/Struct/Field/PointerType/PointerType/Typedef/Struct
        // <Field id="_1419" name="ExtParam" type="_1171" context="_1007" access="public" location="f3:99" file="f3" line="99" offset="64" isfilled="yes">
        //   <PointerType id="_1171" type="_1538" isfilled="yes">
        //     <PointerType id="_1538" type="_31" isfilled="yes">
        //       <Typedef id="_31" name="mfxExtBuffer" type="_30" context="_1" location="f3:49" file="f3" line="49" isfilled="yes">
        //         <Struct id="_30" name="" context="_1" location="f3:46" file="f3" line="46" members="_270 _271 _272 _273 _274 _275" size="64" align="32" isfilled="yes" />
        //       </Typedef>
        //     </PointerType>
        //   </PointerType>
        // </Field>
        [FieldOffset(16)]
        public UInt16 NumExtParam;
        //WARN: array reserved of type UInt32 not included;
        [FieldOffset(24)]
        public Int64 DecodeTimeStamp;
        [FieldOffset(32)]
        public UInt64 TimeStamp;
        [FieldOffset(40)]
        public IntPtr Data;
        [FieldOffset(40)]
        public byte* Data_ptr;
        [FieldOffset(48)]
        public UInt32 DataOffset;
        [FieldOffset(52)]
        public UInt32 DataLength;
        [FieldOffset(56)]
        public UInt32 MaxLength;
        [FieldOffset(60)]
        public PicStruct PicStruct;
        [FieldOffset(62)]
        public UInt16 FrameType;
        [FieldOffset(64)]
        public UInt16 DataFlag;
        [FieldOffset(66)]
        public UInt16 reserved2;
    }
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxFrameInfo
    {
        //WARN: array reserved of type UInt32 not included;
        [FieldOffset(16)]
        public UInt16 reserved4;
        [FieldOffset(18)]
        public UInt16 BitDepthLuma;
        [FieldOffset(20)]
        public UInt16 BitDepthChroma;
        [FieldOffset(22)]
        public UInt16 Shift;
        [FieldOffset(24)]
        public mfxFrameId FrameId;
        [FieldOffset(32)]
        public FourCC FourCC;
        [FieldOffset(36)]
        public UInt16 Width;
        [FieldOffset(38)]
        public UInt16 Height;
        [FieldOffset(40)]
        public UInt16 CropX;
        [FieldOffset(42)]
        public UInt16 CropY;
        [FieldOffset(44)]
        public UInt16 CropW;
        [FieldOffset(46)]
        public UInt16 CropH;
        [FieldOffset(36)]
        public UInt64 BufferSize;
        [FieldOffset(44)]
        public UInt32 reserved5;
        [FieldOffset(48)]
        public UInt32 FrameRateExtN;
        [FieldOffset(52)]
        public UInt32 FrameRateExtD;
        [FieldOffset(56)]
        public UInt16 reserved3;
        [FieldOffset(58)]
        public UInt16 AspectRatioW;
        [FieldOffset(60)]
        public UInt16 AspectRatioH;
        [FieldOffset(62)]
        public PicStruct PicStruct;
        [FieldOffset(64)]
        public ChromaFormat ChromaFormat;
        [FieldOffset(66)]
        public UInt16 reserved2;
    }
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxInfoMFX
    {
        //WARN: array reserved of type UInt32 not included;
        [FieldOffset(28)]
        public UInt16 LowPower;
        [FieldOffset(30)]
        public UInt16 BRCParamMultiplier;
        [FieldOffset(32)]
        public mfxFrameInfo FrameInfo;
        [FieldOffset(100)]
        public CodecId CodecId;
        [FieldOffset(104)]
        public UInt16 CodecProfile;
        [FieldOffset(106)]
        public UInt16 CodecLevel;
        [FieldOffset(108)]
        public UInt16 NumThread;
        [FieldOffset(110)]
        public TargetUsage TargetUsage;
        [FieldOffset(112)]
        public UInt16 GopPicSize;
        [FieldOffset(114)]
        public UInt16 GopRefDist;
        [FieldOffset(116)]
        public UInt16 GopOptFlag;
        [FieldOffset(118)]
        public UInt16 IdrInterval;
        [FieldOffset(120)]
        public RateControlMethod RateControlMethod;
        [FieldOffset(122)]
        public UInt16 InitialDelayInKB;
        [FieldOffset(122)]
        public UInt16 QPI;
        [FieldOffset(122)]
        public UInt16 Accuracy;
        [FieldOffset(124)]
        public UInt16 BufferSizeInKB;
        [FieldOffset(126)]
        public UInt16 TargetKbps;
        [FieldOffset(126)]
        public UInt16 QPP;
        [FieldOffset(126)]
        public UInt16 ICQQuality;
        [FieldOffset(128)]
        public UInt16 MaxKbps;
        [FieldOffset(128)]
        public UInt16 QPB;
        [FieldOffset(128)]
        public UInt16 Convergence;
        [FieldOffset(130)]
        public UInt16 NumSlice;
        [FieldOffset(132)]
        public UInt16 NumRefFrame;
        [FieldOffset(134)]
        public UInt16 EncodedOrder;
        [FieldOffset(110)]
        public UInt16 DecodedOrder;
        [FieldOffset(112)]
        public UInt16 ExtendedPicStruct;
        [FieldOffset(114)]
        public UInt16 TimeStampCalc;
        [FieldOffset(116)]
        public UInt16 SliceGroupsPresent;
        //WARN: array reserved2 of type UInt16 not included;
        [FieldOffset(110)]
        public UInt16 JPEGChromaFormat;
        [FieldOffset(112)]
        public UInt16 Rotation;
        [FieldOffset(114)]
        public UInt16 JPEGColorFormat;
        [FieldOffset(116)]
        public UInt16 InterleavedDec;
        //WARN: array reserved3 of type UInt16 not included;
        [FieldOffset(110)]
        public UInt16 Interleaved;
        [FieldOffset(112)]
        public UInt16 Quality;
        [FieldOffset(114)]
        public UInt16 RestartInterval;
        //WARN: array reserved5 of type UInt16 not included;
    }
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxInfoVPP
    {
        //WARN: array reserved of type UInt32 not included;
        [FieldOffset(32)]
        public mfxFrameInfo In;
        [FieldOffset(100)]
        public mfxFrameInfo Out;
    }
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxExtBuffer
    {
        [FieldOffset(0)]
        public BufferId BufferId;
        [FieldOffset(4)]
        public UInt32 BufferSz;
    }
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct mfxVersion
    {
        [FieldOffset(0)]
        public UInt16 Minor;
        [FieldOffset(2)]
        public UInt16 Major;
        [FieldOffset(0)]
        public UInt32 Version;


    }
}
