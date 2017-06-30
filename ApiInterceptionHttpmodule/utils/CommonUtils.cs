using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ApiInterceptionHttpmodule.utils
{
    public class CommonUtils
    {
        /// <summary>
        /// 获取调用服务的调用者IP
        /// </summary>
        /// <returns></returns>
        public static String getClallApiRequestIp()
        {
            if (HttpContext.Current == null
                || HttpContext.Current.Request == null)
            {
                return String.Empty;
            }
            return HttpContext.Current.Request.UserHostAddress;
        }

        /// <summary>
        /// clone new Dictionary
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> cloneDictionaryCloningValues<TKey, TValue>
            (IDictionary<TKey, TValue> original) where TValue : ICloneable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue) entry.Value.Clone());
            }
            return ret;
        }

        public static String getLocalHostIp()
        {          
            String ipString = Dns.GetHostAddresses(Dns.GetHostName())
                .First(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ToString();
            return ipString;
        }
    }
}
