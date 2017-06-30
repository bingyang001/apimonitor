using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ApiInterceptionHttpmodule.domain;
using log4net.Config;
using Microsoft.ApplicationInsights.Web;

namespace WebAppTest
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            XmlConfigurator.Configure(new System.IO.
                FileInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            ApiReqContainer.stopTask();
        }
    }
}
