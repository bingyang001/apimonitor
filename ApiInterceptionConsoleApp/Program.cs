using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiInterceptionHttpmodule.domain;
using log4net.Config;
using log4net.Core;

namespace ApiInterceptionConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new System.IO.
                FileInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
            ApiReqContainer.startTask();
            Thread trhThread=new Thread(() =>
            {
                int index = 0;
                while (true)
                {
                    var r=new System.Random().Next(1,1000);
                    var i=(index++)% r;
                    ApiReqContainer.add("/a/b/"+i,"0.0.0."+ i,"hosttest");
                    Thread.Sleep(1000);
                }
            });
            trhThread.Start();
            Console.Read();
        }
    }
}
