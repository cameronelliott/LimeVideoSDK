using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using LimeVideoSDKQuickSync;

namespace UnitTests
{
    [TestClass]
    public class UnitTests
    {

        static UnitTests()
        {
            Trace.Listeners.Add(new System.Diagnostics.DefaultTraceListener());
        }


        [DataTestMethod]
        [DataRow("a", "b")]
        [DataRow(" ", "a")]
        [TestCategory("Quick"), TestMethod()]
        public void TestMathos1(string value1, string value2)
        {
            Assert.AreEqual(value1 + value2, string.Concat(value1, value2));
        }


        [TestCategory("Sample Programs - Slow"), TestMethod()]
        public void Decoder1ProgramTest()
        {
            //var o = "BigBuckBunny_320x180.NV12.yuv";
            //File.Delete(o);
            //Decoder1.Program.Main();
            //Console.WriteLine(Environment.CurrentDirectory);
            //Assert.IsTrue(File.Exists(o));

            var a = new NV12Resizer(320, 180, 8, 8);

           //a.Convert(new byte[320 * 180 * 3 / 2]);
           // QualityMeasure.NV12FilesSame("")
        }
    }
}
