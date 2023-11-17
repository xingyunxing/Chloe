using Chloe.Annotations;
using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.Reflection;
using System.Data;
using System.Reflection;

namespace Chloe.MySql
{
    partial class SqlGenerator : SqlGeneratorBase
    {
        DbParamCollection _parameters = new DbParamCollection();

        public static readonly Dictionary<string, IMethodHandler[]> MethodHandlerDic = InitMethodHandlers();
        static readonly Dictionary<string, Action<DbAggregateExpression, SqlGeneratorBase>> AggregateHandlerDic = InitAggregateHandlers();
        static readonly Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGeneratorBase>> BinaryWithMethodHandlersDic = InitBinaryWithMethodHandlers();
        static readonly Dictionary<Type, string> CastTypeMap;
        static readonly List<string> CacheParameterNames;

        static SqlGenerator()
        {
            Dictionary<Type, string> castTypeMap = new Dictionary<Type, string>();
            castTypeMap.Add(typeof(string), "CHAR");
            castTypeMap.Add(typeof(byte), "UNSIGNED");
            castTypeMap.Add(typeof(sbyte), "SIGNED");
            castTypeMap.Add(typeof(Int16), "SIGNED");
            castTypeMap.Add(typeof(UInt16), "UNSIGNED");
            castTypeMap.Add(typeof(int), "SIGNED");
            castTypeMap.Add(typeof(uint), "UNSIGNED");
            castTypeMap.Add(typeof(long), "SIGNED");
            castTypeMap.Add(typeof(ulong), "UNSIGNED");
            castTypeMap.Add(typeof(DateTime), "DATETIME");
            castTypeMap.Add(typeof(bool), "SIGNED");
            CastTypeMap = PublicHelper.Clone(castTypeMap);


            int cacheParameterNameCount = 2 * 12;
            List<string> cacheParameterNames = new List<string>(cacheParameterNameCount);
            for (int i = 0; i < cacheParameterNameCount; i++)
            {
                string paramName = UtilConstants.ParameterNamePrefix + i.ToString();
                cacheParameterNames.Add(paramName);
            }
            CacheParameterNames = cacheParameterNames;
        }

        public List<DbParam> Parameters { get { return this._parameters.ToParameterList(); } }

        protected override string LeftQuoteChar { get; } = UtilConstants.LeftQuoteChar;
        protected override string RightQuoteChar { get; } = UtilConstants.RightQuoteChar;
        protected override Dictionary<string, IMethodHandler[]> MethodHandlers { get; } = MethodHandlerDic;
        protected override Dictionary<string, Action<DbAggregateExpression, SqlGeneratorBase>> AggregateHandlers { get; } = AggregateHandlerDic;
        protected override Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGeneratorBase>> BinaryWithMethodHandlers { get; } = BinaryWithMethodHandlersDic;

        public static SqlGenerator CreateInstance()
        {
            return new SqlGenerator();
        }

        public override DbExpression Visit(DbJoinTableExpression exp)
        {
            if (exp.JoinType == DbJoinType.FullJoin)
            {
                throw new NotSupportedException("JoinType: " + exp.JoinType);
            }

            return base.Visit(exp);
        }

        public override DbExpression Visit(DbSqlQueryExpression exp)
        {
            //构建常规的查询
            this.BuildGeneralSql(exp);
            return exp;
        }

        public override DbExpression Visit(DbCoalesceExpression exp)
        {
            this.SqlBuilder.Append("IFNULL(");
            exp.CheckExpression.Accept(this);
            this.SqlBuilder.Append(",");
            exp.ReplacementValue.Accept(this);
            this.SqlBuilder.Append(")");

            return exp;
        }

        public override DbExpression Visit(DbConvertExpression exp)
        {
            DbExpression stripedExp = DbExpressionExtension.StripInvalidConvert(exp);

            if (stripedExp.NodeType != DbExpressionType.Convert)
            {
                stripedExp.Accept(this);
                return exp;
            }

            exp = (DbConvertExpression)stripedExp;

            string dbTypeString;
            if (TryGetCastTargetDbTypeString(exp.Operand.Type, exp.Type, out dbTypeString, false))
            {
                this.BuildCastState(exp.Operand, dbTypeString);
            }
            else
                exp.Operand.Accept(this);

            return exp;
        }

        protected override DbExpression VisitDbFunctionMethodCallExpression(DbMethodCallExpression exp)
        {
            DbFunctionAttribute dbFunction = exp.Method.GetCustomAttribute<DbFunctionAttribute>();
            string functionName = string.IsNullOrEmpty(dbFunction.Name) ? exp.Method.Name : dbFunction.Name;

            this.QuoteName(functionName);
            this.SqlBuilder.Append("(");

            string c = "";
            foreach (DbExpression argument in exp.Arguments)
            {
                this.SqlBuilder.Append(c);
                argument.Accept(this);
                c = ",";
            }

            this.SqlBuilder.Append(")");

            return exp;
        }

        public override DbExpression Visit(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;

            if (member.DeclaringType == PublicConstants.TypeOfDateTime)
            {
                if (member == PublicConstants.PropertyInfo_DateTime_Now)
                {
                    this.SqlBuilder.Append("NOW()");
                    return exp;
                }

                if (member == PublicConstants.PropertyInfo_DateTime_UtcNow)
                {
                    this.SqlBuilder.Append("UTC_TIMESTAMP()");
                    return exp;
                }

                if (member == PublicConstants.PropertyInfo_DateTime_Today)
                {
                    this.SqlBuilder.Append("CURDATE()");
                    return exp;
                }

                if (member == PublicConstants.PropertyInfo_DateTime_Date)
                {
                    this.SqlBuilder.Append("DATE(");
                    exp.Expression.Accept(this);
                    this.SqlBuilder.Append(")");
                    return exp;
                }

                if (this.IsDatePart(exp))
                {
                    return exp;
                }
            }

            return base.Visit(exp);
        }
        public override DbExpression Visit(DbConstantExpression exp)
        {
            if (exp.Value == null || exp.Value == DBNull.Value)
            {
                this.SqlBuilder.Append("NULL");
                return exp;
            }

            var objType = exp.Value.GetType();
            if (objType == PublicConstants.TypeOfBoolean)
            {
                this.SqlBuilder.Append(((bool)exp.Value) ? "1" : "0");
                return exp;
            }
            else if (objType == PublicConstants.TypeOfString)
            {
                this.SqlBuilder.Append("N'", exp.Value, "'");
                return exp;
            }
            else if (objType.IsEnum)
            {
                this.SqlBuilder.Append(Convert.ChangeType(exp.Value, Enum.GetUnderlyingType(objType)).ToString());
                return exp;
            }
            else if (PublicHelper.IsNumericType(exp.Value.GetType()))
            {
                this.SqlBuilder.Append(exp.Value);
                return exp;
            }

            DbParameterExpression p = new DbParameterExpression(exp.Value);
            p.Accept(this);

            return exp;
        }
        public override DbExpression Visit(DbParameterExpression exp)
        {
            object paramValue = exp.Value;
            Type paramType = exp.Type.GetUnderlyingType();

            if (paramType.IsEnum)
            {
                paramType = Enum.GetUnderlyingType(paramType);
                if (paramValue != null)
                    paramValue = Convert.ChangeType(paramValue, paramType);
            }

            if (paramValue == null)
                paramValue = DBNull.Value;

            DbParam p = this._parameters.Find(paramValue, paramType, exp.DbType);

            if (p != null)
            {
                this.SqlBuilder.Append(p.Name);
                return exp;
            }

            string paramName = GenParameterName(this._parameters.Count);
            p = DbParam.Create(paramName, paramValue, paramType);

            if (paramValue.GetType() == PublicConstants.TypeOfString)
            {
                if (exp.DbType == DbType.AnsiStringFixedLength || exp.DbType == DbType.StringFixedLength)
                    p.Size = ((string)paramValue).Length;
                else if (((string)paramValue).Length <= 4000)
                    p.Size = 4000;
            }

            if (exp.DbType != null)
                p.DbType = exp.DbType;

            this._parameters.Add(p);
            this.SqlBuilder.Append(paramName);
            return exp;
        }

        void BuildGeneralSql(DbSqlQueryExpression exp)
        {
            this.SqlBuilder.Append("SELECT ");

            if (exp.IsDistinct)
                this.SqlBuilder.Append("DISTINCT ");

            List<DbColumnSegment> columns = exp.ColumnSegments;
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegment column = columns[i];
                if (i > 0)
                    this.SqlBuilder.Append(",");

                this.AppendColumnSegment(column);
            }

            this.SqlBuilder.Append(" FROM ");
            exp.Table.Accept(this);
            this.BuildWhereState(exp.Condition);
            this.BuildGroupState(exp);
            this.BuildOrderState(exp.Orderings);

            if (exp.SkipCount != null || exp.TakeCount != null)
            {
                int skipCount = exp.SkipCount ?? 0;
                long takeCount = long.MaxValue;
                if (exp.TakeCount != null)
                    takeCount = exp.TakeCount.Value;

                this.SqlBuilder.Append(" LIMIT ", skipCount.ToString(), ",", takeCount.ToString());
            }

            DbTableSegment seg = exp.Table.Table;
            if (seg.Lock == LockType.UpdLock)
            {
                this.SqlBuilder.Append(" FOR UPDATE");
            }
            else if (seg.Lock == LockType.Unspecified || seg.Lock == LockType.NoLock)
            {
                //Do nothing.
            }
            else
                throw new NotSupportedException($"lock type: {seg.Lock.ToString()}");
        }

        bool IsDatePart(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;

            if (member == PublicConstants.PropertyInfo_DateTime_Year)
            {
                DbFunction_DATEPART(this, "YEAR", exp.Expression);
                return true;
            }

            if (member == PublicConstants.PropertyInfo_DateTime_Month)
            {
                DbFunction_DATEPART(this, "MONTH", exp.Expression);
                return true;
            }

            if (member == PublicConstants.PropertyInfo_DateTime_Day)
            {
                DbFunction_DATEPART(this, "DAY", exp.Expression);
                return true;
            }

            if (member == PublicConstants.PropertyInfo_DateTime_Hour)
            {
                DbFunction_DATEPART(this, "HOUR", exp.Expression);
                return true;
            }

            if (member == PublicConstants.PropertyInfo_DateTime_Minute)
            {
                DbFunction_DATEPART(this, "MINUTE", exp.Expression);
                return true;
            }

            if (member == PublicConstants.PropertyInfo_DateTime_Second)
            {
                DbFunction_DATEPART(this, "SECOND", exp.Expression);
                return true;
            }

            /* MySql is not supports MILLISECOND */


            if (member == PublicConstants.PropertyInfo_DateTime_DayOfWeek)
            {
                this.SqlBuilder.Append("(");
                DbFunction_DATEPART(this, "DAYOFWEEK", exp.Expression);
                this.SqlBuilder.Append(" - 1)");

                return true;
            }

            return false;
        }
    }
}
