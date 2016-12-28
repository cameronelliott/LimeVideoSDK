// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory


// commented out - create disk output 
// uncommented -  no disk output, and display benchmark measurements
#define ENABLE_BENCHMARK



using LimeVideoSDKQuickSync;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace Transcoder1
{


    public class Program
    {
        static public void Main(string[] args)
        {
            ConfirmQuickSyncReadiness.HaltIfNotReady();

            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // keep ascending directories until 'media' folder is found
            for (int i = 0; i < 10 && !Directory.Exists("Media"); i++)
                Directory.SetCurrentDirectory("..");
            Directory.SetCurrentDirectory("Media");

            mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO;
            CodecId inputCodecId = CodecId.MFX_CODEC_AVC;
            CodecId outputCodecId = CodecId.MFX_CODEC_AVC;
            string outputExtension = ".transcoded.264";//this should match codecld above


            string inFilename;
            inFilename = "BigBuckBunny_320x180.264";
            //inFilename = "BigBuckBunny_1920x1080.264";
            //inFilename = "BigBuckBunny_3840x2160.264";
            string outFilename = Path.ChangeExtension(inFilename, outputExtension);

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


            var config = TranscoderConfiguration.BuildTranscoderConfigurationFromStream(infs,
                inputCodecId,
                outputCodecId);

            var transcoder = new StreamTranscoder(infs, config, impl, false);

            string impltext = QuickSyncStatic.ImplementationString(transcoder.lowLevelTranscoder.session);
            Console.WriteLine("Implementation = {0}", impltext);
            //string memtext = QuickSyncStatic.ImplementationString(transcoder.lowLevelTranscoder.deviceSetup.memType);
            //Console.WriteLine("Memory type = {0}", memtext);



            int modulo = 100;

            int count = 0;


            foreach (var item in transcoder.GetFrames())
            {

                outfs.Write(item.bitstream, 0, item.bytesAvailable);

                if (++count % modulo == 0)
                    Console.Write("Frames transcoded {0}\r", count);
            }


            Console.WriteLine("Frames transcoded {0}", count);
            Console.WriteLine();

            if (bt != null)
                bt.StopAndReport(count, infs.Position, outfs.Position);

            infs.Close();
            outfs.Close();



            if (Debugger.IsAttached)
            {
                Console.WriteLine("done - press a key to exit");
                Console.ReadKey();
            }

        }
    }
}
