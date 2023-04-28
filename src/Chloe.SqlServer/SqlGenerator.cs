using Chloe.Annotations;
using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using Chloe.RDBMS;
using Chloe.Reflection;
using System.Data;
using System.Reflection;

namespace Chloe.SqlServer
{
    partial class SqlGenerator : SqlGeneratorBase
    {
        DbParamCollection _parameters = new DbParamCollection();

        public static readonly Dictionary<string, IMethodHandler> MethodHandlerDic = InitMethodHandlers();
        static readonly Dictionary<string, Action<DbAggregateExpression, SqlGeneratorBase>> AggregateHandlerDic = InitAggregateHandlers();
        static readonly Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGeneratorBase>> BinaryWithMethodHandlersDic = InitBinaryWithMethodHandlers();
        static readonly Dictionary<Type, string> CastTypeMap;
        static readonly List<string> CacheParameterNames;

        static SqlGenerator()
        {
            Dictionary<Type, string> castTypeMap = new Dictionary<Type, string>();
            castTypeMap.Add(typeof(string), "NVARCHAR(MAX)");
            castTypeMap.Add(typeof(byte), "TINYINT");
            castTypeMap.Add(typeof(Int16), "SMALLINT");
            castTypeMap.Add(typeof(int), "INT");
            castTypeMap.Add(typeof(long), "BIGINT");
            castTypeMap.Add(typeof(float), "REAL");
            castTypeMap.Add(typeof(double), "FLOAT");
            castTypeMap.Add(typeof(decimal), "DECIMAL(19,0)");//I think this will be a bug.
            castTypeMap.Add(typeof(bool), "BIT");
            castTypeMap.Add(typeof(DateTime), "DATETIME");
            castTypeMap.Add(typeof(Guid), "UNIQUEIDENTIFIER");
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

        protected override string LeftQuoteChar { get; } = "[";
        protected override string RightQuoteChar { get; } = "]";
        protected override Dictionary<string, IMethodHandler> MethodHandlers { get; } = MethodHandlerDic;
        protected override Dictionary<string, Action<DbAggregateExpression, SqlGeneratorBase>> AggregateHandlers { get; } = AggregateHandlerDic;
        protected override Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGeneratorBase>> BinaryWithMethodHandlers { get; } = BinaryWithMethodHandlersDic;

        public static SqlGenerator CreateInstance()
        {
            return new SqlGenerator();
        }

        public override DbExpression Visit(DbSqlQueryExpression exp)
        {
            if (exp.SkipCount != null)
            {
                this.BuildLimitSql(exp);
                return exp;
            }
            else
            {
                //构建常规的查询
                this.BuildGeneralSql(exp);
                return exp;
            }

            throw new NotImplementedException();
        }
        public override DbExpression Visit(DbInsertExpression exp)
        {
            string separator = "";

            this.SqlBuilder.Append("INSERT INTO ");
            this.AppendTable(exp.Table);
            this.SqlBuilder.Append("(");

            separator = "";
            foreach (var item in exp.InsertColumns)
            {
                this.SqlBuilder.Append(separator);
                this.QuoteName(item.Key.Name);
                separator = ",";
            }

            this.SqlBuilder.Append(")");

            this.AppendOutputClause(exp.Returns);

            this.SqlBuilder.Append(" VALUES(");
            separator = "";
            foreach (var item in exp.InsertColumns)
            {
                this.SqlBuilder.Append(separator);

                DbExpression valExp = DbExpressionExtension.StripInvalidConvert(item.Value);
                PublicHelper.AmendDbInfo(item.Key, valExp);
                DbValueExpressionTransformer.Transform(valExp).Accept(this);
                separator = ",";
            }

            this.SqlBuilder.Append(")");

            return exp;
        }
        public override DbExpression Visit(DbUpdateExpression exp)
        {
            this.SqlBuilder.Append("UPDATE ");
            this.AppendTable(exp.Table);
            this.SqlBuilder.Append(" SET ");

            bool first = true;
            foreach (var item in exp.UpdateColumns)
            {
                if (first)
                    first = false;
                else
                    this.SqlBuilder.Append(",");

                this.QuoteName(item.Key.Name);
                this.SqlBuilder.Append("=");

                DbExpression valExp = DbExpressionExtension.StripInvalidConvert(item.Value);
                PublicHelper.AmendDbInfo(item.Key, valExp);
                DbValueExpressionTransformer.Transform(valExp).Accept(this);
            }

            this.AppendOutputClause(exp.Returns);
            this.BuildWhereState(exp.Condition);

            return exp;
        }
        public override DbExpression Visit(DbDeleteExpression exp)
        {
            this.SqlBuilder.Append("DELETE ");
            this.AppendTable(exp.Table);
            this.BuildWhereState(exp.Condition);

            return exp;
        }

        public override DbExpression Visit(DbCoalesceExpression exp)
        {
            this.SqlBuilder.Append("ISNULL(");
            EnsureDbExpressionReturnCSharpBoolean(exp.CheckExpression).Accept(this);
            this.SqlBuilder.Append(",");
            EnsureDbExpressionReturnCSharpBoolean(exp.ReplacementValue).Accept(this);
            this.SqlBuilder.Append(")");

            return exp;
        }
        // then 部分必须返回 C# type，所以得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
        public override DbExpression Visit(DbCaseWhenExpression exp)
        {
            this.SqlBuilder.Append("CASE");
            foreach (var whenThen in exp.WhenThenPairs)
            {
                // then 部分得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
                this.SqlBuilder.Append(" WHEN ");
                whenThen.When.Accept(this);
                this.SqlBuilder.Append(" THEN ");
                EnsureDbExpressionReturnCSharpBoolean(whenThen.Then).Accept(this);
            }

            this.SqlBuilder.Append(" ELSE ");
            EnsureDbExpressionReturnCSharpBoolean(exp.Else).Accept(this);
            this.SqlBuilder.Append(" END");

            return exp;
        }
        public override DbExpression Visit(DbConvertExpression exp)
        {
            DbExpression stripedExp = DbExpressionExtension.StripInvalidConvert(exp);

            if (stripedExp.NodeType != DbExpressionType.Convert)
            {
                EnsureDbExpressionReturnCSharpBoolean(stripedExp).Accept(this);
                return exp;
            }

            exp = (DbConvertExpression)stripedExp;

            string dbTypeString;
            if (TryGetCastTargetDbTypeString(exp.Operand.Type, exp.Type, out dbTypeString))
            {
                this.BuildCastState(EnsureDbExpressionReturnCSharpBoolean(exp.Operand), dbTypeString);
            }
            else
                EnsureDbExpressionReturnCSharpBoolean(exp.Operand).Accept(this);

            return exp;
        }

        public override DbExpression Visit(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;

            if (member.DeclaringType == PublicConstants.TypeOfDateTime)
            {
                if (member == PublicConstants.PropertyInfo_DateTime_Now)
                {
                    this.SqlBuilder.Append("GETDATE()");
                    return exp;
                }

                if (member == PublicConstants.PropertyInfo_DateTime_UtcNow)
                {
                    this.SqlBuilder.Append("GETUTCDATE()");
                    return exp;
                }

                if (member == PublicConstants.PropertyInfo_DateTime_Today)
                {
                    this.BuildCastState("GETDATE()", "DATE");
                    return exp;
                }

                if (member == PublicConstants.PropertyInfo_DateTime_Date)
                {
                    this.BuildCastState(exp.Expression, "DATE");
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
                this.SqlBuilder.Append(((bool)exp.Value) ? "CAST(1 AS BIT)" : "CAST(0 AS BIT)");
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

        protected override DbExpression VisitStringLengthMemberAccessExpression(DbMemberExpression exp)
        {
            this.SqlBuilder.Append("LEN(");
            exp.Expression.Accept(this);
            this.SqlBuilder.Append(")");

            return exp;
        }
        protected override DbExpression VisitDbFunctionMethodCallExpression(DbMethodCallExpression exp)
        {
            DbFunctionAttribute dbFunction = exp.Method.GetCustomAttribute<DbFunctionAttribute>();
            string schema = string.IsNullOrEmpty(dbFunction.Schema) ? "dbo" : dbFunction.Schema;
            string functionName = string.IsNullOrEmpty(dbFunction.Name) ? exp.Method.Name : dbFunction.Name;

            this.QuoteName(schema);
            this.SqlBuilder.Append(".");
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

        protected override void AppendTableSegment(DbTableSegment seg)
        {
            base.AppendTableSegment(seg);

            string lockString = null;
            switch (seg.Lock)
            {
                case LockType.Unspecified:
                    return;
                case LockType.NoLock:
                    lockString = "NOLOCK";
                    break;
                case LockType.UpdLock:
                    lockString = "UPDLOCK";
                    break;
                default:
                    throw new NotSupportedException($"lock type: {seg.Lock.ToString()}");
            }

            this.SqlBuilder.Append(" WITH(", lockString, ")");
        }
        protected override void AppendColumnSegment(DbColumnSegment seg)
        {
            DbValueExpressionTransformer.Transform(seg.Body).Accept(this);
            this.SqlBuilder.Append(" AS ");
            this.QuoteName(seg.Alias);
        }

        void BuildGeneralSql(DbSqlQueryExpression exp)
        {
            this.SqlBuilder.Append("SELECT ");

            this.AppendDistinct(exp.IsDistinct);

            if (exp.TakeCount != null)
                this.SqlBuilder.Append("TOP ", exp.TakeCount.ToString(), " ");

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
        }
        protected virtual void BuildLimitSql(DbSqlQueryExpression exp)
        {
            bool shouldSortResults = false;
            if (exp.TakeCount != null)
                shouldSortResults = true;
            else if (this.SqlBuilder.Length == 0)
                shouldSortResults = true;

            this.SqlBuilder.Append("SELECT ");

            this.AppendDistinct(exp.IsDistinct);

            if (exp.TakeCount != null)
                this.SqlBuilder.Append("TOP ", exp.TakeCount.ToString(), " ");

            string tableAlias = "T";

            List<DbColumnSegment> columns = exp.ColumnSegments;
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegment column = columns[i];
                if (i > 0)
                    this.SqlBuilder.Append(",");

                this.QuoteName(tableAlias);
                this.SqlBuilder.Append(".");
                this.QuoteName(column.Alias);
                this.SqlBuilder.Append(" AS ");
                this.QuoteName(column.Alias);
            }

            this.SqlBuilder.Append(" FROM ");
            this.SqlBuilder.Append("(");

            //------------------------//
            this.SqlBuilder.Append("SELECT ");
            for (int i = 0; i < columns.Count; i++)
            {
                DbColumnSegment column = columns[i];
                if (i > 0)
                    this.SqlBuilder.Append(",");

                DbValueExpressionTransformer.Transform(column.Body).Accept(this);
                this.SqlBuilder.Append(" AS ");
                this.QuoteName(column.Alias);
            }

            List<DbOrdering> orderings = exp.Orderings;
            if (orderings.Count == 0)
            {
                DbOrdering ordering = new DbOrdering(PublicConstants.DbParameter_1, DbOrderType.Asc);
                orderings = new List<DbOrdering>(1);
                orderings.Add(ordering);
            }

            string row_numberName = GenRowNumberName(columns);
            this.SqlBuilder.Append(",ROW_NUMBER() OVER(ORDER BY ");
            this.ConcatOrderings(orderings);
            this.SqlBuilder.Append(") AS ");
            this.QuoteName(row_numberName);
            this.SqlBuilder.Append(" FROM ");
            exp.Table.Accept(this);
            this.BuildWhereState(exp.Condition);
            this.BuildGroupState(exp);
            //------------------------//

            this.SqlBuilder.Append(")");
            this.SqlBuilder.Append(" AS ");
            this.QuoteName(tableAlias);
            this.SqlBuilder.Append(" WHERE ");
            this.QuoteName(tableAlias);
            this.SqlBuilder.Append(".");
            this.QuoteName(row_numberName);
            this.SqlBuilder.Append(" > ");
            this.SqlBuilder.Append(exp.SkipCount.ToString());

            if (shouldSortResults)
            {
                this.SqlBuilder.Append(" ORDER BY ");
                this.QuoteName(tableAlias);
                this.SqlBuilder.Append(".");
                this.QuoteName(row_numberName);
                this.SqlBuilder.Append(" ASC");
            }
        }
        protected void AppendDistinct(bool isDistinct)
        {
            if (isDistinct)
                this.SqlBuilder.Append("DISTINCT ");
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

            if (member == PublicConstants.PropertyInfo_DateTime_Millisecond)
            {
                DbFunction_DATEPART(this, "MILLISECOND", exp.Expression);
                return true;
            }

            if (member == PublicConstants.PropertyInfo_DateTime_DayOfWeek)
            {
                this.SqlBuilder.Append("(");
                DbFunction_DATEPART(this, "WEEKDAY", exp.Expression);
                this.SqlBuilder.Append(" - 1)");

                return true;
            }

            return false;
        }

        void AppendOutputClause(List<DbColumn> returns)
        {
            if (returns.Count > 0)
            {
                this.SqlBuilder.Append(" output ");
                string separator = "";
                foreach (DbColumn returnColumn in returns)
                {
                    this.SqlBuilder.Append(separator);
                    this.SqlBuilder.Append("inserted.");
                    this.QuoteName(returnColumn.Name);
                    separator = ",";
                }
            }
        }
    }
}
