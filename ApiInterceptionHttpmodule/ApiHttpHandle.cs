using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ApiInterceptionHttpmodule.domain;

namespace ApiInterceptionHttpmodule
{
   public class ApiHttpHandle:IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {         
            context.Response.Write(ApiReqContainer.getStats());
            context.Response.ContentType = "text/html";
            context.Response.End();
        }

        public bool IsReusable { get; }
    }
}
