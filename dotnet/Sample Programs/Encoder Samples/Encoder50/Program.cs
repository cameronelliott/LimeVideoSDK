// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

#pragma warning disable CS1591 // Missing XML comment warnings

// commented out - create disk output 
// uncommented -  no disk output, and display benchmark measurements
//#define ENABLE_BENCHMARK

using LimeVideoSDKQuickSync;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using LowLevelEncoder = LimeVideoSDKQuickSync.LowLevelEncoderCSharp;
//using LowLevelEncoder = LimeVideoSDKQuickSync.LowLevelEncoderNative;


namespace Encoder50
{
    class Program
    {

        unsafe static void Main(string[] args)
        {
            ConfirmQuickSyncReadiness.HaltIfNotReady();

            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // keep ascending directories until 'media' folder is found
            for (int i = 0; i < 10 && !Directory.Exists("Media"); i++)
                Directory.SetCurrentDirectory("..");
            Directory.SetCurrentDirectory("Media");


            CodecId codecId = CodecId.MFX_CODEC_JPEG;
            FourCC fourcc = FourCC.UYVY;    // supported: RGB4, YUY2 NV12 [UYVY through tricks! see below]
            mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO;


            int width, height;
            string inFilename;
            //inFilename = "BigBuckBunny_320x180." + fourcc + ".yuv"; width = 320; height = 180;
            inFilename = "BigBuckBunny_1920x1080." + fourcc + ".yuv"; width = 1920; height = 1080;
            string outFilename = Path.ChangeExtension(inFilename, "enc.jpeg");


            Console.WriteLine("Working directory: {0}", Environment.CurrentDirectory);
            Console.WriteLine("Input filename: {0}", inFilename);       
            Console.WriteLine("Input width: {0}  Input height: {1}", width, height);
     

            if (!File.Exists(inFilename))
            {
                Console.WriteLine("Input file not found.");
                Console.WriteLine("Please let Decoder1 run to completion to create input file");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }


            Stream infs, outfs;
            BenchmarkTimer bt = null;
          

#if !ENABLE_BENCHMARK

            infs = File.Open(inFilename, FileMode.Open);
            outfs = File.Open(outFilename, FileMode.Create);

#else       // delete this code for most simple example

            // * Benchmark Mode *
            // this block does a couple things:
            //   1. causes the file to be pre-read into memory so we are not timing disk reads.
            //   2. replaces the output stream with a NullStream so nothing gets written to disk.
            //   3. Starts the timer for benchmarking
            // this pre-reads file into memory for benchmarking
            long maximumMemoryToAllocate = (long)4L * 1024 * 1024 * 1024;
            Console.WriteLine("Pre-reading input");
            infs = new PreReadLargeMemoryStream(File.Open(inFilename, FileMode.Open), maximumMemoryToAllocate);
            Console.WriteLine("Input read");

            outfs = new NullStream();
            bt = new BenchmarkTimer();
            bt.Start();

            int minimumFrames = 4000;
#endif

            Console.WriteLine("Output filename: {0}",
                Path.GetFileName((outfs as FileStream)?.Name ?? "NO OUTPUT"));
            Console.WriteLine();

            // The encoder cannot encode UYVY, but if you are the only decoder of the JPEG
            // files, you can encode UYVY as YUY2 and everything is good.
            if (fourcc == FourCC.UYVY)
                fourcc = FourCC.YUY2;


            mfxVideoParam mfxEncParams = new mfxVideoParam();
            mfxEncParams.mfx.CodecId = codecId;
            mfxEncParams.mfx.TargetUsage = TargetUsage.MFX_TARGETUSAGE_BALANCED;
            //mfxEncParams.mfx.TargetKbps = 2000;
            //mfxEncParams.mfx.RateControlMethod = RateControlMethod.MFX_RATECONTROL_VBR;
            mfxEncParams.mfx.Quality = 90;
            mfxEncParams.mfx.Interleaved = 1;
            mfxEncParams.mfx.FrameInfo.FrameRateExtN = 30;
            mfxEncParams.mfx.FrameInfo.FrameRateExtD = 1;
            mfxEncParams.mfx.FrameInfo.FourCC = fourcc;
            

            switch (fourcc)
            {
                case FourCC.NV12:
                case FourCC.YV12:
                    mfxEncParams.mfx.FrameInfo.ChromaFormat = ChromaFormat.MFX_CHROMAFORMAT_YUV420;
                    break;
                case FourCC.YUY2:
                    mfxEncParams.mfx.FrameInfo.ChromaFormat = ChromaFormat.MFX_CHROMAFORMAT_YUV422V; // fatal on SKYLAKE!
                    mfxEncParams.mfx.FrameInfo.ChromaFormat = ChromaFormat.MFX_CHROMAFORMAT_YUV422;
                    break;
                case FourCC.RGB4:
                    mfxEncParams.mfx.FrameInfo.ChromaFormat = ChromaFormat.MFX_CHROMAFORMAT_YUV444;
                    break;
                default:
                    Trace.Assert(false);
                    break;
            }


            mfxEncParams.mfx.FrameInfo.PicStruct = PicStruct.MFX_PICSTRUCT_PROGRESSIVE;
            mfxEncParams.mfx.FrameInfo.CropX = 0;
            mfxEncParams.mfx.FrameInfo.CropY = 0;
            mfxEncParams.mfx.FrameInfo.CropW = (ushort)width;
            mfxEncParams.mfx.FrameInfo.CropH = (ushort)height;
            // Width must be a multiple of 16
            // Height must be a multiple of 16 in case of frame picture and a multiple of 32 in case of field picture
            mfxEncParams.mfx.FrameInfo.Width = QuickSyncStatic.ALIGN16(width);
            mfxEncParams.mfx.FrameInfo.Height = QuickSyncStatic.AlignHeightTo32or16(height, mfxEncParams.mfx.FrameInfo.PicStruct);
            mfxEncParams.IOPattern = IOPattern.MFX_IOPATTERN_IN_SYSTEM_MEMORY; // must be 'in system memory'
            mfxEncParams.AsyncDepth = 4;   // Pipeline depth. Best at 4


            mfxEncParams.mfx.FrameInfo.Width = QuickSyncStatic.ALIGN32(width);
            mfxEncParams.mfx.FrameInfo.Height = QuickSyncStatic.ALIGN32(height);


            BitStreamChunk bsc = new BitStreamChunk(); //where we receive compressed frame data

            ILowLevelEncoder encoder = new LowLevelEncoder(mfxEncParams, impl);
            //ILowLevelEncoder encoder = new LowLevelEncoder(mfxEncParams, impl);


            string impltext = QuickSyncStatic.ImplementationString(encoder.session);
            Console.WriteLine("Implementation = {0}", impltext);


            // not needed for YUY2 encoding
            //var formatConverter = new NV12FromXXXXConverter(fileFourcc, width, height);


            int inputFrameLength = width * height * VideoUtility.GetBitsPerPixel(fourcc) / 8;

            byte[] uncompressed = new byte[inputFrameLength];

            int count = 0;

            // we do not call encoder.LockFrame() and encoder.UnlockFrame() as this example is 
            // for system memory.


            while (infs.Read(uncompressed, 0, inputFrameLength) == inputFrameLength)
            {
                int ix = encoder.GetFreeFrameIndex();  //this call relys locks in authoritative array of surf        

                //formatConverter.ConvertToNV12FrameSurface(ref encoder.Frames[ix], uncompressed, 0);
                mfxFrameSurface1* f = (mfxFrameSurface1*)encoder.Frames[ix];


                switch (fourcc)
                {
                    case FourCC.NV12:
                        Trace.Assert(f->Data.Pitch == width * 1);
                        fixed (byte* aa = &uncompressed[0])
                            FastMemcpyMemmove.memcpy(f->Data.Y, (IntPtr)aa, height * width);
                        fixed (byte* aa = &uncompressed[height * width])
                            FastMemcpyMemmove.memcpy(f->Data.UV, (IntPtr)aa, height / 2 * width);
                        break;
                    case FourCC.YUY2:

                        Trace.Assert(f->Data.Pitch == width * 2);
                        fixed (byte* aa = &uncompressed[0])
                            FastMemcpyMemmove.memcpy(f->Data.Y, (IntPtr)aa, height * width * 2);
                        break;
                    default:
                        Trace.Assert(false);
                        break;
                }

                encoder.EncodeFrame(ix, ref bsc);

                if (bsc.bytesAvailable > 0)
                {
                    outfs.Write(bsc.bitstream, 0, bsc.bytesAvailable);

                    if (++count % 100 == 0)
                        Console.Write("Frame {0}\r", count);
                }

#if ENABLE_BENCHMARK     // delete this code for most simple example
                if (infs.Position + inputFrameLength - 1 >= infs.Length)
                    infs.Position = 0;
                if (count >= minimumFrames)
                    break;
#endif
            }

            while (encoder.Flush(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                {
                    outfs.Write(bsc.bitstream, 0, bsc.bytesAvailable);

                    if (++count % 100 == 0)
                        Console.Write("Frame {0}\r", count);
                }
            }

            if (bt != null)
                bt.StopAndReport(count, infs.Position, outfs.Position);

            infs.Close();
            outfs.Close();

            encoder.Dispose();

            Console.WriteLine("Encoded {0} frames", count);

            // make sure program always waits for user, except F5-Release run
            if (Debugger.IsAttached ||
                Environment.GetEnvironmentVariable("VisualStudioVersion") == null)
            {
                Console.WriteLine("done - press a key to exit");
                Console.ReadKey();
            }
        }
    }
}
