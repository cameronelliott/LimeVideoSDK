// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using LimeVideoSDKQuickSync;
using System;
using System.Diagnostics;
using System.IO;



namespace Transcoder3
{

    public class Program
    {
        static public void Main(string[] args)
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // keep ascending directories until 'media' folder is found
            for (int i = 0; i < 10 && !Directory.Exists("Media"); i++)
                Directory.SetCurrentDirectory("..");
            Directory.SetCurrentDirectory("Media");

            mfxIMPL impl = mfxIMPL.MFX_IMPL_AUTO;
            CodecId inputCodecId = CodecId.MFX_CODEC_AVC;
            CodecId outputCodecId = CodecId.MFX_CODEC_AVC;
            string outputExtension = ".transcoded.264";//this should match codecld above


            string inFilename = "BigBuckBunny_320x180.264";
            //string inFilename = "BigBuckBunny_1920x1080.264";
            string outFilename = Path.ChangeExtension(inFilename, outputExtension);

            Console.WriteLine("Working directory: {0}", Environment.CurrentDirectory);
            Console.WriteLine("Input filename: {0}", inFilename);
            Console.WriteLine("Output filename: {0}", outFilename);
            Console.WriteLine();

            if (!File.Exists(inFilename))
            {
                Console.WriteLine("Input file not found. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            var infs = File.Open(inFilename, FileMode.Open);
            var outfs = File.Open(outFilename, FileMode.Create);


            var config = TranscoderConfiguration.BuildTranscoderConfigurationFromStream(infs,
                inputCodecId,
                outputCodecId);

            var transcoder = new LowLevelTranscoderCSharp(config, impl);

            string impltext = QuickSyncStatic.ImplementationString(transcoder.session);
            Console.WriteLine("Implementation = {0}", impltext);
            //string memtext = QuickSyncStatic.ImplementationString(transcoder.deviceSetup.memType);
            //Console.WriteLine("Memory type = {0}", memtext);

            int count = 0;
            var buf = new byte[transcoder.BufferFreeCount];
            BitStreamChunk bsc = new BitStreamChunk();

            int modulo = 100;



            while (true)
            {
                int free = transcoder.BufferFreeCount;


                if (free > transcoder.BufferSize / 2)
                {
                    int n = infs.Read(buf, 0, free);
                    if (n <= 0)
                        break;
                    transcoder.PutBitstream(buf, 0, n);
                }



                transcoder.GetNextFrame(ref bsc);
                if (bsc.bytesAvailable > 0)
                {
                    outfs.Write(bsc.bitstream, 0, bsc.bytesAvailable);
                    if (++count % modulo == 0)
                        Console.Write("Frames transcoded {0}\r", count);
                }
            }

            while (transcoder.GetNextFrame(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                {
                    outfs.Write(bsc.bitstream, 0, bsc.bytesAvailable);
                    if (++count % modulo == 0)
                        Console.Write("Frames transcoded {0}\r", count);
                }
            }

            while (transcoder.Flush1(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                {
                    outfs.Write(bsc.bitstream, 0, bsc.bytesAvailable);
                    if (++count % modulo == 0)
                        Console.Write("Frames transcoded {0}\r", count);
                }
            }

            while (transcoder.Flush2(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                {
                    outfs.Write(bsc.bitstream, 0, bsc.bytesAvailable);
                    if (++count % modulo == 0)
                        Console.Write("Frames transcoded {0}\r", count);
                }
            }

            while (transcoder.Flush3(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                {
                    outfs.Write(bsc.bitstream, 0, bsc.bytesAvailable);
                    if (++count % modulo == 0)
                        Console.Write("Frames transcoded {0}\r", count);
                }
            }

            while (transcoder.Flush4(ref bsc))
            {
                if (bsc.bytesAvailable > 0)
                {
                    outfs.Write(bsc.bitstream, 0, bsc.bytesAvailable);
                    if (++count % modulo == 0)
                        Console.Write("Frames transcoded {0}\r", count);
                }
            }

            infs.Close();
            outfs.Close();

            Console.WriteLine("Frames transcoded {0}", count);




            if (Debugger.IsAttached)
            {
                Console.WriteLine("done - press a key to exit");
                Console.ReadKey();
            }

        }
    }
}
