using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiInterceptionHttpmodule.utils;
using log4net;

namespace ApiInterceptionHttpmodule.domain
{
    public class ApiRequestEvent : ICloneable
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ApiRequestEvent));
        private static readonly String localHostIp = CommonUtils.getLocalHostIp();
      
        protected ApiRequestEvent()
        {
        }

        public ApiRequestEvent(String url, String reqIp,String userHostName)
        {
            this.url = url;
            this.reqIp = reqIp;
            this.createDateTime = DateTime.Now;
            this.lastUpdateTime = DateTime.Now;
            this.userHostName = userHostName;
            this.reqCount = 1;
            this.setHostName();
            this.setRequestId();
        }

        public String reqId { get; private set; }
        public String hostName { get; private set; }
        public String url { get; private set; }
        public int reqCount { get; private set; }
        public DateTime createDateTime { get; private set; }
        public DateTime lastUpdateTime { get; private set; }
        public String reqIp { get; private set; }
        public String userHostName { get; private set; }

        public String serviceIp
        {
            get { return localHostIp; }
            private set { value = localHostIp; }
        }

        /// <summary>
        /// 刷新计数
        /// </summary>
        public void incrementCount()
        {
            setHostName();
            addCount();
            setRequestId();
            logger.DebugFormat("incrementCount done.{0}",this.ToString());
        }

        /// <summary>
        /// 重置计数器，
        /// 如果先前的计数和当前计数相等则重置为0，
        /// 否则当前计数减去先前的计数
        /// </summary>
        /// <param name="previousCount"></param>
        public void resetCount(int previousCount)
        {
            logger.DebugFormat("resetCount previousCount:{0},{1}", previousCount, this.ToString());
            if (this.reqCount == previousCount)
            {
                this.reqCount = 0;
            }
            else
            {
                int tmpCount = this.reqCount;
                Interlocked.Add(ref tmpCount, -previousCount);
                Interlocked.MemoryBarrier();
                this.reqCount = tmpCount;
            }
            this.lastUpdateTime = DateTime.Now;
        }

        public void setHostName()
        {
            hostName = ConfigurationManager.AppSettings["apiAppId"]??userHostName;
        }

        public void addCount()
        {
            int count = reqCount;
            Interlocked.Increment(ref count);
            Interlocked.MemoryBarrier();
            reqCount = count;
            this.lastUpdateTime = DateTime.Now;
        }

        /**
         * 是否在当前时间范围内，根据此值清理老的数据
         */
        public bool isInCurrentTimeRange()
        {
            String id = this.reqId.Split('-')[0];
            return CountUnit.isInCurrentTimeRange(id);
        }

        public void setRequestId()
        {
            String urlId = String.IsNullOrEmpty(url) ? null : CryptographyUtils.encryption(this.url);
            String id = String.Format("{0}-{1}-{2}"
                , CountUnit.getCountType()
                , this.reqIp?.Replace(".", "")
                , urlId);
            this.reqId = id;
            this.lastUpdateTime = DateTime.Now;
        }

       
        public override string ToString()
        {
            return "ApiRequestEvent{" +
                   "reqId='" + reqId + '\'' +
                   ", hostName='" + hostName + '\'' +
                   ", url='" + url + '\'' +
                   ", reqCount=" + reqCount +
                   ", createDateTime=" + createDateTime +
                   ", lastUpdateTime=" + lastUpdateTime +
                   ", reqIp='" + reqIp + '\'' +
                   ", serviceIp='" + serviceIp + '\'' +
                   ", clientHost='" + userHostName + '\'' +
                   '}';
        }

        public object Clone()
        {
            ApiRequestEvent @event = new ApiRequestEvent(this.url, this.reqIp,this.userHostName);
            @event.reqCount = this.reqCount;
            @event.serviceIp = this.serviceIp;
            @event.url = this.url;
            @event.hostName = this.hostName;
            @event.createDateTime = this.createDateTime;
            @event.lastUpdateTime = this.lastUpdateTime;

            return @event;
        }
    }
}
