// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LimeVideoSDKQuickSync
{

    /// <summary>
    /// Helper class to print progress bars to the Console
    /// </summary>
    public class ProgressReporter
    {
        Stopwatch sw = new Stopwatch();
        int s = 0;
        bool finished = false;
        /// <summary>
        /// Constructor
        /// </summary>
        public ProgressReporter()
        {
            sw.Start();
        }
        /// <summary>
        /// If a second has passed since last call, print a progress report.
        /// </summary>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        public void Report(long numerator, long denominator)
        {
            if (s != (int)sw.Elapsed.TotalSeconds || numerator == denominator)
            {
                Console.Write("Progress {0}%  \r", numerator * 100 / denominator);
                s = (int)sw.Elapsed.TotalSeconds;
            }

        }
        /// <summary>
        /// Print the final progress report.
        /// </summary>
        public void Done()
        {
            Report(1, 1);
            if (!finished)
            {
                Console.WriteLine();
                finished = true;
            }
        }
    }
}
