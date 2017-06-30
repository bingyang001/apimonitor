using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiInterceptionHttpmodule.utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInterceptionHttpmoduleUnitTest
{
    [TestClass]
    public class CommonUtilsTest
    {
        [TestMethod]
        public void getLocalHostIp()
        {
            var  ip= CommonUtils.getLocalHostIp();
            Console.WriteLine(ip);
        }
        [TestMethod]
        public void add()
        {
            int tmpCount = 1;
            Interlocked.Add(ref tmpCount, -3);
            Assert.AreEqual(-2,tmpCount);
        }
    }
}
