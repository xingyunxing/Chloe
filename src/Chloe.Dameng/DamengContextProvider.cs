﻿using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.RDBMS;
using System.Data;
using System.Threading.Tasks;

namespace Chloe.Dameng
{
    //hongyl 调整增删改处理、加入无主键处理
    public partial class DamengContextProvider : DbContextProvider
    {
        readonly DatabaseProvider _databaseProvider;
        public DamengContextProvider(IDbConnectionFactory dbConnectionFactory)
        {
            PublicHelper.CheckNull(dbConnectionFactory);
            this._databaseProvider = new DatabaseProvider(dbConnectionFactory);
        }
        public DamengContextProvider(Func<IDbConnection> dbConnectionFactory) : this(new DbConnectionFactory(dbConnectionFactory))
        {
        }


        /// <summary>
        /// 设置方法解析器。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="handler"></param>
        public static void SetMethodHandler(string methodName, IMethodHandler handler)
        {
            PublicHelper.CheckNull(methodName, nameof(methodName));
            PublicHelper.CheckNull(handler, nameof(handler));
            lock (SqlGenerator.MethodHandlerDic)
            {
                SqlGenerator.MethodHandlerDic[methodName] = handler;
            }
        }

        public override IDatabaseProvider DatabaseProvider
        {
            get { return this._databaseProvider; }
        }

        protected override async Task InsertRange<TEntity>(List<TEntity> entities, string table, bool @async)
        {
            /*
             * 将 entities 分批插入数据库
             * 每批生成 insert into TableName(...) values(...),(...)... 
             * 该方法相对循环一条一条插入，速度提升 2/3 这样
             */

            PublicHelper.CheckNull(entities);
            if (entities.Count == 0)
                return;

            int maxParameters = 2100;
            int batchSize = 50; /* 每批实体大小，此值通过测试得出相对插入速度比较快的一个值 */

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(TEntity));

            List<PrimitivePropertyDescriptor> mappingPropertyDescriptors = typeDescriptor.PrimitivePropertyDescriptors.Where(a => a.IsAutoIncrement == false).ToList();
            int maxDbParamsCount = maxParameters - mappingPropertyDescriptors.Count; /* 控制一个 sql 的参数个数 */

            DbTable dbTable = PublicHelper.CreateDbTable(typeDescriptor, table);
            string sqlTemplate = AppendInsertRangeSqlTemplate(dbTable, mappingPropertyDescriptors);

            Func<Task> insertAction = async () =>
            {
                int batchCount = 0;
                List<DbParam> dbParams = new List<DbParam>();
                StringBuilder sqlBuilder = new StringBuilder();
                for (int i = 0; i < entities.Count; i++)
                {
                    var entity = entities[i];

                    if (batchCount > 0)
                        sqlBuilder.Append(",");

                    sqlBuilder.Append("(");
                    for (int j = 0; j < mappingPropertyDescriptors.Count; j++)
                    {
                        if (j > 0)
                            sqlBuilder.Append(",");

                        PrimitivePropertyDescriptor mappingPropertyDescriptor = mappingPropertyDescriptors[j];

                        if (mappingPropertyDescriptor.HasSequence())
                        {
                            string sequenceSchema = mappingPropertyDescriptor.Definition.SequenceSchema;
                            sequenceSchema = string.IsNullOrEmpty(sequenceSchema) ? dbTable.Schema : sequenceSchema;

                            sqlBuilder.Append("nextval('");

                            if (!string.IsNullOrEmpty(sequenceSchema))
                            {
                                sqlBuilder.Append(sequenceSchema).Append(".");
                            }

                            sqlBuilder.Append(mappingPropertyDescriptor.Definition.SequenceName);
                            sqlBuilder.Append("')");
                            continue;
                        }

                        object val = mappingPropertyDescriptor.GetValue(entity);

                        PublicHelper.NotNullCheck(mappingPropertyDescriptor, val);

                        if (val == null)
                        {
                            sqlBuilder.Append("NULL");
                            continue;
                        }

                        Type valType = val.GetType();
                        if (valType.IsEnum)
                        {
                            val = Convert.ChangeType(val, Enum.GetUnderlyingType(valType));
                            valType = val.GetType();
                        }

                        if (PublicHelper.IsToStringableNumericType(valType))
                        {
                            sqlBuilder.Append(val.ToString());
                            continue;
                        }

                        if (val is bool)
                        {
                            if ((bool)val == true)
                                sqlBuilder.AppendFormat("1");
                            else
                                sqlBuilder.AppendFormat("0");
                            continue;
                        }

                        string paramName = UtilConstants.ParameterNamePrefix + dbParams.Count.ToString();
                        DbParam dbParam = new DbParam(paramName, val) { DbType = mappingPropertyDescriptor.Column.DbType };
                        dbParams.Add(dbParam);
                        sqlBuilder.Append(paramName);
                    }
                    sqlBuilder.Append(")");

                    batchCount++;

                    if ((batchCount >= 20 && dbParams.Count >= 120/*参数个数太多也会影响速度*/) || dbParams.Count >= maxDbParamsCount || batchCount >= batchSize || (i + 1) == entities.Count)
                    {
                        sqlBuilder.Insert(0, sqlTemplate);
                        string sql = sqlBuilder.ToString();
                        await this.Session.ExecuteNonQuery(sql, dbParams.ToArray(), @async);

                        sqlBuilder.Clear();
                        dbParams.Clear();
                        batchCount = 0;
                    }
                }
            };

            Func<Task> fAction = insertAction;

            if (this.Session.IsInTransaction)
            {
                await fAction();
                return;
            }

            /* 因为分批插入，所以需要开启事务保证数据一致性 */
            this.Session.BeginTransaction();
            try
            {
                await fAction();
                this.Session.CommitTransaction();
            }
            catch
            {
                this.Session.RollbackTransaction();
                throw;
            }
        }

        static string AppendInsertRangeSqlTemplate(DbTable table, List<PrimitivePropertyDescriptor> mappingPropertyDescriptors)
        {
            StringBuilder sqlBuilder = new StringBuilder();

            sqlBuilder.Append("INSERT INTO ");
            sqlBuilder.Append(AppendTableName(table));
            sqlBuilder.Append("(");

            for (int i = 0; i < mappingPropertyDescriptors.Count; i++)
            {
                PrimitivePropertyDescriptor mappingPropertyDescriptor = mappingPropertyDescriptors[i];
                if (i > 0)
                    sqlBuilder.Append(",");
                sqlBuilder.Append(Utils.QuoteName(mappingPropertyDescriptor.Column.Name));
            }

            sqlBuilder.Append(") VALUES");

            string sqlTemplate = sqlBuilder.ToString();
            return sqlTemplate;
        }

        static string AppendTableName(DbTable table)
        {
            if (string.IsNullOrEmpty(table.Schema))
                return Utils.QuoteName(table.Name);

            return string.Format("{0}.{1}", Utils.QuoteName(table.Schema), Utils.QuoteName(table.Name));
        }
    }
}