﻿using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.Reflection;
using System.Data;
using System.Reflection;

namespace Chloe.Oracle
{
    partial class SqlGenerator : SqlGeneratorBase
    {
        static readonly object Boxed_1 = 1;
        static readonly object Boxed_0 = 0;

        DbParamCollection _parameters = new DbParamCollection();

        public static readonly Dictionary<string, IPropertyHandler[]> PropertyHandlerDic = InitPropertyHandlers();
        public static readonly Dictionary<string, IMethodHandler[]> MethodHandlerDic = InitMethodHandlers();
        static readonly Dictionary<string, Action<DbAggregateExpression, SqlGeneratorBase>> AggregateHandlerDic = InitAggregateHandlers();
        static readonly Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGeneratorBase>> BinaryWithMethodHandlersDic = InitBinaryWithMethodHandlers();
        static readonly Dictionary<Type, string> CastTypeMap;
        static readonly List<string> CacheParameterNames;

        static SqlGenerator()
        {
            Dictionary<Type, string> castTypeMap = new Dictionary<Type, string>();
            //castTypeMap.Add(typeof(string), "NVARCHAR2"); // instead of using to_char(exp) 
            castTypeMap.Add(typeof(byte), "NUMBER(3,0)");
            castTypeMap.Add(typeof(Int16), "NUMBER(5,0)");
            castTypeMap.Add(typeof(int), "NUMBER(10,0)");
            castTypeMap.Add(typeof(long), "NUMBER(19,0)");
            castTypeMap.Add(typeof(float), "BINARY_FLOAT");
            castTypeMap.Add(typeof(double), "BINARY_DOUBLE");
            castTypeMap.Add(typeof(decimal), "NUMBER");
            castTypeMap.Add(typeof(bool), "NUMBER(10,0)");
            //castTypeMap.Add(typeof(DateTime), "DATE"); // instead of using TO_TIMESTAMP(exp) 
            //castTypeMap.Add(typeof(Guid), "BLOB");
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

        public SqlGenerator(OracleSqlGeneratorOptions options) : base(options)
        {
            this.Options = options;
        }

        public new OracleSqlGeneratorOptions Options { get; set; }

        public List<DbParam> Parameters { get { return this._parameters.ToParameterList(); } }

        protected override Dictionary<string, IPropertyHandler[]> PropertyHandlers { get; } = PropertyHandlerDic;
        protected override Dictionary<string, IMethodHandler[]> MethodHandlers { get; } = MethodHandlerDic;
        protected override Dictionary<string, Action<DbAggregateExpression, SqlGeneratorBase>> AggregateHandlers { get; } = AggregateHandlerDic;
        protected override Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGeneratorBase>> BinaryWithMethodHandlers { get; } = BinaryWithMethodHandlersDic;

        public override DbExpression Visit(DbNotEqualExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = DbExpressionExtension.StripInvalidConvert(left);
            right = DbExpressionExtension.StripInvalidConvert(right);

            MethodInfo method_Sql_IsNotEqual = PublicConstants.MethodInfo_Sql_IsNotEqual.MakeGenericMethod(left.Type);

            /* Sql.IsNotEqual(left, right) */
            DbMethodCallExpression left_not_equals_right = DbExpression.MethodCall(null, method_Sql_IsNotEqual, new List<DbExpression>(2) { left, right });

            //明确 left right 其中一边一定为 null
            if (DbExpressionHelper.AffirmExpressionRetValueIsNullOrEmpty(right) || DbExpressionHelper.AffirmExpressionRetValueIsNullOrEmpty(left))
            {
                /*
                 * a.Name != null --> a.Name != null
                 */

                left_not_equals_right.Accept(this);
                return exp;
            }

            if (right.NodeType == DbExpressionType.SubQuery || left.NodeType == DbExpressionType.SubQuery)
            {
                /*
                 * a.Id != (select top 1 T.Id from T) --> a.Id <> (select top 1 T.Id from T)，对于这种查询，我们不考虑 null
                 */

                left_not_equals_right.Accept(this);
                return exp;
            }

            MethodInfo method_Sql_IsEqual = PublicConstants.MethodInfo_Sql_IsEqual.MakeGenericMethod(left.Type);

            if (left.NodeType == DbExpressionType.Parameter || left.NodeType == DbExpressionType.Constant)
            {
                var t = right;
                right = left;
                left = t;
            }
            if (right.NodeType == DbExpressionType.Parameter || right.NodeType == DbExpressionType.Constant)
            {
                /*
                 * 走到这说明 name 不可能为 null
                 * a.Name != name --> a.Name <> name or a.Name is null
                 */

                if (left.NodeType != DbExpressionType.Parameter && left.NodeType != DbExpressionType.Constant)
                {
                    /*
                     * a.Name != name --> a.Name <> name or a.Name is null
                     */

                    /* Sql.IsEqual(left, null) */
                    var left_is_null1 = DbExpression.MethodCall(null, method_Sql_IsEqual, new List<DbExpression>(2) { left, DbExpression.Constant(null, left.Type) });

                    /* Sql.IsNotEqual(left, right) || Sql.IsEqual(left, null) */
                    var left_not_equals_right_or_left_is_null = DbExpression.Or(left_not_equals_right, left_is_null1);
                    left_not_equals_right_or_left_is_null.Accept(this);
                }
                else
                {
                    /*
                     * name != name1 --> name <> name，其中 name 和 name1 都为变量且都不可能为 null
                     */

                    left_not_equals_right.Accept(this);
                }

                return exp;
            }


            /*
             * a.Name != a.XName --> a.Name <> a.XName or (a.Name is null and a.XName is not null) or (a.Name is not null and a.XName is null)
             * ## a.Name != a.XName 不能翻译成：not (a.Name == a.XName or (a.Name is null and a.XName is null))，因为数据库里的 not 有时候并非真正意义上的“取反”！
             * 当 a.Name 或者 a.XName 其中一个字段有为 NULL，另一个字段有值时，会查不出此条数据 ##
             */

            DbConstantExpression null_Constant = DbExpression.Constant(null, left.Type);

            /* Sql.IsEqual(left, null) */
            var left_is_null = DbExpression.MethodCall(null, method_Sql_IsEqual, new List<DbExpression>(2) { left, null_Constant });
            /* Sql.IsNotEqual(left, null) */
            var left_is_not_null = DbExpression.MethodCall(null, method_Sql_IsNotEqual, new List<DbExpression>(2) { left, null_Constant });

            /* Sql.IsEqual(right, null) */
            var right_is_null = DbExpression.MethodCall(null, method_Sql_IsEqual, new List<DbExpression>(2) { right, null_Constant });
            /* Sql.IsNotEqual(right, null) */
            var right_is_not_null = DbExpression.MethodCall(null, method_Sql_IsNotEqual, new List<DbExpression>(2) { right, null_Constant });

            /* Sql.IsEqual(left, null) && Sql.IsNotEqual(right, null) */
            var left_is_null_and_right_is_not_null = DbExpression.And(left_is_null, right_is_not_null);

            /* Sql.IsNotEqual(left, null) && Sql.IsEqual(right, null) */
            var left_is_not_null_and_right_is_null = DbExpression.And(left_is_not_null, right_is_null);

            /* (Sql.IsEqual(left, null) && Sql.IsNotEqual(right, null)) || (Sql.IsNotEqual(left, null) && Sql.IsEqual(right, null)) */
            var left_is_null_and_right_is_not_null_or_left_is_not_null_and_right_is_null = DbExpression.Or(left_is_null_and_right_is_not_null, left_is_not_null_and_right_is_null);

            /* Sql.IsNotEqual(left, right) || (Sql.IsEqual(left, null) && Sql.IsNotEqual(right, null)) || (Sql.IsNotEqual(left, null) && Sql.IsEqual(right, null)) */
            var e = DbExpression.Or(left_not_equals_right, left_is_null_and_right_is_not_null_or_left_is_not_null_and_right_is_null);

            e.Accept(this);

            return exp;
        }

        public override DbExpression Visit(DbBitAndExpression exp)
        {
            this.SqlBuilder.Append("BITAND(");
            exp.Left.Accept(this);
            this.SqlBuilder.Append(",");
            exp.Left.Accept(this);
            this.SqlBuilder.Append(")");

            return exp;
        }
        public override DbExpression Visit(DbBitOrExpression exp)
        {
            throw new NotSupportedException("'|' operator is not supported.");
        }

        // %
        public override DbExpression Visit(DbModuloExpression exp)
        {
            this.SqlBuilder.Append("MOD(");
            exp.Left.Accept(this);
            this.SqlBuilder.Append(",");
            exp.Right.Accept(this);
            this.SqlBuilder.Append(")");

            return exp;
        }

        public override DbExpression Visit(DbAggregateExpression exp)
        {
            Action<DbAggregateExpression, SqlGeneratorBase> aggregateHandler;
            if (!AggregateHandlers.TryGetValue(exp.Method.Name, out aggregateHandler))
            {
                throw PublicHelper.MakeNotSupportedMethodException(exp.Method);
            }

            aggregateHandler(exp, this);
            return exp;
        }

        public override DbExpression Visit(DbSqlQueryExpression exp)
        {
            if (exp.TakeCount != null)
            {
                DbSqlQueryExpression newSqlQuery = CloneWithoutLimitInfo(exp, "TTAKE");

                if (exp.SkipCount == null)
                    AppendLimitCondition(newSqlQuery, exp.TakeCount.Value);
                else
                {
                    AppendLimitCondition(newSqlQuery, exp.TakeCount.Value + exp.SkipCount.Value);
                    newSqlQuery.SkipCount = exp.SkipCount.Value;
                }

                newSqlQuery.IsDistinct = exp.IsDistinct;
                newSqlQuery.Accept(this);
                return exp;
            }
            else if (exp.SkipCount != null)
            {
                DbSqlQueryExpression subSqlQuery = CloneWithoutLimitInfo(exp, "TSKIP");

                string row_numberName = GenRowNumberName(subSqlQuery.ColumnSegments);
                DbColumnSegment row_numberSeg = new DbColumnSegment(OracleSemantics.DbMemberExpression_ROWNUM, row_numberName);
                subSqlQuery.ColumnSegments.Add(row_numberSeg);

                DbTable table = new DbTable("T");
                DbSqlQueryExpression newSqlQuery = WrapSqlQuery(subSqlQuery, table, exp.ColumnSegments);

                DbColumnAccessExpression columnAccessExp = new DbColumnAccessExpression(table, DbColumn.MakeColumn(row_numberSeg.Body, row_numberName));
                newSqlQuery.Condition = DbExpression.GreaterThan(columnAccessExp, DbExpression.Constant(exp.SkipCount.Value));

                newSqlQuery.IsDistinct = exp.IsDistinct;
                newSqlQuery.Accept(this);
                return exp;
            }

            this.BuildGeneralSql(exp);
            return exp;
        }
        public override DbExpression Visit(DbInsertExpression exp)
        {
            this.SqlBuilder.Append("INSERT INTO ");
            this.AppendTable(exp.Table);
            this.SqlBuilder.Append("(");

            bool first = true;
            foreach (var item in exp.InsertColumns)
            {
                if (first)
                    first = false;
                else
                {
                    this.SqlBuilder.Append(",");
                }

                this.QuoteName(item.Column.Name);
            }

            this.SqlBuilder.Append(")");

            this.SqlBuilder.Append(" VALUES(");
            first = true;
            foreach (var item in exp.InsertColumns)
            {
                if (first)
                    first = false;
                else
                {
                    this.SqlBuilder.Append(",");
                }

                DbExpression valExp = item.Value.StripInvalidConvert();
                PublicHelper.AmendDbInfo(item.Column, valExp);
                DbValueExpressionTransformer.Transform(valExp).Accept(this);
            }

            this.SqlBuilder.Append(")");

            if (exp.Returns.Count > 0)
            {
                this.SqlBuilder.Append(" RETURNING ");

                string outputParamNames = "";
                for (int i = 0; i < exp.Returns.Count; i++)
                {
                    if (i > 0)
                    {
                        this.SqlBuilder.Append(",");
                        outputParamNames = outputParamNames + ",";
                    }

                    DbColumn outputColumn = exp.Returns[i];
                    string paramName = Utils.GenOutputColumnParameterName(outputColumn.Name);
                    DbParam outputParam = new DbParam() { Name = paramName, DbType = outputColumn.DbType, Precision = outputColumn.Precision, Scale = outputColumn.Scale, Size = outputColumn.Size, Value = DBNull.Value, Direction = ParamDirection.Output };
                    outputParam.Type = outputColumn.Type;

                    this.QuoteName(outputColumn.Name);
                    outputParamNames = outputParamNames + paramName;

                    this._parameters.Add(outputParam);
                }

                this.SqlBuilder.Append(" INTO ", outputParamNames);
            }

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

                this.QuoteName(item.Column.Name);
                this.SqlBuilder.Append("=");

                DbExpression valExp = item.Value.StripInvalidConvert();
                PublicHelper.AmendDbInfo(item.Column, valExp);
                DbValueExpressionTransformer.Transform(valExp).Accept(this);
            }

            this.BuildWhereState(exp.Condition);

            return exp;
        }

        public override DbExpression Visit(DbCoalesceExpression exp)
        {
            this.SqlBuilder.Append("NVL(");
            EnsureDbExpressionReturnCSharpBoolean(exp.CheckExpression).Accept(this);
            this.SqlBuilder.Append(",");
            EnsureDbExpressionReturnCSharpBoolean(exp.ReplacementValue).Accept(this);
            this.SqlBuilder.Append(")");

            return exp;
        }
        // then 部分必须返回 C# type，所以得判断是否是诸如 a>1,a=b,in,like 等等的情况，如果是则将其构建成一个 case when 
        public override DbExpression Visit(DbCaseWhenExpression exp)
        {
            this.LeftBracket();

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

            this.RightBracket();

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

            if (exp.Type == PublicConstants.TypeOfString)
            {
                this.SqlBuilder.Append("TO_CHAR(");
                exp.Operand.Accept(this);
                this.SqlBuilder.Append(")");
                return exp;
            }

            if (exp.Type == PublicConstants.TypeOfDateTime)
            {
                this.SqlBuilder.Append("TO_TIMESTAMP(");
                exp.Operand.Accept(this);
                this.SqlBuilder.Append(",'yyyy-mm-dd hh24:mi:ssxff')");
                return exp;
            }

            string dbTypeString;
            if (TryGetCastTargetDbTypeString(exp.Operand.Type, exp.Type, out dbTypeString, false))
            {
                BuildCastState(this, EnsureDbExpressionReturnCSharpBoolean(exp.Operand), dbTypeString);
            }
            else
                EnsureDbExpressionReturnCSharpBoolean(exp.Operand).Accept(this);

            return exp;
        }

        public override DbExpression Visit(DbMemberExpression exp)
        {
            if (this.IsDateSubtract(exp))
            {
                return exp;
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
                if (string.Empty.Equals(exp.Value))
                    this.SqlBuilder.Append("'", exp.Value, "'");
                else
                    this.SqlBuilder.Append("N'", exp.Value, "'");
                return exp;
            }
            else if (objType.IsEnum)
            {
                this.SqlBuilder.Append(Convert.ChangeType(exp.Value, Enum.GetUnderlyingType(objType)).ToString());
                return exp;
            }
            else if (PublicHelper.IsToStringableNumericType(exp.Value.GetType()))
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
            DbType? dbType = exp.DbType;

            if (paramType.IsEnum)
            {
                paramType = Enum.GetUnderlyingType(paramType);
                if (paramValue != null)
                    paramValue = Convert.ChangeType(paramValue, paramType);
            }
            else if (paramType == PublicConstants.TypeOfBoolean)
            {
                paramType = PublicConstants.TypeOfInt32;
                if (paramValue != null)
                {
                    paramValue = (bool)paramValue ? Boxed_1 : Boxed_0;
                }

                if (dbType == null || dbType == DbType.Boolean)
                {
                    dbType = DbType.Int32;
                }
            }

            if (paramValue == null)
                paramValue = DBNull.Value;

            DbParam p = this._parameters.Find(paramValue, paramType, dbType);

            if (p != null)
            {
                this.SqlBuilder.Append(p.Name);
                return exp;
            }

            string paramName = GenParameterName(this._parameters.Count);
            p = DbParam.Create(paramName, paramValue, paramType);

            if (paramValue.GetType() == PublicConstants.TypeOfString)
            {
                if (dbType == DbType.AnsiStringFixedLength || dbType == DbType.StringFixedLength)
                    p.Size = ((string)paramValue).Length;
                else if (((string)paramValue).Length <= 4000)
                    p.Size = 4000;
            }

            if (dbType != null)
                p.DbType = dbType;

            this._parameters.Add(p);
            this.SqlBuilder.Append(paramName);
            return exp;
        }


        protected override void AppendTableSegment(DbTableSegment seg)
        {
            seg.Body.Accept(this);
            this.SqlBuilder.Append(" ");
            this.QuoteName(seg.Alias);
        }
        protected override void AppendColumnSegment(DbColumnSegment seg)
        {
            var e = DbValueExpressionTransformer.Transform(seg.Body);
            e.Accept(this);

            if (e.IsColumnAccessWithName(seg.Alias))
            {
                return;
            }

            this.SqlBuilder.Append(" AS ");
            this.QuoteName(seg.Alias);
        }

        void BuildGeneralSql(DbSqlQueryExpression exp)
        {
            if (exp.TakeCount != null || exp.SkipCount != null)
                throw new ArgumentException();

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

        public override void QuoteName(string name)
        {
            if (this.Options.ConvertToUppercase)
                name = name.ToUpper();

            base.QuoteName(name);
        }

        bool IsDateSubtract(DbMemberExpression exp)
        {
            MemberInfo member = exp.Member;

            if (member.DeclaringType == PublicConstants.TypeOfTimeSpan)
            {
                if (exp.Expression.NodeType == DbExpressionType.Call)
                {
                    DbMethodCallExpression dbMethodExp = (DbMethodCallExpression)exp.Expression;
                    if (dbMethodExp.Method == PublicConstants.MethodInfo_DateTime_Subtract_DateTime)
                    {
                        int? intervalDivisor = null;

                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalDays)
                        {
                            intervalDivisor = 24 * 60 * 60 * 1000;
                            goto appendIntervalTime;
                        }
                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalHours)
                        {
                            intervalDivisor = 60 * 60 * 1000;
                            goto appendIntervalTime;
                        }
                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalMinutes)
                        {
                            intervalDivisor = 60 * 1000;
                            goto appendIntervalTime;
                        }
                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalSeconds)
                        {
                            intervalDivisor = 1000;
                            goto appendIntervalTime;
                        }
                        if (member == UtilConstants.PropertyInfo_TimeSpan_TotalMilliseconds)
                        {
                            intervalDivisor = 1;
                            goto appendIntervalTime;
                        }

                        return false;

                    appendIntervalTime:
                        this.CalcDateDiffPrecise(dbMethodExp.Object, dbMethodExp.Arguments[0], intervalDivisor.Value);
                        return true;
                    }
                }
                else
                {
                    DbSubtractExpression dbSubtractExp = exp.Expression as DbSubtractExpression;
                    if (dbSubtractExp != null && dbSubtractExp.Left.Type == PublicConstants.TypeOfDateTime && dbSubtractExp.Right.Type == PublicConstants.TypeOfDateTime)
                    {
                        DbMethodCallExpression dbMethodExp = new DbMethodCallExpression(dbSubtractExp.Left, PublicConstants.MethodInfo_DateTime_Subtract_DateTime, new List<DbExpression>(1) { dbSubtractExp.Right });
                        DbMemberExpression dbMemberExp = DbExpression.MemberAccess(member, dbMethodExp);
                        dbMemberExp.Accept(this);

                        return true;
                    }
                }
            }

            return false;
        }

        void CalcDateDiffPrecise(DbExpression dateTime1, DbExpression dateTime2, int divisor)
        {
            if (divisor == 1)
            {
                this.CalcDateDiffMillisecond(dateTime1, dateTime2);
                return;
            }

            this.LeftBracket();
            this.CalcDateDiffMillisecond(dateTime1, dateTime2);
            this.SqlBuilder.Append(" / ");
            this.SqlBuilder.Append(divisor.ToString());
            this.RightBracket();
        }
        void CalcDateDiffMillisecond(DbExpression dateTime1, DbExpression dateTime2)
        {
            /*
             * 计算两个日期相差的毫秒数：
             * (cast(dateTime1 as date)-cast(dateTime2 as date)) * 24 * 60 * 60 * 1000 
             * +
             * cast(to_char(cast(dateTime1 as timestamp),'ff3') as number)
             * -
             * cast(to_char(cast(dateTime2 as timestamp),'ff3') as number) 
             */

            this.LeftBracket();
            this.CalcDateDiffMillisecondSketchy(dateTime1, dateTime2);
            this.SqlBuilder.Append(" + ");
            this.ExtractMillisecondPart(dateTime1);
            this.SqlBuilder.Append(" - ");
            this.ExtractMillisecondPart(dateTime2);
            this.RightBracket();
        }
        void CalcDateDiffMillisecondSketchy(DbExpression dateTime1, DbExpression dateTime2)
        {
            /*
             * 计算去掉毫秒部分后两个日期相差的毫秒数：
             * (cast(dateTime1 as date)-cast(dateTime2 as date)) * 24 * 60 * 60 * 1000 
             */
            this.LeftBracket();
            BuildCastState(this, dateTime1, "DATE");
            this.SqlBuilder.Append("-");
            BuildCastState(this, dateTime2, "DATE");
            this.RightBracket();

            this.SqlBuilder.Append(" * ");
            this.SqlBuilder.Append((24 * 60 * 60 * 1000).ToString());
        }
        void ExtractMillisecondPart(DbExpression dateTime)
        {
            /* 提取一个日期的毫秒部分：
             * cast(to_char(cast(dateTime as timestamp),'ff3') as number) 
             */
            this.SqlBuilder.Append("CAST(");

            this.SqlBuilder.Append("TO_CHAR(");
            BuildCastState(this, dateTime, "TIMESTAMP");
            this.SqlBuilder.Append(",'ff3')");

            this.SqlBuilder.Append(" AS NUMBER)");
        }
    }
}
