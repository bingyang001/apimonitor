using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiInterceptionHttpmodule.domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInterceptionHttpmoduleUnitTest
{
    [TestClass]
    public class ApiRequestEntityTest
    {
        [TestMethod]
        public void instanceApiRequestEntity()
        {
            var entity = new ApiRequestEvent("/a/b", "0.0.0.0",null);
            Console.WriteLine(entity.ToString());
        }

        [TestMethod]
        public void countType()
        {
            var entity = new ApiRequestEvent("/a/b", "0.0.0.0",null);
            Console.WriteLine(entity.ToString());
            bool inCurrentTimeRange = entity.isInCurrentTimeRange();
            Assert.AreEqual(true, inCurrentTimeRange);


            entity = new ApiRequestEvent("/a/b", "0.0.0.0", null);
            Console.WriteLine(entity.ToString());
            inCurrentTimeRange = entity.isInCurrentTimeRange();
            Assert.AreEqual(true, inCurrentTimeRange);
        }
    }
}
