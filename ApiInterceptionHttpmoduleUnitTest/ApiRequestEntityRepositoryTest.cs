using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ApiInterceptionHttpmodule.domain;
using ApiInterceptionHttpmodule.repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInterceptionHttpmoduleUnitTest
{
    [TestClass]
    public class ApiRequestEntityRepositoryTest
    {
        [TestMethod]
        public async Task add()
        {
            var result = await ApiRequestEntityRepository.add(new ApiRequestEvent("/a/b/c/d", "127.0.0.1",null), 1000);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public async Task addDuplicateEntity()
        {
            try
            {
                var result = await ApiRequestEntityRepository.add(new ApiRequestEvent("/a/b/c/d", "127.0.0.1",null), 1000);
            }
            catch (SqlException e)
            {
                Assert.AreEqual(2627, e.Number);
            }           
        }

        [TestMethod]
        public async Task upate()
        {
            var entity = new ApiRequestEvent("/a/b/c/d", "127.0.0.1", null);
            var result = await ApiRequestEntityRepository.update(
                entity.reqCount
                , entity.reqId
                ,"127.1.236.1"
                ,"testHost"
                ,"0.0.0.1"
                , 1000);
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public async Task exists()
        {
            var entity = new ApiRequestEvent("/a/b/c/d", "127.0.0.1", null);
            var result = await ApiRequestEntityRepository.checkExists(entity.reqId, 1000);
            Assert.AreEqual(true, result);

            entity = new ApiRequestEvent("/a/b/c/f", "127.0.0.1", null);
            result = await ApiRequestEntityRepository.checkExists(entity.reqId, 1000);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void batchUpdate()
        {
            var list = new List<ApiRequestEvent>();
            list.Add(new ApiRequestEvent("/a/b/c/d", "127.0.0.1", null));
            list.Add(new ApiRequestEvent("/a/b/0", "127.0.0.1", null));
            list.Add(new ApiRequestEvent("/a/b/100", "127.0.0.1", null));
            list.Add(new ApiRequestEvent("/a/b/71", "127.0.0.1", null));
            list.Add(new ApiRequestEvent("/a/b/7", "127.0.0.1", null));
            list.Add(new ApiRequestEvent("/a/b/37", "127.0.0.1", null));
            ApiRequestEntityRepository.updateBatchData(list,3000);
        }
    }
}
