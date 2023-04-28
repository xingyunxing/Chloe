using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using Chloe.RDBMS;
using Chloe.Reflection;

namespace Chloe.PostgreSQL
{
    partial class SqlGenerator : SqlGeneratorBase
    {
        static string GenParameterName(int ordinal)
        {
            if (ordinal < CacheParameterNames.Count)
            {
                return CacheParameterNames[ordinal];
            }

            return UtilConstants.ParameterNamePrefix + ordinal.ToString();
        }

        static bool TryGetCastTargetDbTypeString(Type sourceType, Type targetType, out string dbTypeString, bool throwNotSupportedException = true)
        {
            dbTypeString = null;

            sourceType = ReflectionExtension.GetUnderlyingType(sourceType);
            targetType = ReflectionExtension.GetUnderlyingType(targetType);

            if (sourceType == targetType)
                return false;

            if (CastTypeMap.TryGetValue(targetType, out dbTypeString))
            {
                return true;
            }

            if (throwNotSupportedException)
                throw new NotSupportedException(PublicHelper.AppendNotSupportedCastErrorMsg(sourceType, targetType));
            else
                return false;
        }

        public static void DbFunction_DATEADD(SqlGeneratorBase generator, string interval, DbMethodCallExpression exp)
        {
            generator.SqlBuilder.Append("(");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" + ");
            generator.SqlBuilder.Append("make_interval");
            generator.SqlBuilder.Append("(");
            generator.SqlBuilder.Append(interval);
            generator.SqlBuilder.Append(":=");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(")");
            generator.SqlBuilder.Append(")");
        }
        public static void DbFunction_DATEPART(SqlGeneratorBase generator, string interval, DbExpression exp)
        {
            generator.SqlBuilder.Append("CAST(");
            generator.SqlBuilder.Append("DATE_PART(");
            generator.SqlBuilder.Append("'", interval, "'");
            generator.SqlBuilder.Append(",");
            exp.Accept(generator);
            generator.SqlBuilder.Append(")");
            generator.SqlBuilder.Append(" as integer)");
        }
        public static void DbFunction_DATEDIFF(SqlGeneratorBase generator, string interval, DbExpression startDateTimeExp, DbExpression endDateTimeExp)
        {
            generator.SqlBuilder.Append("DATEDIFF(");
            generator.SqlBuilder.Append(interval);
            generator.SqlBuilder.Append(",");
            startDateTimeExp.Accept(generator);
            generator.SqlBuilder.Append(",");
            endDateTimeExp.Accept(generator);
            generator.SqlBuilder.Append(")");
        }

        #region AggregateFunction
        public static void Aggregate_Count(SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("COUNT(1)");
        }
        public static void Aggregate_Count(SqlGeneratorBase generator, DbExpression arg)
        {
            generator.SqlBuilder.Append("COUNT(");
            arg.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
        public static void Aggregate_LongCount(SqlGeneratorBase generator)
        {
            Aggregate_Count(generator);
        }
        public static void Aggregate_LongCount(SqlGeneratorBase generator, DbExpression arg)
        {
            Aggregate_Count(generator, arg);
        }
        public static void Aggregate_Max(SqlGeneratorBase generator, DbExpression exp, Type retType)
        {
            AppendAggregateFunction(generator, exp, retType, "MAX", false);
        }
        public static void Aggregate_Min(SqlGeneratorBase generator, DbExpression exp, Type retType)
        {
            AppendAggregateFunction(generator, exp, retType, "MIN", false);
        }
        public static void Aggregate_Sum(SqlGeneratorBase generator, DbExpression exp, Type retType)
        {
            if (retType.IsNullable())
            {
                AppendAggregateFunction(generator, exp, retType, "SUM", true);
            }
            else
            {
                generator.SqlBuilder.Append("COALESCE(");
                AppendAggregateFunction(generator, exp, retType, "SUM", false);
                generator.SqlBuilder.Append(",");
                generator.SqlBuilder.Append("0");
                generator.SqlBuilder.Append(")");
            }
        }
        public static void Aggregate_Average(SqlGeneratorBase generator, DbExpression exp, Type retType)
        {
            AppendAggregateFunction(generator, exp, retType, "AVG", false);
        }

        static void AppendAggregateFunction(SqlGeneratorBase generator, DbExpression exp, Type retType, string functionName, bool withCast)
        {
            string dbTypeString = null;
            if (withCast == true)
            {
                Type underlyingType = ReflectionExtension.GetUnderlyingType(retType);
                if (CastTypeMap.TryGetValue(underlyingType, out dbTypeString))
                {
                    generator.SqlBuilder.Append("CAST(");
                }
            }

            generator.SqlBuilder.Append(functionName, "(");
            exp.Accept(generator);
            generator.SqlBuilder.Append(")");

            if (dbTypeString != null)
            {
                generator.SqlBuilder.Append(" AS ", dbTypeString, ")");
            }
        }
        #endregion

    }
}
