using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using Chloe.RDBMS;
using Chloe.Reflection;

namespace Chloe.SqlServer.Odbc
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
        static string GenRowNumberName(List<DbColumnSegment> columns)
        {
            int ROW_NUMBER_INDEX = 1;
            string row_numberName = "ROW_NUMBER_0";
            while (columns.Any(a => string.Equals(a.Alias, row_numberName, StringComparison.OrdinalIgnoreCase)))
            {
                row_numberName = "ROW_NUMBER_" + ROW_NUMBER_INDEX.ToString();
                ROW_NUMBER_INDEX++;
            }

            return row_numberName;
        }

        static DbExpression EnsureDbExpressionReturnCSharpBoolean(DbExpression exp)
        {
            return DbValueExpressionTransformer.Transform(exp);
        }

        static bool TryGetCastTargetDbTypeString(Type sourceType, Type targetType, out string dbTypeString, bool throwNotSupportedException = true)
        {
            dbTypeString = null;

            sourceType = ReflectionExtension.GetUnderlyingType(sourceType);
            targetType = ReflectionExtension.GetUnderlyingType(targetType);

            if (sourceType == targetType)
                return false;

            if (targetType == PublicConstants.TypeOfDecimal)
            {
                //Casting to Decimal is not supported when missing the precision and scale information.I have no idea to deal with this case now.
                if (sourceType != PublicConstants.TypeOfInt16 && sourceType != PublicConstants.TypeOfInt32 && sourceType != PublicConstants.TypeOfInt64 && sourceType != PublicConstants.TypeOfByte)
                {
                    if (throwNotSupportedException)
                        throw new NotSupportedException(PublicHelper.AppendNotSupportedCastErrorMsg(sourceType, targetType));
                    else
                        return false;
                }
            }

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
            generator.SqlBuilder.Append("DATEADD(");
            generator.SqlBuilder.Append(interval);
            generator.SqlBuilder.Append(",");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(",");
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
        public static void DbFunction_DATEPART(SqlGeneratorBase generator, string interval, DbExpression exp)
        {
            generator.SqlBuilder.Append("DATEPART(");
            generator.SqlBuilder.Append(interval);
            generator.SqlBuilder.Append(",");
            exp.Accept(generator);
            generator.SqlBuilder.Append(")");
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
            generator.SqlBuilder.Append("COUNT_BIG(1)");
        }
        public static void Aggregate_LongCount(SqlGeneratorBase generator, DbExpression arg)
        {
            generator.SqlBuilder.Append("COUNT_BIG(");
            arg.Accept(generator);
            generator.SqlBuilder.Append(")");
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
                generator.SqlBuilder.Append("ISNULL(");
                AppendAggregateFunction(generator, exp, retType, "SUM", true);
                generator.SqlBuilder.Append(",");
                generator.SqlBuilder.Append("0");
                generator.SqlBuilder.Append(")");
            }
        }
        public static void Aggregate_Average(SqlGeneratorBase generator, DbExpression exp, Type retType)
        {
            string targetDbType = null;

            Type underlyingType = ReflectionExtension.GetUnderlyingType(retType);
            if (underlyingType != exp.Type.GetUnderlyingType())
            {
                CastTypeMap.TryGetValue(underlyingType, out targetDbType);
            }

            generator.SqlBuilder.Append("AVG", "(");
            if (string.IsNullOrEmpty(targetDbType))
            {
                exp.Accept(generator);
            }
            else
            {
                generator.SqlBuilder.Append("CAST(");
                exp.Accept(generator);
                generator.SqlBuilder.Append(" AS ", targetDbType, ")");
            }

            generator.SqlBuilder.Append(")");
        }

        static void AppendAggregateFunction(SqlGeneratorBase generator, DbExpression exp, Type retType, string functionName, bool withCast)
        {
            string targetDbType = null;
            if (withCast == true)
            {
                Type underlyingType = ReflectionExtension.GetUnderlyingType(retType);
                if (underlyingType != PublicConstants.TypeOfDecimal/* We don't know the precision and scale,so,we can not cast exp to decimal,otherwise maybe cause problems. */ && CastTypeMap.TryGetValue(underlyingType, out targetDbType))
                {
                    generator.SqlBuilder.Append("CAST(");
                }
            }

            generator.SqlBuilder.Append(functionName, "(");
            exp.Accept(generator);
            generator.SqlBuilder.Append(")");

            if (targetDbType != null)
            {
                generator.SqlBuilder.Append(" AS ", targetDbType, ")");
            }
        }
        #endregion

    }
}
