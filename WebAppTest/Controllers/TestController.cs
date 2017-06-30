using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace WebAppTest.Controllers
{
    public class TestController : ApiController
    {
        // GET: Test
        public String Get()
        {
            return "test.";
        }
    }
}