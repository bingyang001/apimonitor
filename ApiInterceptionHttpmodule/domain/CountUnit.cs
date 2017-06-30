using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ApiInterceptionHttpmodule.domain
{
    /// <summary>
    /// 计数单位
    /// </summary>
    public class CountUnit
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CountUnit));
        //计数结束时间
        private static DateTime countEndTime = DateTime.Now;

        public static bool isInCurrentTimeRange(String countDateTime)
        {
            return getCountType().Equals(countDateTime);
        }

        private static String getCountType(DateTime dateTime, String dateType)
        {
            switch (dateType)
            {
                case "Day":
                    return dateTime.ToString("yyyyMMdd");
                case "Hour":
                    return dateTime.ToString("yyyyMMddHH");
                case "Minute":
                    return dateTime.ToString("yyyyMMddHHmm");
                default:
                    return dateTime.ToString("yyyyMMdd");
            }
        }

        public static String getCountType()
        {
            String dateType = ConfigurationManager.AppSettings["apiUrlCountDateType"];
            if (DateTime.Now <= countEndTime)
            {
                return getCountType(countEndTime, dateType);                              
            }
            String dateRange = ConfigurationManager.AppSettings["apiUrlCountDateRange"];
            String countType;
            switch (dateType)
            {
                case "Day":
                    if (String.IsNullOrEmpty(dateRange))
                    {
                        countEndTime = DateTime.Now;
                        countType= DateTime.Now.ToString("yyyyMMdd");
                    }
                    else
                    {
                        countEndTime = DateTime.Now.AddDays(Convert.ToInt32(dateRange));
                        countType = countEndTime.ToString("yyyyMMdd");
                    }
                    break;
                case "Hour":
                    if (String.IsNullOrEmpty(dateRange))
                    {
                        countEndTime = DateTime.Now;
                        countType= DateTime.Now.ToString("yyyyMMddHH");
                    }
                    else
                    {
                        countEndTime = DateTime.Now.AddHours(Convert.ToInt32(dateRange));
                        countType= countEndTime.ToString("yyyyMMddHH");
                    }
                    break;
                case "Minute":
                    if (String.IsNullOrEmpty(dateRange))
                    {
                        countEndTime = DateTime.Now;
                        countType= DateTime.Now.ToString("yyyyMMddHHmm");
                    }
                    else
                    {
                        countEndTime = DateTime.Now.AddMinutes(Convert.ToInt32(dateRange));
                        countType= countEndTime.ToString("yyyyMMddHHmm");
                    }
                    break;                    
                default:
                    countEndTime = DateTime.Now;
                    countType = DateTime.Now.ToString("yyyyMMdd");
                    break;
            }
            logger.InfoFormat("new count date time,countType:{0},countEndTime:{1}"
                , countType
                , countEndTime);
            return countType;
        }
    }
}
