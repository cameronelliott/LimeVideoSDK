// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using LimeVideoSDKQuickSync;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace _SystemCapabilities
{

    class Program
    {
        unsafe static void Main(string[] args)
        {
            ConfirmQuickSyncReadiness.HaltIfNotReady(true);

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
