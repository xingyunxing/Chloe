using Chloe.Annotations;
using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.Reflection;
using System.Data;
using System.Reflection;

namespace Chloe.SQLite
{
    partial class SqlGenerator : SqlGeneratorBase
    {
        DbParamCollection _parameters = new DbParamCollection();

        public static readonly Dictionary<string, IPropertyHandler[]> PropertyHandlerDic = InitPropertyHandlers();
        public static readonly Dictionary<string, IMethodHandler[]> MethodHandlerDic = InitMethodHandlers();
        static readonly Dictionary<string, Action<DbAggregateExpression, SqlGeneratorBase>> AggregateHandlerDic = InitAggregateHandlers();
        static readonly Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGeneratorBase>> BinaryWithMethodHandlersDic = InitBinaryWithMethodHandlers();
        public static readonly Dictionary<Type, string> CastTypeMap;
        static readonly List<string> CacheParameterNames;

        static SqlGenerator()
        {
            Dictionary<Type, string> castTypeMap = new Dictionary<Type, string>();
            castTypeMap.Add(typeof(string), "TEXT");
            castTypeMap.Add(typeof(byte), "INTEGER");
            castTypeMap.Add(typeof(Int16), "INTEGER");
            castTypeMap.Add(typeof(int), "INTEGER");
            castTypeMap.Add(typeof(long), "INTEGER");
            castTypeMap.Add(typeof(float), "REAL");
            castTypeMap.Add(typeof(double), "REAL");
            //castTypeMap.Add(typeof(decimal), "DECIMAL(19,0)");//I think this will be a bug.
            castTypeMap.Add(typeof(bool), "INTEGER");
            CastTypeMap = PublicHelper.Clone(castTypeMap);


            int cacheParameterNameCount = 4 * 12;
            List<string> cacheParameterNames = new List<string>(cacheParameterNameCount);
            for (int i = 0; i < cacheParameterNameCount; i++)
            {
                string paramName = UtilConstants.ParameterNamePrefix + i.ToString();
                cacheParameterNames.Add(paramName);
            }
            CacheParameterNames = cacheParameterNames;
        }

        public SqlGenerator(SqlGeneratorOptions options) : base(options)
        {

        }

        public List<DbParam> Parameters { get { return this._parameters.ToParameterList(); } }

        protected override Dictionary<string, IPropertyHandler[]> PropertyHandlers { get; } = PropertyHandlerDic;
        protected override Dictionary<string, IMethodHandler[]> MethodHandlers { get; } = MethodHandlerDic;
        protected override Dictionary<string, Action<DbAggregateExpression, SqlGeneratorBase>> AggregateHandlers { get; } = AggregateHandlerDic;
        protected override Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGeneratorBase>> BinaryWithMethodHandlers { get; } = BinaryWithMethodHandlersDic;

        public override DbExpression VisitJoinTable(DbJoinTableExpression exp)
        {
            if (exp.JoinType == DbJoinType.InnerJoin || exp.JoinType == DbJoinType.LeftJoin)
            {
                return base.VisitJoinTable(exp);
            }

            throw new NotSupportedException("JoinType: " + exp.JoinType);
        }

        public override DbExpression VisitSqlQuery(DbSqlQueryExpression exp)
        {
            this.BuildGeneralSql(exp);
            return exp;
        }

        public override DbExpression VisitCoalesce(DbCoalesceExpression exp)
        {
            this.SqlBuilder.Append("IFNULL(");
            exp.CheckExpression.Accept(this);
            this.SqlBuilder.Append(",");
            exp.ReplacementValue.Accept(this);
            this.SqlBuilder.Append(")");

            return exp;
        }

        public override DbExpression VisitConvert(DbConvertExpression exp)
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
                BuildCastState(this, exp.Operand, dbTypeString);
            }
            else
            {
                Type targetType = ReflectionExtension.GetUnderlyingType(exp.Type);
                if (targetType == PublicConstants.TypeOfDateTime)
                {
                    /* DATETIME('2016-08-06 09:01:24') */
                    this.SqlBuilder.Append("DATETIME(");
                    exp.Operand.Accept(this);
                    this.SqlBuilder.Append(")");
                }
                else
                    exp.Operand.Accept(this);
            }

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

        public override DbExpression VisitConstant(DbConstantExpression exp)
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
                this.SqlBuilder.Append("'", exp.Value, "'");
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
        public override DbExpression VisitParameter(DbParameterExpression exp)
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

            if (exp.SkipCount == null && exp.TakeCount == null)
                return;

            int skipCount = exp.SkipCount ?? 0;
            long takeCount = long.MaxValue;
            if (exp.TakeCount != null)
                takeCount = exp.TakeCount.Value;

            this.SqlBuilder.Append(" LIMIT ", takeCount.ToString(), " OFFSET ", skipCount.ToString());
        }

        protected override void AppendTable(DbTable table)
        {
            this.QuoteName(table.Name);
        }

    }
}
