using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ApiInterceptionHttpmodule.domain;
using ApiInterceptionHttpmodule.utils;
using log4net;

namespace ApiInterceptionHttpmodule
{
    public class ApiHttpmodule : IHttpModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ApiHttpmodule));

        public void Init(HttpApplication context)
        {
            context.BeginRequest += Context_BeginRequest;
            context.EndRequest += Context_EndRequest;            
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                return;
            }
            try
            {               
                ApiReqContainer.add(context.Request.FilePath
                    , CommonUtils.getClallApiRequestIp()
                    , context.Request.UserHostName);
            }
            catch (Exception ex)
            {
                logger.Error("ApiInterceptionHttpmodule Exception", ex);
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
