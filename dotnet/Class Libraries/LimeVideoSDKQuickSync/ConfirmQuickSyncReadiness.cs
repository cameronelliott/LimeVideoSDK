// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using LimeVideoSDKQuickSync;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LimeVideoSDKQuickSync
{
    /// <summary>
    /// This class is a static class simply to provice a single static helper method
    /// </summary>
    static public class ConfirmQuickSyncReadiness
    {
        /// <summary>This method will determine if:
        /// 1. Hardware IGP/GPU support is ready with driver support
        /// 2. If the software fallback is available.
        /// </summary>
        /// <param name="verbose">if set to <c>true</c> [verbose].</param>
        unsafe public static void HaltIfNotReady(bool verbose = false)
        {
            string impltext;
            mfxStatus sts;
            var v = new mfxVersion();
            v.Major = 1;
            v.Minor = 0;

            var session = new mfxSession();

            mfxIMPL impl;

            bool readySoftware = false;
            bool readyHardware = false;



            //https://en.wikipedia.org/wiki/Graphics_processing_unit#Integrated_graphics

            sts = UnsafeNativeMethods.MFXInit(mfxIMPL.MFX_IMPL_HARDWARE, &v, &session);
            if (sts == mfxStatus.MFX_ERR_UNSUPPORTED)
            {
                if (verbose)
                {
                    Console.WriteLine("Your computer IS NOT READY for Quick Sync Hardware operations");
                    Console.WriteLine("  Please check your 1) Drivers 2) Motherboard 3) CPU");
                    Console.WriteLine("  Please see the LVSDK documentation on system preparation");
                }
            }
            else if (sts == mfxStatus.MFX_ERR_NONE)
            {

                sts = UnsafeNativeMethods.MFXQueryIMPL(session, &impl);
                QuickSyncStatic.ThrowOnBadStatus(sts, "MFXQueryIMPL");
                sts = UnsafeNativeMethods.MFXQueryVersion(session, &v);
                QuickSyncStatic.ThrowOnBadStatus(sts, "MFXQueryIMPL");
                sts = UnsafeNativeMethods.MFXClose(session);
                QuickSyncStatic.ThrowOnBadStatus(sts, "MFXClose");
                impltext = QuickSyncStatic.ImplementationString(impl);
                Console.WriteLine("Your computer IS READY for Quick Sync Hardware operations");
                //Console.WriteLine("Implementation = {0}", impltext);
                if (verbose)
                    Console.WriteLine("  Version major.minor =  {0}.{1}", v.Major, v.Minor);


                readyHardware = true;
            }
            else
                QuickSyncStatic.ThrowOnBadStatus(sts, "MFXInit");

            if (verbose)
                Console.WriteLine();


            sts = UnsafeNativeMethods.MFXInit(mfxIMPL.MFX_IMPL_SOFTWARE, &v, &session);
            if (sts == mfxStatus.MFX_ERR_UNSUPPORTED)
            {
                if (verbose)
                {
                    Console.WriteLine("Your software IS NOT READY for software Quick Sync operations");
                    Console.WriteLine("  You must replace the dummy libmfxsw64.dll with a genuine Intel copy");
                    Console.WriteLine("  Install either free:\"Media Server Studio\" or \"Intel Media SDK\" to fix this");
                    Console.WriteLine("  Please see the LVSDK documentation on system preparation");

                }
            }
            else if (sts == mfxStatus.MFX_ERR_NONE)
            {
                sts = UnsafeNativeMethods.MFXQueryIMPL(session, &impl);
                QuickSyncStatic.ThrowOnBadStatus(sts, "MFXQueryIMPL");
                sts = UnsafeNativeMethods.MFXQueryVersion(session, &v);
                QuickSyncStatic.ThrowOnBadStatus(sts, "MFXQueryIMPL");
                sts = UnsafeNativeMethods.MFXClose(session);
                QuickSyncStatic.ThrowOnBadStatus(sts, "MFXClose");
                impltext = QuickSyncStatic.ImplementationString(impl);
                Console.WriteLine("Your software IS READY for software Quick Sync operations");
                //Console.WriteLine("  Implementation = {0}", impltext);
                if (verbose)
                    Console.WriteLine("  Version major.minor =  {0}.{1}", v.Major, v.Minor);


                readySoftware = true;

            }
            else
                QuickSyncStatic.ThrowOnBadStatus(sts, "MFXInit");

            if (verbose)
                Console.WriteLine();

            if (!readyHardware && !readySoftware)
            {
                Console.WriteLine("You are not properly configured for either: A) GPU/IGP Quick Sync B) Software fallback");
                Console.WriteLine("You may not continue until you fix one option for Quick Sync coding");
                Console.WriteLine("Press a key to exit");
                Console.ReadKey();
                Environment.Exit(-1);
            }

            Console.WriteLine();
        }
    }
}
