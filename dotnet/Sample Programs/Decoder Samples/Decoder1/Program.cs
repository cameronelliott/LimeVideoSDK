// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory


// commented out - create disk output 
// uncommented -  no disk output, and display benchmark measurements
//#define ENABLE_BENCHMARK


using LimeVideoSDKQuickSync;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;



namespace Decoder1
{
    public class Program
    {
        static public void Main()
        {
            ConfirmQuickSyncReadiness.HaltIfNotReady();

            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // keep ascending directories until 'media' folder is found
            for (int i = 0; i < 10 && !Directory.Exists("Media"); i++)
                Directory.SetCurrentDirectory("..");
            Directory.SetCurrentDirectory("Media");

            mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO;  //automatic GPU/CPU mode
            CodecId codecId = CodecId.MFX_CODEC_AVC;
            // avc fourcc supported: RGB3 RGB4 BGR4 BGR3 NV12 I420 IYUV YUY2 UYVY YV12 P411 P422  
            FourCC fourcc = FourCC.NV12;
            string fourccString = fourcc.ToString().Substring(0, 4);

            string inFilename;
            inFilename = "BigBuckBunny_320x180.264";
            //inFilename = "BigBuckBunny_1920x1080.264";
            //inFilename = "BigBuckBunny_3840x2160.264";
            string outFilename = Path.ChangeExtension(inFilename, fourccString + ".yuv");

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
            // maximumMemoryToAllocate = (long)4L * 1024 * 1024 * 1024;
            Console.WriteLine("Pre-reading input");
            infs = new PreReadLargeMemoryStream(File.Open(inFilename, FileMode.Open));
            Console.WriteLine("Input read");

            outfs = new NullStream();
            bt = new BenchmarkTimer();
            bt.Start();

            //int minimumFrames = 4000;
#endif

            Console.WriteLine("Output filename: {0}",
                Path.GetFileName((outfs as FileStream)?.Name ?? "NO OUTPUT"));
            Console.WriteLine();

            var outIOPattern = IOPattern.MFX_IOPATTERN_OUT_SYSTEM_MEMORY;

            mfxVideoParam decoderParameters = QuickSyncStatic.ReadFileHeaderInfo(codecId, impl, infs, outIOPattern);
            decoderParameters.mfx.FrameInfo.FourCC = fourcc;

            var decoder = new StreamDecoder(infs, CodecId.MFX_CODEC_AVC, impl, outIOPattern);

            string impltext = QuickSyncStatic.ImplementationString(decoder.lowLevelDecoder.session);
            Console.WriteLine("Implementation = {0}", impltext);





            var formatConverter = new NV12ToXXXXConverter(fourcc, decoder.width, decoder.height);




            int count = 0;



            foreach (var frame in decoder.GetFrames())
            {
                var frameBytes = formatConverter.ConvertFromNV12(frame.Data);        // Convert to format requested
                outfs.Write(frameBytes, 0, frameBytes.Length);



                if (++count % 100 == 0)
                    Console.Write("Frame {0}\r", count);


            }
            Console.WriteLine("Decoded {0} frames", count);
            Console.WriteLine();

            if (bt != null)
                bt.StopAndReport(count, infs.Position, outfs.Position);

            infs.Close();
            outfs.Close();






            // make sure program always waits for user, except F5-Release run
            if (!UnitTest.IsRunning && Debugger.IsAttached ||
                Environment.GetEnvironmentVariable("VisualStudioVersion") == null)
            {
                Console.WriteLine("done - press a key to exit");
                Console.ReadKey();
            }
        }
    }
}
