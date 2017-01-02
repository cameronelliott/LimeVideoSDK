using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member


namespace LimeVideoSDK.QuickSync
{
    public static class UnitTest
    {
        public static readonly bool IsRunning =
       AppDomain.CurrentDomain.GetAssemblies().Any(
           a => a.FullName.ToLowerInvariant().StartsWith("Microsoft.VisualStudio.TestPlatform".ToLowerInvariant()));
    }
}
