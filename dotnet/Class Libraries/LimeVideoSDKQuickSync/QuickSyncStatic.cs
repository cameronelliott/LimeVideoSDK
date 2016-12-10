// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory


using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;


namespace LimeVideoSDKQuickSync
{
    /// <summary>
    /// Static methods for Quick Sync operations
    /// </summary>
    unsafe public partial class QuickSyncStatic
    {
        // On Windows:
        // This binds the this Class Library to the Native DLL via CPP/CLI
        // This means:
        // Visual Studio will copy the Native DLL to the output folders of any project referencing this project.
        // YOU WANT THIS, OTHERWISE WHEN YOU RUN ANY PROJECT REFERENCING THIS DLL, THE NATIVE DLL WILL NOT BE FOUND!
        // AND ANY PROGRAM THAT TRYS TO USE THIS DLL WILL CRASH CAUSE IT CANNOT LOAD/PINVOKE THE NATIVE DLL.
        //
        // If this is failing to compile:
        // It means your file reference in this project is broken: bad path, missing native DLL, etc...
//#if !DEBUG
#pragma warning disable 169
#if true
        DummyReferenceClass __DummyReferenceClass;
#endif
#pragma warning restore 169
//#endif


        /// <summary>Helper method that throws an exception when a status code reflects an error.</summary>
        /// <param name="sts">The STS.</param>
        /// <param name="msg">The MSG.</param>
        /// <exception cref="LimeVideoSDKQuickSyncException"></exception>
        public static void ThrowOnBadStatus(mfxStatus sts, string msg)
        {
            if (sts < 0)
                throw new LimeVideoSDKQuickSyncException(msg, sts);
        }

        /// <summary>Attempts to decode a stream using codecId as the format indicator.
        /// Only enough of the stream is decoded to return stream parameters such as width, height, etc...</summary>
        /// <param name="stream">The stream.</param>
        /// <param name="codecId">The codec identifier.</param>
        /// <param name="impl">The implementation.</param>
        /// <returns>A video parameter structure describing the bitstream.</returns>
        public static mfxVideoParam DecodeHeader(Stream stream, CodecId codecId, mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO)
        {
            var buf = new byte[65536]; //avail after init
            int n = stream.Read(buf, 0, buf.Length);
            if (n < buf.Length)
                Array.Resize(ref buf, n);

            return DecodeHeader(buf, codecId, impl);
        }


        /// <summary>Attempts to decode a byte array using codecId as the format indicator.
        /// If the array is decodable stream parameters such as width, height, etc... will be returned</summary>
        /// <param name="bitstream">The bitstream.</param>
        /// <param name="codecId">The codec identifier.</param>
        /// <param name="impl">The implementation.</param>
        /// <returns>A video parameter structure describing the bitstream.</returns>
        public static mfxVideoParam DecodeHeader(byte[] bitstream, CodecId codecId, mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO)
        {
            mfxVideoParam mfxDecParam;

            mfxStatus sts;
            var v = new mfxVersion();
            v.Major = 1;
            v.Minor = 0;

            var session = new mfxSession();

            sts = UnsafeNativeMethods.MFXInit(impl, &v, &session);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXInit");

            try
            {
                mfxBitstream bs;
                mfxDecParam.mfx.CodecId = codecId;
                mfxDecParam.IOPattern = IOPattern.MFX_IOPATTERN_OUT_SYSTEM_MEMORY;

                fixed (byte* pp = &bitstream[0])
                {
                    // bs.Data_ptr = p;
                    bs.Data = (IntPtr)pp;
                    bs.DataLength = (uint)bitstream.Length;
                    bs.MaxLength = (uint)bitstream.Length;
                    bs.DataOffset = 0;

                    sts = UnsafeNativeMethods.MFXVideoDECODE_DecodeHeader(session, &bs, &mfxDecParam);
                    QuickSyncStatic.ThrowOnBadStatus(sts, "decodeheader");
                }
            }
            finally
            {
                UnsafeNativeMethods.MFXClose(session);
            }

            mfxDecParam.IOPattern = (IOPattern)0;       // we do not want this to be the source of IOPattern
                                                        // must be set it another place so it doesnt default to sysmem
            return mfxDecParam;
        }

        /// <summary>Reads the file header information.</summary>
        /// <param name="codecId">The codec identifier.</param>
        /// <param name="impl">The implementation.</param>
        /// <param name="infs">The infs.</param>
        /// <param name="outIOPattern">The out io pattern.</param>
        /// <returns></returns>
        public static unsafe mfxVideoParam ReadFileHeaderInfo(CodecId codecId, mfxIMPL impl, Stream infs, IOPattern outIOPattern)
        {
            long oldposition = infs.Position;

            var buf = new byte[65536]; //avail after init
            int n = infs.Read(buf, 0, buf.Length);
            if (n < buf.Length)
                Array.Resize(ref buf, n);

            infs.Position = oldposition;
            var decoderParameters = QuickSyncStatic.DecodeHeader(buf, codecId, impl);
            decoderParameters.IOPattern = outIOPattern;
            return decoderParameters;
        }


        /// <summary>make sure x is divisible by 16, if not rounds up to next multiple of 16</summary>
        public static ushort ALIGN16(int x)
        {
            return (ushort)((x + 15) / 16 * 16);
        }

        /// <summary>make sure x is divisible by 32, if not rounds up to next multiple of 32</summary>
        public static ushort ALIGN32(int x)
        {
            return (ushort)((x + 31) / 32 * 32);
        }


        /// <summary>
        /// Rounds height upto value divisible by 16,
        /// or 32 in the case where picstruct is MFX_PICSTRUCT_PROGRESSIVE
        /// </summary>
        /// <param name="height"></param>
        /// <param name="picstruct">Used to decide 16 or 32 for rounding up</param>
        /// <returns></returns>
        public static ushort AlignHeightTo32or16(int height, PicStruct picstruct)
        {
            ushort v =
                (PicStruct.MFX_PICSTRUCT_PROGRESSIVE == picstruct) ?
                (ushort)QuickSyncStatic.ALIGN16(height) :
                (ushort)QuickSyncStatic.ALIGN32(height);

            return v;
        }

        /// <summary>Get a human readable implementation description.</summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public static string ImplementationString(mfxSession session)
        {
            mfxIMPL impl;
            var sts = UnsafeNativeMethods.MFXQueryIMPL(session, &impl);
            QuickSyncStatic.ThrowOnBadStatus(sts, nameof(UnsafeNativeMethods.MFXQueryIMPL));

            return ImplementationString(impl);
        }

        /// <summary>
        /// generate human readable description of an mfxIMPL type
        /// </summary>
        /// <param name="impl"></param>
        public static string ImplementationString(mfxIMPL impl)
        {
            string part1 = ((mfxIMPL)((int)impl & 0xff)).ToString();

            if (part1.Contains("SOFTWARE"))
                part1 = "CPU: " + part1;
            if (part1.Contains("HARDWARE"))
                part1 = "GPU: " + part1;

            impl = (mfxIMPL)((int)impl & ~0xff);

            switch (impl)
            {

                case mfxIMPL.MFX_IMPL_VIA_ANY:
                    part1 += "|VIA_ANY";
                    break;
                case mfxIMPL.MFX_IMPL_VIA_D3D9:
                    part1 += "|VIA_D3D9";
                    break;
                case mfxIMPL.MFX_IMPL_VIA_D3D11:
                    part1 += "|VIA_D3D11";
                    break;
                case mfxIMPL.MFX_IMPL_VIA_VAAPI:
                    part1 += "|VIA_VAAPI";
                    break;
                case mfxIMPL.MFX_IMPL_AUDIO:
                    part1 += "|AUDIO";
                    break;
                default:
                    break;
            }
            return part1;
        }


        /// <summary>
        /// generate human readable description of an DeviceSetup.FrameMemType type
        /// </summary>

       // public static string ImplementationString(DeviceSetup.FrameMemType memtype)
              public static string ImplementationString()
        {
            // no bit masks makes this easy
            //string part1 = ((DeviceSetup.FrameMemType)(memtype)).ToString();
            string part1 = "TBD: not implemented";
            part1 = "Memory Type: " + part1;
            return part1;
        }
    }
}
