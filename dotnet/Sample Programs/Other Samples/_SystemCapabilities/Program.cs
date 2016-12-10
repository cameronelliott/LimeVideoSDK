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
            string impltext;
            mfxStatus sts;
            var v = new mfxVersion();
            v.Major = 1;
            v.Minor = 3;

            var session = new mfxSession();

            mfxIMPL impl;




            sts = UnsafeNativeMethods.MFXInit(mfxIMPL.MFX_IMPL_AUTO, &v, &session);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXInit");



            sts = UnsafeNativeMethods.MFXQueryIMPL(session, &impl);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXQueryIMPL");
            sts = UnsafeNativeMethods.MFXQueryVersion(session, &v);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXQueryIMPL");
            sts = UnsafeNativeMethods.MFXClose(session);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXClose");
            impltext = QuickSyncStatic.ImplementationString(impl);
            Console.WriteLine("Implementation = {0}", impltext);
            Console.WriteLine("Version major.minor =  {0}.{1}", v.Major, v.Minor);


            sts = UnsafeNativeMethods.MFXInit(mfxIMPL.MFX_IMPL_SOFTWARE, &v, &session);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXInit");


            sts = UnsafeNativeMethods.MFXQueryIMPL(session, &impl);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXQueryIMPL");
            sts = UnsafeNativeMethods.MFXQueryVersion(session, &v);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXQueryIMPL");
            sts = UnsafeNativeMethods.MFXClose(session);
            QuickSyncStatic.ThrowOnBadStatus(sts, "MFXClose");
            impltext = QuickSyncStatic.ImplementationString(impl);
            Console.WriteLine("Implementation = {0}", impltext);
            Console.WriteLine("Version major.minor =  {0}.{1}", v.Major, v.Minor);


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
