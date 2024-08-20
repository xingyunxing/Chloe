namespace Chloe.DbExpressions
{
    public class DbExpressionComparer
    {
        public static DbExpressionComparer Instance { get; } = new DbExpressionComparer();

        public bool Compare(DbExpression left, DbExpression right)
        {
            if (left == right)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (left.NodeType != right.NodeType)
            {
                return false;
            }

            if (left.Type != right.Type)
            {
                return false;
            }

            switch (left.NodeType)
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
                    return CompareBinary((DbBinaryExpression)left, (DbBinaryExpression)right);
                case DbExpressionType.Not:
                case DbExpressionType.Negate:
                case DbExpressionType.Convert:
                    return this.CompareUnary((DbUnaryExpression)left, (DbUnaryExpression)right);
                case DbExpressionType.Constant:
                    return this.CompareConstant((DbConstantExpression)left, (DbConstantExpression)right);
                case DbExpressionType.Coalesce:
                    return this.CompareCoalesce((DbCoalesceExpression)left, (DbCoalesceExpression)right);
                case DbExpressionType.CaseWhen:
                    return this.CompareCaseWhen((DbCaseWhenExpression)left, (DbCaseWhenExpression)right);
                case DbExpressionType.MemberAccess:
                    return this.CompareMemberAccess((DbMemberAccessExpression)left, (DbMemberAccessExpression)right);
                case DbExpressionType.MethodCall:
                    return this.CompareMethodCall((DbMethodCallExpression)left, (DbMethodCallExpression)right);
                case DbExpressionType.Table:
                    return this.CompareTable((DbTableExpression)left, (DbTableExpression)right);
                case DbExpressionType.ColumnAccess:
                    return this.CompareColumnAccess((DbColumnAccessExpression)left, (DbColumnAccessExpression)right);
                case DbExpressionType.Parameter:
                    return this.CompareParameter((DbParameterExpression)left, (DbParameterExpression)right);
                case DbExpressionType.FromTable:
                    return this.CompareFromTable((DbFromTableExpression)left, (DbFromTableExpression)right);
                case DbExpressionType.JoinTable:
                    return this.CompareJoinTable((DbJoinTableExpression)left, (DbJoinTableExpression)right);
                case DbExpressionType.Aggregate:
                    return this.CompareAggregate((DbAggregateExpression)left, (DbAggregateExpression)right);
                case DbExpressionType.SqlQuery:
                    return this.CompareSqlQuery((DbSqlQueryExpression)left, (DbSqlQueryExpression)right);
                case DbExpressionType.Subquery:
                    return this.CompareSubquery((DbSubqueryExpression)left, (DbSubqueryExpression)right);
                case DbExpressionType.Insert:
                    return this.CompareInsert((DbInsertExpression)left, (DbInsertExpression)right);
                case DbExpressionType.Update:
                    return this.CompareUpdate((DbUpdateExpression)left, (DbUpdateExpression)right);
                case DbExpressionType.Delete:
                    return this.CompareDelete((DbDeleteExpression)left, (DbDeleteExpression)right);
                case DbExpressionType.Exists:
                    return this.CompareExists((DbExistsExpression)left, (DbExistsExpression)right);
                default:
                    throw new NotSupportedException(left.NodeType.ToString());
            }
        }

        bool CompareBinary(DbBinaryExpression left, DbBinaryExpression right)
        {
            return object.Equals(left.Method, right.Method) && this.Compare(left.Left, right.Left) && this.Compare(left.Right, right.Right);
        }

        bool CompareUnary(DbUnaryExpression left, DbUnaryExpression right)
        {
            return this.Compare(left.Operand, right.Operand);
        }

        bool CompareConstant(DbConstantExpression left, DbConstantExpression right)
        {
            return object.Equals(left.Value, right.Value);
        }

        bool CompareCoalesce(DbCoalesceExpression left, DbCoalesceExpression right)
        {
            return this.Compare(left.CheckExpression, right.CheckExpression) && this.Compare(left.ReplacementValue, right.ReplacementValue);
        }

        bool CompareCaseWhen(DbCaseWhenExpression left, DbCaseWhenExpression right)
        {
            if (left.WhenThenPairs.Count != right.WhenThenPairs.Count)
                return false;

            for (int i = 0; i < left.WhenThenPairs.Count; i++)
            {
                if (!this.Compare(left.WhenThenPairs[i].When, right.WhenThenPairs[i].When))
                {
                    return false;
                }

                if (!this.Compare(left.WhenThenPairs[i].Then, right.WhenThenPairs[i].Then))
                {
                    return false;
                }

            }

            if (!this.Compare(left.Else, right.Else))
            {
                return false;
            }

            return true;
        }

        bool CompareMemberAccess(DbMemberAccessExpression left, DbMemberAccessExpression right)
        {
            if (!object.Equals(left.Member, right.Member))
                return false;

            return this.Compare(left.Expression, right.Expression);
        }

        bool CompareMethodCall(DbMethodCallExpression left, DbMethodCallExpression right)
        {
            if (!object.Equals(left.Method, right.Method))
                return false;

            if (!this.Compare(left.Object, right.Object))
                return false;

            return this.CompareExpressions(left.Arguments, right.Arguments);
        }

        bool CompareTable(DbTableExpression left, DbTableExpression right)
        {
            return this.CompareTable(left.Table, right.Table);
        }

        bool CompareColumnAccess(DbColumnAccessExpression left, DbColumnAccessExpression right)
        {
            return this.CompareTable(left.Table, right.Table) && this.CompareColumn(left.Column, right.Column);
        }

        bool CompareParameter(DbParameterExpression left, DbParameterExpression right)
        {
            return left.DbType == right.DbType && object.Equals(left.Value, right.Value);
        }

        bool CompareFromTable(DbFromTableExpression left, DbFromTableExpression right)
        {
            return this.CompareMainTable(left, right);
        }

        bool CompareJoinTable(DbJoinTableExpression left, DbJoinTableExpression right)
        {
            return left.JoinType == right.JoinType && this.Compare(left.Condition, right.Condition) && this.CompareMainTable(left, right);
        }

        bool CompareAggregate(DbAggregateExpression left, DbAggregateExpression right)
        {
            if (!object.Equals(left.Method, right.Method))
                return false;

            return this.CompareExpressions(left.Arguments, right.Arguments);
        }

        bool CompareSqlQuery(DbSqlQueryExpression left, DbSqlQueryExpression right)
        {
            if (left.IsDistinct != right.IsDistinct)
                return false;

            if (left.TakeCount != right.TakeCount)
                return false;

            if (left.SkipCount != right.SkipCount)
                return false;

            if (left.ColumnSegments.Count != right.ColumnSegments.Count)
                return false;

            for (int i = 0; i < left.ColumnSegments.Count; i++)
            {
                if (left.ColumnSegments[i].Alias != right.ColumnSegments[i].Alias)
                    return false;

                if (!this.Compare(left.ColumnSegments[i].Body, right.ColumnSegments[i].Body))
                    return false;
            }

            if (!this.Compare(left.Table, right.Table))
                return false;

            if (!this.Compare(left.Condition, right.Condition))
                return false;

            if (!this.CompareExpressions(left.GroupSegments, right.GroupSegments))
                return false;

            if (!this.Compare(left.HavingCondition, right.HavingCondition))
                return false;

            if (left.Orderings.Count != right.Orderings.Count)
                return false;

            for (int i = 0; i < left.Orderings.Count; i++)
            {
                if (left.Orderings[i].OrderType != right.Orderings[i].OrderType)
                    return false;

                if (!this.Compare(left.Orderings[i].Expression, right.Orderings[i].Expression))
                    return false;
            }

            return true;
        }

        bool CompareSubquery(DbSubqueryExpression left, DbSubqueryExpression right)
        {
            return this.Compare(left.SqlQuery, right.SqlQuery);
        }

        bool CompareInsert(DbInsertExpression left, DbInsertExpression right)
        {
            if (!this.CompareTable(left.Table, right.Table))
                return false;

            if (left.InsertColumns.Count != right.InsertColumns.Count)
                return false;

            for (int i = 0; i < left.InsertColumns.Count; i++)
            {
                if (!this.CompareColumn(left.InsertColumns[i].Column, right.InsertColumns[i].Column))
                    return false;

                if (!this.Compare(left.InsertColumns[i].Value, right.InsertColumns[i].Value))
                    return false;
            }

            if (left.Returns.Count != right.Returns.Count)
                return false;

            for (int i = 0; i < left.Returns.Count; i++)
            {
                if (!this.CompareColumn(left.Returns[i], right.Returns[i]))
                    return false;
            }

            return true;
        }

        bool CompareUpdate(DbUpdateExpression left, DbUpdateExpression right)
        {
            if (!this.CompareTable(left.Table, right.Table))
                return false;

            if (left.UpdateColumns.Count != right.UpdateColumns.Count)
                return false;

            for (int i = 0; i < left.UpdateColumns.Count; i++)
            {
                if (!this.CompareColumn(left.UpdateColumns[i].Column, right.UpdateColumns[i].Column))
                    return false;

                if (!this.Compare(left.UpdateColumns[i].Value, right.UpdateColumns[i].Value))
                    return false;
            }

            if (left.Returns.Count != right.Returns.Count)
                return false;

            for (int i = 0; i < left.Returns.Count; i++)
            {
                if (!this.CompareColumn(left.Returns[i], right.Returns[i]))
                    return false;
            }

            return this.Compare(left.Condition, right.Condition);
        }

        bool CompareDelete(DbDeleteExpression left, DbDeleteExpression right)
        {
            return this.CompareTable(left.Table, right.Table) && this.Compare(left.Condition, right.Condition);
        }

        bool CompareExists(DbExistsExpression left, DbExistsExpression right)
        {
            return this.Compare(left.SqlQuery, right.SqlQuery);
        }


        bool CompareMainTable(DbMainTableExpression left, DbMainTableExpression right)
        {
            if (!this.CompareTableSegment(left.Table, right.Table))
                return false;

            if (left.JoinTables.Count != right.JoinTables.Count)
                return false;

            for (int i = 0; i < left.JoinTables.Count; i++)
            {
                if (this.Compare(left.JoinTables[i], right.JoinTables[i]))
                {
                    return false;
                }
            }

            return true;
        }

        bool CompareTableSegment(DbTableSegment left, DbTableSegment right)
        {
            return left.Alias == right.Alias && left.Lock == right.Lock && this.Compare(left.Body, right.Body);
        }

        bool CompareExpressions(IList<DbExpression> left, IList<DbExpression> right)
        {
            if (left.Count != right.Count)
                return false;

            for (int i = 0; i < left.Count; i++)
            {
                if (this.Compare(left[i], right[i]))
                {
                    return false;
                }
            }

            return true;
        }

        bool CompareTable(DbTable left, DbTable right)
        {
            return DbTableEqualityComparer.Instance.Equals(left, right);
        }

        bool CompareColumn(DbColumn left, DbColumn right)
        {
            return DbColumnEqualityComparer.Instance.Equals(left, right);
        }

    }
}
