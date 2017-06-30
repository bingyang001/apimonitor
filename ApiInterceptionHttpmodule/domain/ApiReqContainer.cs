using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using ApiInterceptionHttpmodule.repository;
using ApiInterceptionHttpmodule.utils;
using log4net;

namespace ApiInterceptionHttpmodule.domain
{
    /// <summary>
    /// API 调用容器
    /// </summary>
    public class ApiReqContainer
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ApiReqContainer));

        private static readonly int taskCycleMillisecond =
            Convert.ToInt32(ConfigurationManager.AppSettings["taskCycleMillisecond"] ?? "60000");

        private static readonly int dbTimeOut =
            Convert.ToInt32(ConfigurationManager.AppSettings["apiMonitor_TimeOutMillisecond"] ?? "2000");

        //private static Timer timer;
        private static Thread thread;

        private static volatile bool isRuning;

        private static readonly ConcurrentDictionary<String, ApiRequestEvent>
            dictionary = new ConcurrentDictionary<string, ApiRequestEvent>();

        static ApiReqContainer()
        {
            try
            {
                Stopwatch wStopwatch=Stopwatch.StartNew();
                startTask();
                wStopwatch.Stop();
                logger.InfoFormat("ApiReqContainer startTask success.wait:{0} ms", wStopwatch.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                logger.Error("ApiReqContainer startTask e",e);
            }
        }

        public static void add(String url, String ip,String userHostName)
        {
            if (UrlFilter.isExclude(url))
            {
                return;
            }
            //log
            logger.DebugFormat("add to dictionary begin,url:{0},ip:{1}"
                , url, ip);
            //执行时间统计
            Stopwatch stopwatch = Stopwatch.StartNew();
            //
            ApiRequestEvent apiRequestEvent = new ApiRequestEvent(url, ip, userHostName);
            //放入计数容器
            dictionary.AddOrUpdate(apiRequestEvent.reqId, v => apiRequestEvent, (v, e) =>
            {
                //刷新当前计数状态
                e.incrementCount();
                logger.InfoFormat("ApiRequestEvent [AddOrUpdate] dic ok.{0}"
                    , e.ToString());
                return e;
            });
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 30)
            {
                logger.InfoFormat("add to dictionary,wait:{0} ms,url:{1},ip:{2}"
                    , stopwatch.ElapsedMilliseconds
                    , url, ip);
            }
            //log
            logger.DebugFormat("add to dictionary done,id:{0},url:{1},ip:{2},wait:{3} ms"
                , apiRequestEvent.reqId, url, ip, stopwatch.ElapsedMilliseconds);
        }

        public static String getStats()
        {
            int dicCount = dictionary.Count;
            IEnumerable<ApiRequestEvent> events = dictionary.Values.OrderByDescending(k => k.reqCount).Take(50);
            StringBuilder builder = new StringBuilder("dictionary size:" + dicCount + "</br>");
            foreach (var e in events)
            {
                builder.AppendFormat("id:{0},url:{1},count:{2},reqIp:{3}</br>"
                    , e.reqId, e.url, e.reqCount, e.reqIp);
            }
            return builder.ToString();
        }

        public static void startTask()
        {
            if (isRuning)
            {
                return;
            }
            isRuning = true;
            thread = new Thread(async () =>
            {
                while (isRuning)
                {
                    try
                    {
                        await runTask();
                    }
                    catch (Exception e)
                    {
                        logger.Error("run task exception,", e);
                    }
                    finally
                    {
                        Thread.Sleep(taskCycleMillisecond);
                    }
                }
            });
            thread.IsBackground = true;
            thread.Name = "ApiReqContainer";
            thread.Start();
        }

//        public static void startTask()
//        {
//            isRuning = true;
//            timer = new Timer(t =>
//            {
//                if (isRuning)
//                {
//                    try
//                    {
//                        runTask();
//                    }
//                    catch (Exception e)
//                    {
//                        logger.Error("ApiInterceptionHttpmodule run task exception,", e);
//                    }
//                }
//                timer.Change(3000, Timeout.Infinite);
//            }, null, Timeout.Infinite, Timeout.Infinite);
//            timer.Change(3000, Timeout.Infinite);
//            logger.Info("ApiInterceptionHttpmodule startTask done.");
//        }

        public static void stopTask()
        {
            try
            {
                Stopwatch wath = Stopwatch.StartNew();
                isRuning = false;
                //timer.Dispose();
                thread.Join(2000);
                wath.Stop();
                logger.InfoFormat("stopTask done.wait {0} ms", wath.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                logger.Error("ApiReqContainer stop exception.",e);
            }
        }

        private static async Task runTask()
        {
            if (dictionary.Count == 0)
            {
                return;
            }
            var stopWatch = Stopwatch.StartNew();
            //clone new dic
            var dic = CommonUtils.cloneDictionaryCloningValues(dictionary);
            var expiredUrls = new List<String>(dic.Count);
            foreach (var apiRequestEntity in dic)
            {
                if (await ApiRequestEntityRepository.checkExists(apiRequestEntity.Value.reqId, getDbTimeOut()))
                {
                    try
                    {
                        //执行更新
                        int result = await ApiRequestEntityRepository
                            .update(apiRequestEntity.Value.reqCount
                                , apiRequestEntity.Value.reqId
                                , apiRequestEntity.Value.serviceIp
                                , apiRequestEntity.Value.userHostName
                                , apiRequestEntity.Value.reqIp
                                , getDbTimeOut())
                            .ConfigureAwait(false);
                        int count = apiRequestEntity.Value.reqCount;
                        //重置已有的计数器
                        dictionary[apiRequestEntity.Key].resetCount(apiRequestEntity.Value.reqCount);
                        logger.InfoFormat(
                            "ApiRequestEntityRepository [update],id:{0},url:{1},count:{2},dbResult:{3},resetCount success."
                            , apiRequestEntity.Value.reqId
                            , apiRequestEntity.Value.url
                            , count
                            , result);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("ApiRequestEntityRepository.add ex", ex);
                    }
                }
                else
                {
                    if (apiRequestEntity.Value.reqCount > 0)
                    {
                        try
                        {
                            int result = await ApiRequestEntityRepository
                                .add(apiRequestEntity.Value, getDbTimeOut())
                                .ConfigureAwait(false);
                            logger.InfoFormat("ApiRequestEntityRepository add,info: {0},dbResult:{1}"
                                , result
                                , apiRequestEntity.Value.ToString());
                            //重置计数
                            dictionary[apiRequestEntity.Key].resetCount(apiRequestEntity.Value.reqCount);
                        }
                        catch (SqlException ex)
                        {
                            logger.Error("ApiRequestEntityRepository.add ex", ex);
                            //重复添加
                            if (ex.Number == 2627)
                            {
                                String ip = await ApiRequestEntityRepository
                                    .getServiceIp(apiRequestEntity.Value.reqId, getDbTimeOut())
                                    .ConfigureAwait(false);
                                if (!ip.Equals(apiRequestEntity.Value.serviceIp))
                                {
                                    //执行更新
                                    int result = await ApiRequestEntityRepository
                                        .update(apiRequestEntity.Value.reqCount
                                            , apiRequestEntity.Value.reqId
                                            , apiRequestEntity.Value.serviceIp
                                            , apiRequestEntity.Value.userHostName
                                            , apiRequestEntity.Value.reqIp
                                            , getDbTimeOut())
                                        .ConfigureAwait(false);
                                    //重置计数
                                    dictionary[apiRequestEntity.Key]
                                        .resetCount(apiRequestEntity.Value.reqCount);
                                    logger.InfoFormat(
                                        "ApiRequestEntityRepository [add duplicate 2627],update {0},ApiRequestEvent String:{1}"
                                        , result
                                        , apiRequestEntity.Value.ToString());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("ApiRequestEntityRepository.add ex", ex);
                        }
                    }
                }
                //统计过期的URL计数器
                if (!apiRequestEntity.Value.isInCurrentTimeRange())
                {
                    expiredUrls.Add(apiRequestEntity.Value.reqId);
                }
            }
            if (expiredUrls.Count > 0)
            {
                //执行清理过期的URL计数
                expiredUrls.ForEach(u =>
                {
                    ApiRequestEvent @event;
                    dictionary.TryRemove(u, out @event);
                });
                logger.InfoFormat("expiredUrls {0} removed.", String.Join(",", expiredUrls.ToArray()));
            }
            stopWatch.Stop();
            logger.InfoFormat("ApiReqContainer [run task done].wait {0} ms"
                , stopWatch.ElapsedMilliseconds);
        }

        private static int getDbTimeOut()
        {
            return dbTimeOut;
        }
    }
}