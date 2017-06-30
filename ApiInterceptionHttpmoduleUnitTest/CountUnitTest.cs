using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiInterceptionHttpmodule.domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInterceptionHttpmoduleUnitTest
{
    [TestClass]
    public class CountUnitTest
    {
        [TestMethod]
        public void countType()
        {
            var countType = CountUnit.getCountType();
            Console.WriteLine(countType);

            var countType2 = CountUnit.getCountType();
            Console.WriteLine(countType2);
            Assert.AreEqual(countType, countType2);
            Thread.Sleep(3000);
        }
    }
}
