using Chloe.DbExpressions;
using Chloe.Utility;

namespace Chloe.Query
{
    public class QueryModel
    {
        public QueryModel(ScopeParameterDictionary scopeParameters, StringSet scopeTables) : this(new QueryOptions(), scopeParameters, scopeTables)
        {

        }

        public QueryModel(QueryOptions options, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            this.Options = options;

            if (scopeTables == null)
                this.ScopeTables = new StringSet();
            else
                this.ScopeTables = scopeTables.Clone();

            if (scopeParameters == null)
                this.ScopeParameters = new ScopeParameterDictionary();
            else
                this.ScopeParameters = scopeParameters.Clone();
        }

        public QueryOptions Options { get; private set; }

        public IObjectModel ResultModel { get; set; }

        /// <summary>
        /// Orderings 是否是传承下来的
        /// </summary>
        public bool InheritOrderings { get; set; }

        public List<DbOrdering> Orderings { get; private set; } = new List<DbOrdering>();
        public List<DbExpression> GroupSegments { get; private set; } = new List<DbExpression>();
        public List<DbExpression> GlobalFilters { get; private set; } = new List<DbExpression>();
        public List<DbExpression> ContextFilters { get; private set; } = new List<DbExpression>();

        /// <summary>
        /// 如 takequery 了以后，则 table 的 Expression 类似 (select T.Id.. from User as T),Alias 则为新生成的
        /// </summary>
        public DbFromTableExpression FromTable { get; set; }
        public DbExpression Condition { get; set; }
        public DbExpression HavingCondition { get; set; }

        public StringSet ScopeTables { get; private set; }
        public ScopeParameterDictionary ScopeParameters { get; private set; }
        public void AppendCondition(DbExpression condition)
        {
            this.Condition = this.Condition.And(condition);
        }
        public void AppendHavingCondition(DbExpression condition)
        {
            if (this.HavingCondition == null)
                this.HavingCondition = condition;
            else
                this.HavingCondition = new DbAndExpression(this.HavingCondition, condition);
        }

        public DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression(0, this.GroupSegments.Count, this.Orderings.Count);

            sqlQuery.Table = this.FromTable;
            sqlQuery.Orderings.AppendRange(this.Orderings);
            sqlQuery.Condition = this.Condition;

            if (!this.Options.IgnoreFilters)
            {
                sqlQuery.Condition = this.ContextFilters.And().And(sqlQuery.Condition).And(this.GlobalFilters);
            }

            sqlQuery.GroupSegments.AppendRange(this.GroupSegments);
            sqlQuery.HavingCondition = this.HavingCondition;

            return sqlQuery;
        }
        public QueryModel Clone(bool includeOrderings = true)
        {
            QueryModel newQueryModel = new QueryModel(this.Options, this.ScopeParameters, this.ScopeTables);
            newQueryModel.FromTable = this.FromTable;

            newQueryModel.ResultModel = this.ResultModel;

            if (includeOrderings)
            {
                newQueryModel.Orderings.AppendRange(this.Orderings);
            }

            newQueryModel.Condition = this.Condition;

            newQueryModel.GlobalFilters.AppendRange(this.GlobalFilters);
            newQueryModel.ContextFilters.AppendRange(this.ContextFilters);
            newQueryModel.GroupSegments.AppendRange(this.GroupSegments);
            newQueryModel.AppendHavingCondition(this.HavingCondition);

            return newQueryModel;
        }

        public string GenerateUniqueTableAlias(string prefix = UtilConstants.DefaultTableAlias)
        {
            string alias = prefix;
            int i = 0;
            DbFromTableExpression fromTable = this.FromTable;
            while (this.ScopeTables.Contains(alias) || ExistTableAlias(fromTable, alias))
            {
                alias = prefix + i.ToString();
                i++;
            }

            this.ScopeTables.Add(alias);

            return alias;
        }

        static bool ExistTableAlias(DbMainTableExpression mainTable, string alias)
        {
            if (mainTable == null)
                return false;

            if (string.Equals(mainTable.Table.Alias, alias, StringComparison.OrdinalIgnoreCase))
                return true;

            foreach (DbJoinTableExpression joinTable in mainTable.JoinTables)
            {
                if (ExistTableAlias(joinTable, alias))
                    return true;
            }

            return false;
        }
    }
}
