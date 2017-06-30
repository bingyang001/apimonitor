using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiInterceptionHttpmodule.repository
{
    public class SqlText
    {
        public static readonly String getServiceIp_sql = "select [lastReqServiceIp] from [dbo].[soa_api_callmonitor] with(nolock) where reqId=@reqId";
        public static readonly String update_ApiRequestEntity_sql =
            "update [soa_api_callmonitor] set [reqCount]+=@reqCount,[lastUpdateTime]=getdate(),lastReqServiceIp=@lastReqServiceIp,clientHost=@clientHost,reqIp=@reqIp where reqId=@reqId";

        public static readonly String exists_ApiRequestEntity_sql =
            @"if exists (select top 1 * from [soa_api_callmonitor] with(nolock) where reqId=@reqId)
            select 1
            else
            select 0
            ";

        public static readonly String add_ApiRequestEntity_sql =
            @"INSERT INTO [dbo].[soa_api_callmonitor]
             ([reqId]
            ,[hostName]
            ,[url]
            ,[reqCount]
            ,[createTime]
            ,[lastUpdateTime]
            ,[reqIp]
            ,[lastReqServiceIp]
            ,clientHost)
            VALUES(
            @reqId
            ,@hostName
            ,@url
            ,@reqCount
            ,getdate()
            ,getdate()
            ,@reqIp
            ,@lastReqServiceIp
            ,@clientHost)";
    }
}
