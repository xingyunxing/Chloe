namespace Chloe.DbExpressions
{
    public class DbExpressionEqualityComparer : IEqualityComparer<DbExpression>
    {
        public static DbExpressionEqualityComparer Instance { get; } = new DbExpressionEqualityComparer();

        public bool Equals(DbExpression x, DbExpression y)
        {
            return DbExpressionComparer.Instance.Compare(x, y);
        }

        public int GetHashCode(DbExpression obj)
        {
            if (obj == null)
            {
                return 0;
            }

            unchecked
            {
                HashCode hash = new HashCode();
                hash.Add(obj.NodeType);
                hash.Add(obj.Type);

                switch (obj.NodeType)
                {
                    case DbExpressionType.And:
                    case DbExpressionType.Or:
                    case DbExpressionType.Equal:
                    case DbExpressionType.NotEqual:
                    case DbExpressionType.LessThan:
                    case DbExpressionType.LessThanOrEqual:
                    case DbExpressionType.GreaterThan:
                    case DbExpressionType.GreaterThanOrEqual:
                    case DbExpressionType.Add:
                    case DbExpressionType.Subtract:
                    case DbExpressionType.Multiply:
                    case DbExpressionType.Divide:
                    case DbExpressionType.BitAnd:
                    case DbExpressionType.BitOr:
                    case DbExpressionType.Modulo:
                        AddBinaryExpressionToHash((DbBinaryExpression)obj);
                        break;
                    case DbExpressionType.Not:
                    case DbExpressionType.Negate:
                    case DbExpressionType.Convert:
                        hash.Add((obj as DbUnaryExpression).Operand, this);
                        break;
                    case DbExpressionType.Constant:
                        DbConstantExpression dbConstant = (DbConstantExpression)obj;
                        hash.Add(dbConstant.Value);
                        break;
                    case DbExpressionType.Coalesce:
                        DbCoalesceExpression dbCoalesce = (DbCoalesceExpression)obj;
                        hash.Add(dbCoalesce.CheckExpression, this);
                        hash.Add(dbCoalesce.ReplacementValue, this);
                        break;
                    case DbExpressionType.CaseWhen:
                        DbCaseWhenExpression dbCaseWhen = (DbCaseWhenExpression)obj;
                        for (int i = 0; i < dbCaseWhen.WhenThenPairs.Count; i++)
                        {
                            hash.Add(dbCaseWhen.WhenThenPairs[i].When, this);
                            hash.Add(dbCaseWhen.WhenThenPairs[i].Then, this);
                        }
                        hash.Add(dbCaseWhen.Else, this);
                        break;
                    case DbExpressionType.MemberAccess:
                        DbMemberAccessExpression dbMemberAccess = (DbMemberAccessExpression)obj;
                        hash.Add(dbMemberAccess.Expression, this);
                        hash.Add(dbMemberAccess.Member);
                        break;
                    case DbExpressionType.MethodCall:
                        DbMethodCallExpression dbMethodCall = (DbMethodCallExpression)obj;
                        hash.Add(dbMethodCall.Method);
                        hash.Add(dbMethodCall.Object, this);
                        AddExpressionsToHash(dbMethodCall.Arguments);
                        break;
                    case DbExpressionType.Table:
                        DbTableExpression dbTable = (DbTableExpression)obj;
                        AddDbTableToHash(dbTable.Table);
                        break;
                    case DbExpressionType.ColumnAccess:
                        DbColumnAccessExpression dbColumnAccess = (DbColumnAccessExpression)obj;
                        AddDbTableToHash(dbColumnAccess.Table);
                        AddDbColumnToHash(dbColumnAccess.Column);
                        break;
                    case DbExpressionType.Parameter:
                        DbParameterExpression dbParameter = (DbParameterExpression)obj;
                        hash.Add(dbParameter.Value);
                        hash.Add(dbParameter.DbType);
                        break;
                    case DbExpressionType.FromTable:
                        DbFromTableExpression dbFromTable = (DbFromTableExpression)obj;
                        AddDbMainTableExpressionToHash(dbFromTable);
                        break;
                    case DbExpressionType.JoinTable:
                        DbJoinTableExpression dbJoinTable = (DbJoinTableExpression)obj;

                        hash.Add(dbJoinTable.Condition, this);
                        hash.Add(dbJoinTable.JoinType);

                        AddDbMainTableExpressionToHash(dbJoinTable);

                        break;
                    case DbExpressionType.Aggregate:
                        DbAggregateExpression dbAggregate = (DbAggregateExpression)obj;
                        hash.Add(dbAggregate.Method);
                        AddExpressionsToHash(dbAggregate.Arguments);
                        break;
                    case DbExpressionType.SqlQuery:
                        DbSqlQueryExpression dbSqlQuery = (DbSqlQueryExpression)obj;
                        hash.Add(dbSqlQuery.IsDistinct);
                        hash.Add(dbSqlQuery.TakeCount);
                        hash.Add(dbSqlQuery.SkipCount);

                        for (int i = 0; i < dbSqlQuery.ColumnSegments.Count; i++)
                        {
                            hash.Add(dbSqlQuery.ColumnSegments[i].Body, this);
                            hash.Add(dbSqlQuery.ColumnSegments[i].Alias);
                        }

                        hash.Add(dbSqlQuery.Table, this);
                        hash.Add(dbSqlQuery.Condition, this);
                        AddExpressionsToHash(dbSqlQuery.GroupSegments);
                        hash.Add(dbSqlQuery.HavingCondition, this);

                        for (int i = 0; i < dbSqlQuery.Orderings.Count; i++)
                        {
                            hash.Add(dbSqlQuery.Orderings[i].Expression, this);
                            hash.Add(dbSqlQuery.Orderings[i].OrderType);

                        }
                        break;
                    case DbExpressionType.Subquery:
                        DbSubqueryExpression dbSubquery = (DbSubqueryExpression)obj;
                        hash.Add(dbSubquery.SqlQuery, this);
                        break;
                    case DbExpressionType.Insert:
                        DbInsertExpression dbInsert = (DbInsertExpression)obj;
                        AddDbTableToHash(dbInsert.Table);

                        for (int i = 0; i < dbInsert.InsertColumns.Count; i++)
                        {
                            AddDbColumnToHash(dbInsert.InsertColumns[i].Column);
                            hash.Add(dbInsert.InsertColumns[i].Value, this);
                        }

                        for (int i = 0; i < dbInsert.Returns.Count; i++)
                        {
                            AddDbColumnToHash(dbInsert.Returns[i]);
                        }

                        break;
                    case DbExpressionType.Update:
                        DbUpdateExpression dbUpdate = (DbUpdateExpression)obj;
                        AddDbTableToHash(dbUpdate.Table);

                        for (int i = 0; i < dbUpdate.UpdateColumns.Count; i++)
                        {
                            AddDbColumnToHash(dbUpdate.UpdateColumns[i].Column);
                            hash.Add(dbUpdate.UpdateColumns[i].Value, this);
                        }

                        for (int i = 0; i < dbUpdate.Returns.Count; i++)
                        {
                            AddDbColumnToHash(dbUpdate.Returns[i]);
                        }

                        hash.Add(dbUpdate.Condition, this);

                        break;
                    case DbExpressionType.Delete:
                        DbDeleteExpression dbDelete = (DbDeleteExpression)obj;
                        AddDbTableToHash(dbDelete.Table);
                        hash.Add(dbDelete.Condition, this);
                        break;
                    case DbExpressionType.Exists:
                        DbExistsExpression dbExists = (DbExistsExpression)obj;
                        hash.Add(dbExists.SqlQuery, this);
                        break;
                    default:
                        throw new NotSupportedException(obj.NodeType.ToString());
                }

                return hash.ToHashCode();

                void AddBinaryExpressionToHash(DbBinaryExpression exp)
                {
                    if (exp == null)
                    {
                        return;
                    }

                    hash.Add(exp.Left, this);
                    hash.Add(exp.Right, this);
                    hash.Add(exp.Method);
                }

                void AddDbMainTableExpressionToHash(DbMainTableExpression exp)
                {
                    hash.Add(exp.Table.Body, this);
                    hash.Add(exp.Table.Alias);
                    hash.Add(exp.Table.Lock);
                    for (int i = 0; i < exp.JoinTables.Count; i++)
                    {
                        hash.Add(exp.JoinTables[i], this);
                    }
                }

                void AddExpressionsToHash(IList<DbExpression> exps)
                {
                    for (int i = 0; i < exps.Count; i++)
                    {
                        hash.Add(exps[i], this);
                    }
                }

                void AddDbTableToHash(DbTable dbTable)
                {
                    hash.Add(DbTableEqualityComparer.Instance.GetHashCode(dbTable));
                }

                void AddDbColumnToHash(DbColumn dbColumn)
                {
                    hash.Add(DbColumnEqualityComparer.Instance.GetHashCode(dbColumn));
                }

            }
        }
    }
}
