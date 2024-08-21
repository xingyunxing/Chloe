using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;

namespace Chloe.MySql.MethodHandlers
{
    class Contains_Handler : Contains_HandlerBase
    {
        protected override void Method_String_Contains(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            exp.Object.Accept(generator);

            generator.SqlBuilder.Append(" LIKE ");
            generator.SqlBuilder.Append("CONCAT(");
            generator.SqlBuilder.Append("'%',");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append(",'%'");
            generator.SqlBuilder.Append(")");
        }

        /// <summary>
        /// in 子查询
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="sqlQuery"></param>
        /// <param name="operand"></param>
        protected override void In(SqlGeneratorBase generator, DbSqlQueryExpression sqlQuery, DbExpression operand)
        {
            if (sqlQuery.SkipCount != null || sqlQuery.TakeCount != null)
            {
                /*
                 * mysql 中不支持 `T`.`CityId` IN (SELECT `T0`.`Id` AS `C` FROM `City` AS `T0` LIMIT 0,10)，操蛋！
                 * 需要改成 `T`.`CityId` IN (SELECT * FROM (SELECT `T0`.`Id` AS `C` FROM `City` AS `T0` LIMIT 0,10) AS T_T)
                 */
                operand.Accept(generator);
                generator.SqlBuilder.Append(" IN (");
                generator.SqlBuilder.Append("SELECT * FROM (");
                sqlQuery.Accept(generator);
                generator.SqlBuilder.Append(") AS T_T");
                generator.SqlBuilder.Append(")");

                return;
            }

            operand.Accept(generator);
            generator.SqlBuilder.Append(" IN (");
            sqlQuery.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}
