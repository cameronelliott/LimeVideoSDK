// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory
#pragma warning disable CS1591 // Missing XML comment warnings




using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;



namespace LimeVideoSDKQuickSync
{
    /// <summary>
    /// For benchmarking. Contains a timer, and prints metrics after stopping.
    /// </summary>
    public class BenchmarkTimer
    {
        Stopwatch stopwatch = new Stopwatch();
        TimeSpan t1, t0;


        /// <summary>
        /// Starts timer
        /// </summary>
        public void Start()
        {
            stopwatch.Start();
            t0 = Process.GetCurrentProcess().TotalProcessorTime;

        }

        /// <summary>Stops gimer and writes calculated statistics to Console.</summary>
        /// <param name="numFrames">The number frames.</param>
        /// <param name="bytesin">count of bytes read.</param>
        /// <param name="bytesout">count of bytes written.</param>
        public void StopAndReport(long numFrames, long bytesin, long bytesout)
        {
            Trace.Assert(stopwatch.IsRunning, "BenchmarkHelper never started. Use Start()");

            t1 = Process.GetCurrentProcess().TotalProcessorTime;
            stopwatch.Stop();

            double cpuSec = t1.Subtract(t0).TotalSeconds;

            double adjustedCpuSec = cpuSec / Environment.ProcessorCount;

            Console.WriteLine("{0,9:f1} seconds wall clock total", stopwatch.Elapsed.TotalSeconds);
            Console.WriteLine("{0,9:f1} CPU-seconds total", adjustedCpuSec);
            Console.WriteLine("{0,9:f1}% CPU utilization", 100 * adjustedCpuSec / stopwatch.Elapsed.TotalSeconds);

            Console.WriteLine();
            Console.WriteLine("{0,9:f1} frames coded", numFrames);
            Console.WriteLine("{0,9:f1} frames / second coded", numFrames / stopwatch.Elapsed.TotalSeconds);

            Console.WriteLine("{0,9:f1} CPU-milliseconds per frame used", adjustedCpuSec * 1000 / numFrames);
            Console.WriteLine("{0,9:f1} CPU-microseconds per frame used", adjustedCpuSec * 1000 * 1000 / numFrames);

            Console.WriteLine("{0,9:f1} KBytes read", bytesin / 1000);
            Console.WriteLine("{0,9:f1} KBytes written", bytesout / 1000);
            Console.WriteLine("{0,9:f1} KBytes / second read", bytesin / stopwatch.ElapsedMilliseconds / 1000);
            Console.WriteLine("{0,9:f1} KBytes / second written", bytesout / stopwatch.ElapsedMilliseconds / 1000);

            Console.WriteLine();


        }
    }
}
