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
using System.Runtime.InteropServices;

namespace Decoder50
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






            string fourccString = fourcc.ToString().Substring(0, 4);
            string inFilename;
            //inFilename = "BigBuckBunny_320x180.UYVY.enc.jpeg";
            inFilename = "BigBuckBunny_1920x1080.UYVY.enc.jpeg";
            //inFilename = "BigBuckBunny_3840x2160.UYVY.enc.jpeg";
            string outFilename = Path.ChangeExtension(inFilename, ".yuv");

            Console.WriteLine("Working directory: {0}", Environment.CurrentDirectory);
            Console.WriteLine("Input filename: {0}", inFilename);
            Console.WriteLine();

            if (!File.Exists(inFilename))
            {
                Console.WriteLine("Input file not found. Press any key to exit.");
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

            var outIOPattern = IOPattern.MFX_IOPATTERN_OUT_SYSTEM_MEMORY;


            // The encoder cannot encode UYVY, but if you are the only decoder of the JPEG
            // files, you can encode UYVY as YUY2 and everything is good.
            if (fourcc == FourCC.UYVY)
                fourcc = FourCC.YUY2;

            mfxVideoParam decoderParameters = QuickSyncStatic.ReadFileHeaderInfo(codecId, impl, infs, outIOPattern);
            decoderParameters.mfx.FrameInfo.FourCC = fourcc;

            AssignChromaFormat(fourcc, ref decoderParameters);
        

            var decoder = new StreamDecoder(infs, decoderParameters, null, impl);

#if ENABLE_BENCHMARK     // delete this code for most simple example
            decoder.benchmarkNeverStopMode = true;
#endif

            string impltext = QuickSyncStatic.ImplementationString(decoder.lowLevelDecoder.session);
            Console.WriteLine("Implementation = {0}", impltext);

            // not needed
            //var formatConverter = new NV12ToXXXXConverter(fourcc, decoder.width, decoder.height);

            int width = decoderParameters.mfx.FrameInfo.CropW;
            int height = decoderParameters.mfx.FrameInfo.CropH;
            var tmpbuf = new byte[width * height * 2];

            int count = 0;

            foreach (var frame in decoder.GetFrames())
            {
                //var frameBytes = formatConverter.ConvertFromNV12(frame.Data);        // Convert to format requested


                Trace.Assert(frame.Data.Pitch == width * 2);  // yuy2 only

                fixed (byte* aa = &tmpbuf[0])
                    FastMemcpyMemmove.memcpy((IntPtr)aa, frame.Data.Y, height * width * 2);

                outfs.Write(tmpbuf, 0, tmpbuf.Length);

                if (++count % 100 == 0)
                    Console.Write("Frame {0}\r", count);

#if ENABLE_BENCHMARK     // delete this code for most simple example
                if (count > minimumFrames)
                    break;
#endif
            }

            if (bt != null)
                bt.StopAndReport(count, infs.Position, outfs.Position);

            infs.Close();
            outfs.Close();

            Console.WriteLine("Decoded {0} frames", count);

            // make sure program always waits for user, except F5-Release run
            if (Debugger.IsAttached ||
                Environment.GetEnvironmentVariable("VisualStudioVersion") == null)
            {
                Console.WriteLine("done - press a key to exit");
                Console.ReadKey();
            }
        }

      

        private static void AssignChromaFormat(FourCC decoderFourcc, ref mfxVideoParam decoderParameters)
        {
            switch (decoderFourcc)
            {
                case FourCC.NV12:
                case FourCC.YV12:
                    decoderParameters.mfx.FrameInfo.ChromaFormat = ChromaFormat.MFX_CHROMAFORMAT_YUV420;
                    break;
                case FourCC.YUY2:
                    decoderParameters.mfx.FrameInfo.ChromaFormat = ChromaFormat.MFX_CHROMAFORMAT_YUV422;
                    break;
                case FourCC.RGB4:
                    decoderParameters.mfx.FrameInfo.ChromaFormat = ChromaFormat.MFX_CHROMAFORMAT_YUV444;
                    break;
                default:
                    Trace.Assert(false);
                    break;
            }
        }
    }
}
