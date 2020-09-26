using Chloe;
using Chloe.Infrastructure;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ChloeDemo
{
    /// <summary>
    /// 处理 PostgreSQL json、Oracle Clob/NClob
    /// </summary>
    class String_MappingType : DbParameterAssembler, IDbParameterAssembler, IDbValueConverter
    {
        /// <summary>
        /// Implementation of IDbValueConverter.Convert
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Convert(object value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Implementation of IDbParameterAssembler.SetupParameter
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="param"></param>
        public override void SetupParameter(IDbDataParameter parameter, DbParam param)
        {
            base.SetupParameter(parameter, param);

            if (parameter is NpgsqlParameter)
            {
                //For PostgreSQL json
                NpgsqlParameter pgsqlParameter = (NpgsqlParameter)parameter;

                if (param.DbType == DbTypeConsts.NpgJson)
                {
                    parameter.DbType = DbType.String;
                    pgsqlParameter.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Json;
                }
                else if (param.DbType == DbTypeConsts.NpgJsonb)
                {
                    parameter.DbType = DbType.String;
                    pgsqlParameter.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb;
                }
            }
            else if (parameter is OracleParameter)
            {
                //For Oracle Clob/NClob

                OracleParameter oracleParameter = (OracleParameter)parameter;

                /* 针对 oracle 长文本做处理 */
                string value = oracleParameter.Value as string;
                if (value != null && value.Length > 2000)
                {
                    if (param.DbType == DbType.String || param.DbType == DbType.StringFixedLength)
                        oracleParameter.OracleDbType = OracleDbType.NClob;
                    else if (param.DbType == DbType.AnsiString || param.DbType == DbType.AnsiStringFixedLength)
                        oracleParameter.OracleDbType = OracleDbType.Clob;
                }
            }
        }
    }
}
