/*
大量数据批量插入扩展。此扩展依赖具体驱动，如需使用，需要自行拷贝进项目
 */

using Chloe;
using Chloe.Dameng;
using Chloe.Data;
using Chloe.Infrastructure;
using Chloe.KingbaseES;
using Chloe.MySql;
using Chloe.Oracle;
using Chloe.PostgreSQL;
using Chloe.Reflection;
using Chloe.SqlServer;
using Dm;
using Kdbndp;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Common.Data
{
    public static class DbBulkInsertExtension
    {
        /// <summary>
        /// 批量插入
        /// </summary>
        public static async Task BulkInsertAsync<T>(this IDbContext source, List<T> entities)
        {
            switch (source)
            {
                case OracleContext oracleContext:
                    await BulkInsertAsync(oracleContext, entities);
                    break;

                case MySqlContext mySqlContext:
                    await BulkInsertAsync(mySqlContext, entities);
                    break;

                case DamengContext damengContext:
                    await BulkInsertAsync(damengContext, entities);
                    break;

                case KingbaseESContext kingbaseESContext:
                    await BulkInsertAsync(kingbaseESContext, entities);
                    break;

                case MsSqlContext mssqlContext:
                    await BulkInsertAsync(mssqlContext, entities);
                    break;

                case PostgreSQLContext postgreSQLContext:
                    await BulkInsertAsync(postgreSQLContext, entities);
                    break;

                default:
                    await source.InsertRangeAsync(entities);
                    break;
            }
        }

        /// <summary>
        /// oracle批量插入
        /// </summary>
        public static async Task BulkInsertAsync<T>(OracleContext source, List<T> entities, int? batchSize = null, int? bulkCopyTimeout = null)
        {
#if !NETCORE
            throw new NotSupportedException();
#else
            DataTable dt = null;
            var connection = source.Session.CurrentConnection;
            var isOpen = false;
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    isOpen = true;
                    connection.Open();
                }
                var persistedConnection = (connection as DbConnectionDecorator).PersistedConnection as OracleConnection;
                var versionString = (persistedConnection.ServerVersion ?? "").Split('.').FirstOrDefault();

                int version;
                if (!int.TryParse(versionString, out version))
                {
                    version = 11;
                }

                if (version < 11)
                {
                    await source.InsertRangeAsync(entities);
                    return;
                }
                dt = ToDataTable(entities);
                using var bulkCopy = new OracleBulkCopy(persistedConnection, OracleBulkCopyOptions.Default)
                {
                    DestinationTableName = dt.TableName
                };
                if (batchSize.HasValue) bulkCopy.BatchSize = batchSize.Value;
                if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
                foreach (DataColumn column in dt.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                bulkCopy.WriteToServer(dt);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                if ((ex.Message ?? "").Contains("-26083"))
                {
                    await source.InsertRangeAsync(entities);
                    return;
                }
                throw;
            }
            finally
            {
                if (isOpen) connection.Close();
                dt?.Clear();
            }
#endif
        }

        /// <summary>
        /// mysql批量插入
        /// </summary>
        public static async Task BulkInsertAsync<T>(MySqlContext source, List<T> entities, int? bulkCopyTimeout = null)
        {
            DataTable dt = null;
            var connection = source.Session.CurrentConnection;
            var isOpen = false;
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    isOpen = true;
                    connection.Open();
                }
                dt = ToDataTable(entities);
                var persistedConnection = connection as MySqlConnection;
                var bulkCopy = new MySqlBulkCopy(persistedConnection, source.Session.CurrentTransaction as MySqlTransaction)
                {
                    DestinationTableName = dt.TableName
                };
                if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
                for (int i = 0, l = dt.Columns.Count; i < l; i++)
                {
                    var column = dt.Columns[i];
                    bulkCopy.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(i, column.ColumnName));
                }
                await bulkCopy.WriteToServerAsync(dt);
            }
            finally
            {
                if (isOpen) connection.Close();
                dt?.Clear();
            }
        }

        /// <summary>
        /// 达梦批量插入
        /// </summary>
        public static async Task BulkInsertAsync<T>(DamengContext source, List<T> entities, int? batchSize = null, int? bulkCopyTimeout = null)
        {
            DataTable dt = null;
            var connection = source.Session.CurrentConnection as DmConnection;
            var isOpen = false;
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    isOpen = true;
                    connection.Open();
                }
                dt = ToDataTable(entities);
                var bulkCopy = new DmBulkCopy(connection, DmBulkCopyOptions.Default, source.Session.CurrentTransaction as DmTransaction)
                {
                    DestinationTableName = dt.TableName
                };
                if (batchSize.HasValue) bulkCopy.BatchSize = batchSize.Value;
                if (bulkCopyTimeout.HasValue) bulkCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
                foreach (DataColumn column in dt.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                bulkCopy.WriteToServer(dt);
                await Task.CompletedTask;
            }
            finally
            {
                if (isOpen) connection.Close();
                dt?.Clear();
            }
        }

        /// <summary>
        /// 人大金仓批量插入
        /// </summary>
        public static async Task BulkInsertAsync<T>(KingbaseESContext source, List<T> entities)
        {
            DataTable dt = null;
            var connection = source.Session.CurrentConnection as KdbndpConnection;
            var isOpen = false;
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    isOpen = true;
                    connection.Open();
                }
                dt = ToDataTable(entities);
                var sb = new StringBuilder().Append("COPY ").Append(dt.TableName).Append('(');
                for (int i = 0, l = dt.Columns.Count; i < l; i++)
                {
                    var column = dt.Columns[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(column.ColumnName);
                }
                sb.Append(") FROM STDIN BINARY");
                using var writer = connection.BeginBinaryImport(sb.ToString());
                foreach (DataRow item in dt.Rows)
                {
                    writer.WriteRow(item.ItemArray);
                }
                writer.Complete();
                sb.Clear();
                await Task.CompletedTask;
            }
            finally
            {
                if (isOpen) connection.Close();
                dt?.Clear();
            }
        }

        /// <summary>
        /// SqlServer批量插入
        /// </summary>
        public static async Task BulkInsertAsync<T>(MsSqlContext source, List<T> entities, int? batchSize = null, int? bulkCopyTimeout = null)
        {
            await source.BulkInsertAsync(entities, null, batchSize, bulkCopyTimeout);
        }

        /// <summary>
        /// pgsql批量插入
        /// </summary>
        public static async Task BulkInsertAsync<T>(PostgreSQLContext source, List<T> entities)
        {
            DataTable dt = null;
            var connection = source.Session.CurrentConnection as NpgsqlConnection;
            var isOpen = false;
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    isOpen = true;
                    connection.Open();
                }
                dt = ToDataTable(entities);
                var sb = new StringBuilder().Append("COPY ").Append(dt.TableName).Append('(');
                for (int i = 0, l = dt.Columns.Count; i < l; i++)
                {
                    var column = dt.Columns[i];
                    if (i > 0) sb.Append(", ");
                    sb.Append(column.ColumnName);
                }
                sb.Append(") FROM STDIN BINARY");
                using var writer = connection.BeginBinaryImport(sb.ToString());
                foreach (DataRow item in dt.Rows)
                {
                    writer.WriteRow(item.ItemArray);
                }
                writer.Complete();
                sb.Clear();
                await Task.CompletedTask;
            }
            finally
            {
                if (isOpen) connection.Close();
                dt?.Clear();
            }
        }

        /// <summary>
        /// 实体转datatable
        /// </summary>
        private static DataTable ToDataTable<T>(List<T> entities)
        {
            var typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(T));
            var descriptors = typeDescriptor.PrimitivePropertyDescriptors.Where(a => a.IsAutoIncrement == false).ToList();
            var dt = new DataTable()
            {
                TableName = typeDescriptor.Table.Name
            };
            descriptors.ForEach(c =>
            {
                dt.Columns.Add(c.Column.Name, c.Column.Type.GetUnderlyingType());
            });
            entities.ForEach(c =>
            {
                var row = new object[dt.Columns.Count];
                for (int i = 0, l = descriptors.Count; i < l; i++)
                {
                    row[i] = descriptors[i].GetValue(c);
                }
                dt.Rows.Add(row);
            });
            return dt;
        }
    }
}