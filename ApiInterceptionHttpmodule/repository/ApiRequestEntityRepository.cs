using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiInterceptionHttpmodule.domain;

namespace ApiInterceptionHttpmodule.repository
{
    public class ApiRequestEntityRepository
    {
        private static String getConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["HJApiMonitor_Write_DbConnString"].ConnectionString;
        }

        public static async Task<String> getServiceIp(String id, int millisecondsDelay)
        {
            String connString = getConnectionString();
            CancellationTokenSource tokenSource = new CancellationTokenSource(millisecondsDelay);
            CancellationToken token = tokenSource.Token;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand command = conn.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = SqlText.getServiceIp_sql;
                command.CommandTimeout = millisecondsDelay;
                command.Parameters.AddWithValue("@reqId", id);
                await conn.OpenAsync(token).ConfigureAwait(false);
                var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
                while (await reader.ReadAsync(token).ConfigureAwait(false))
                {
                    return reader["lastReqServiceIp"] as String;
                }
                if (!reader.IsClosed)
                {
                    reader.Close();
                }
            }
            return String.Empty;
        }

        public static async Task<int> update(int count
            , String id
            , String localHostIp
            , String userHostName
            , String reqIp
            , int millisecondsDelay)
        {
            String connString = getConnectionString();
            CancellationTokenSource tokenSource = new CancellationTokenSource(millisecondsDelay);
            CancellationToken token = tokenSource.Token;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand command = conn.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = SqlText.update_ApiRequestEntity_sql;
                command.CommandTimeout = millisecondsDelay;
                command.Parameters.AddWithValue("@reqCount", count);
                command.Parameters.AddWithValue("@reqId", id);
                command.Parameters.AddWithValue("@lastReqServiceIp", localHostIp?? DBNull.Value.ToString());               
                command.Parameters.AddWithValue("@clientHost", userHostName??DBNull.Value.ToString());
                command.Parameters.AddWithValue("@reqIp", reqIp ?? DBNull.Value.ToString());
                await conn.OpenAsync(token).ConfigureAwait(false);
                int rowsAffected = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                return rowsAffected;
            }
        }

        public static async Task<bool> checkExists(String id, int millisecondsDelay)
        {
            String connString = getConnectionString();
            CancellationTokenSource tokenSource = new CancellationTokenSource(millisecondsDelay);
            CancellationToken token = tokenSource.Token;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand command = conn.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = SqlText.exists_ApiRequestEntity_sql;
                command.CommandTimeout = millisecondsDelay;
                command.Parameters.AddWithValue("@reqId", id);
                await conn.OpenAsync(token).ConfigureAwait(false);
                object rowsAffected = await command.ExecuteScalarAsync(token)
                    .ConfigureAwait(false);
                return Convert.ToInt32(rowsAffected) > 0;
            }
        }

        public static async Task<int> add(ApiRequestEvent @event, int millisecondsDelay)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource(millisecondsDelay);
            CancellationToken token = tokenSource.Token;
            String connString = getConnectionString();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand command = conn.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = SqlText.add_ApiRequestEntity_sql;
                command.CommandTimeout = millisecondsDelay;

                command.Parameters.AddRange(getAddParameters(@event));

                await conn.OpenAsync(token).ConfigureAwait(false);
                int rowsAffected = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                return rowsAffected;
            }
        }

        [Obsolete]
        public static void updateBatchData<T>(List<T> list, int millisecondsDelay)
        {
            DataTable dt = convertToDataTable(list, "soa_api_callmonitor");
            String connString = getConnectionString();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand command = null;
                try
                {
                    conn.Open();
                    command = conn.CreateCommand();
                    //Creating temp table on database
                    command.CommandText = @"create table #TmpTable(reqId varchar(200) Primary key,
                    hostName varchar(200),
                    url varchar(200),
                    reqCount int,
                    createTime datetime,
                    lastUpdateTime datetime,
                    reqIp varchar(30),
                    lastReqServiceIp varchar(50));select 1;";
                    command.CommandType = CommandType.Text;
                    object result = command.ExecuteScalar();
                    Console.WriteLine("创建表：" + result);
                    //Bulk insert into temp table
                    using (SqlBulkCopy bulkcopy = new SqlBulkCopy(conn))
                    {
                        bulkcopy.BatchSize = 100;
                        bulkcopy.BulkCopyTimeout = 660;
                        bulkcopy.DestinationTableName = "#TmpTable";
                        bulkcopy.WriteToServer(dt);
                        bulkcopy.SqlRowsCopied += Bulkcopy_SqlRowsCopied;
                        bulkcopy.Close();
                    }

                    // Updating destination table, and dropping temp table
                    command.CommandTimeout = millisecondsDelay;
                    command.CommandText = @"UPDATE soa_api_callmonitor SET reqCount+=1,lastUpdateTime=getdate() 
                    FROM soa_api_callmonitor as tb_callmonitor INNER JOIN #TmpTable as Temp ON Temp.reqId=tb_callmonitor.reqId";
                    int r = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    command.CommandText = "drop table #TmpTable;select 1;";
                    command.ExecuteScalar();
                    conn.Close();
                }
            }
        }

        private static void Bulkcopy_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
        }

        public static DataTable convertToDataTable<T>(IList<T> data, string tableName)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable(tableName);
            foreach (PropertyDescriptor prop in properties)
            {
                table.Columns.Add(prop.Name
                    , Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        private static SqlParameter[] getAddParameters(ApiRequestEvent @event)
        {
            List<SqlParameter> list = new List<SqlParameter>();
            SqlParameter parameter = new SqlParameter("@hostName", SqlDbType.VarChar);
            parameter.Value = @event.hostName;
            list.Add(parameter);

            parameter = new SqlParameter("@url", SqlDbType.VarChar);
            parameter.Value = @event.url;
            list.Add(parameter);

            parameter = new SqlParameter("@reqId", SqlDbType.VarChar);
            parameter.Value = @event.reqId;
            list.Add(parameter);

            parameter = new SqlParameter("@reqCount", SqlDbType.Int);
            parameter.Value = @event.reqCount;
            list.Add(parameter);

            parameter = new SqlParameter("@reqIp", SqlDbType.VarChar);
            parameter.Value = @event.reqIp;
            list.Add(parameter);

            parameter = new SqlParameter("@lastReqServiceIp", SqlDbType.VarChar);
            parameter.Value = @event.serviceIp;
            list.Add(parameter);

            parameter = new SqlParameter("@clientHost", SqlDbType.VarChar);
            parameter.Value = @event.userHostName;
            list.Add(parameter);

            list.ForEach(l =>
            {
                if (l.Value == null)
                {
                    l.Value = DBNull.Value;
                }
            });
            return list.ToArray();
        }
    }
}
