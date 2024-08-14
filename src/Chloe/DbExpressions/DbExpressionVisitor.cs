namespace Chloe.DbExpressions
{
    public class DbExpressionVisitor : DbExpressionVisitor<DbExpression>
    {
        public override DbExpression VisitEqual(DbEqualExpression exp)
        {
            return new DbEqualExpression(this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        public override DbExpression VisitNotEqual(DbNotEqualExpression exp)
        {
            return new DbNotEqualExpression(this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        // +
        public override DbExpression VisitAdd(DbAddExpression exp)
        {
            return new DbAddExpression(exp.Type, this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        // -
        public override DbExpression VisitSubtract(DbSubtractExpression exp)
        {
            return new DbSubtractExpression(exp.Type, this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        // *
        public override DbExpression VisitMultiply(DbMultiplyExpression exp)
        {
            return new DbMultiplyExpression(exp.Type, this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        // /
        public override DbExpression VisitDivide(DbDivideExpression exp)
        {
            return new DbDivideExpression(exp.Type, this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        // %
        public override DbExpression VisitModulo(DbModuloExpression exp)
        {
            return new DbModuloExpression(exp.Type, this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }

        public override DbExpression VisitNegate(DbNegateExpression exp)
        {
            return new DbNegateExpression(exp.Type, this.MakeNewExpression(exp.Operand));
        }

        // <
        public override DbExpression VisitLessThan(DbLessThanExpression exp)
        {
            return new DbLessThanExpression(this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        // <=
        public override DbExpression VisitLessThanOrEqual(DbLessThanOrEqualExpression exp)
        {
            return new DbLessThanOrEqualExpression(this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        // >
        public override DbExpression VisitGreaterThan(DbGreaterThanExpression exp)
        {
            return new DbGreaterThanExpression(this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        // >=
        public override DbExpression VisitGreaterThanOrEqual(DbGreaterThanOrEqualExpression exp)
        {
            return new DbGreaterThanOrEqualExpression(this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        public override DbExpression VisitBitAnd(DbBitAndExpression exp)
        {
            return new DbBitAndExpression(exp.Type, this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right));
        }
        public override DbExpression VisitAnd(DbAndExpression exp)
        {
            return new DbAndExpression(this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        public override DbExpression VisitBitOr(DbBitOrExpression exp)
        {
            return new DbBitOrExpression(exp.Type, this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right));
        }
        public override DbExpression VisitOr(DbOrExpression exp)
        {
            return new DbOrExpression(this.MakeNewExpression(exp.Left), this.MakeNewExpression(exp.Right), exp.Method);
        }
        public override DbExpression VisitConstant(DbConstantExpression exp)
        {
            return exp;
        }
        public override DbExpression VisitMember(DbMemberExpression exp)
        {
            DbExpression body = this.MakeNewExpression(exp.Expression);
            if (body == exp.Expression)
            {
                return exp;
            }

            return new DbMemberExpression(exp.Member, body);
        }
        public override DbExpression VisitNot(DbNotExpression exp)
        {
            return new DbNotExpression(this.MakeNewExpression(exp.Operand));
        }
        public override DbExpression VisitConvert(DbConvertExpression exp)
        {
            return new DbConvertExpression(exp.Type, this.MakeNewExpression(exp.Operand));
        }
        public override DbExpression VisitCoalesce(DbCoalesceExpression exp)
        {
            return new DbCoalesceExpression(this.MakeNewExpression(exp.CheckExpression), this.MakeNewExpression(exp.ReplacementValue));
        }
        public override DbExpression VisitCaseWhen(DbCaseWhenExpression exp)
        {
            var whenThenPairs = exp.WhenThenPairs.Select(a => new DbCaseWhenExpression.WhenThenExpressionPair(this.MakeNewExpression(a.When), this.MakeNewExpression(a.Then))).ToList();
            return new DbCaseWhenExpression(exp.Type, whenThenPairs, this.MakeNewExpression(exp.Else));
        }
        public override DbExpression VisitMethodCall(DbMethodCallExpression exp)
        {
            List<DbExpression> arguments = new List<DbExpression>(exp.Arguments.Count);

            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                arguments.Add(this.MakeNewExpression(exp.Arguments[i]));
            }

            return new DbMethodCallExpression(this.MakeNewExpression(exp.Object), exp.Method, arguments);
        }

        public override DbExpression VisitTable(DbTableExpression exp)
        {
            return exp;
        }
        public override DbExpression VisitColumnAccess(DbColumnAccessExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitParameter(DbParameterExpression exp)
        {
            return new DbParameterExpression(exp.Value, exp.Type, exp.DbType);
        }
        public override DbExpression VisitSubQuery(DbSubQueryExpression exp)
        {
            return new DbSubQueryExpression((DbSqlQueryExpression)this.MakeNewExpression(exp.SqlQuery));
        }
        public override DbExpression VisitSqlQuery(DbSqlQueryExpression exp)
        {
            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression(exp.Type, exp.ColumnSegments.Count, exp.GroupSegments.Count, exp.Orderings.Count)
            {
                TakeCount = exp.TakeCount,
                SkipCount = exp.SkipCount,
                Table = (DbFromTableExpression)this.MakeNewExpression(exp.Table),
                Condition = this.MakeNewExpression(exp.Condition),
                HavingCondition = this.MakeNewExpression(exp.HavingCondition),
                IsDistinct = exp.IsDistinct
            };

            for (int i = 0; i < exp.ColumnSegments.Count; i++)
            {
                sqlQuery.ColumnSegments.Add(this.MakeColumnSegment(exp.ColumnSegments[i]));
            }

            for (int i = 0; i < exp.GroupSegments.Count; i++)
            {
                sqlQuery.GroupSegments.Add(this.MakeNewExpression(exp.GroupSegments[i]));
            }

            for (int i = 0; i < exp.Orderings.Count; i++)
            {
                sqlQuery.Orderings.Add(this.MakeOrdering(exp.Orderings[i]));
            }

            return sqlQuery;
        }
        public override DbExpression VisitFromTable(DbFromTableExpression exp)
        {
            DbFromTableExpression ret = new DbFromTableExpression(this.MakeTableSegment(exp.Table));
            for (int i = 0; i < exp.JoinTables.Count; i++)
            {
                ret.JoinTables.Add((DbJoinTableExpression)this.MakeNewExpression(exp.JoinTables[i]));
            }

            return ret;
        }
        public override DbExpression VisitJoinTable(DbJoinTableExpression exp)
        {
            DbJoinTableExpression ret = new DbJoinTableExpression(exp.JoinType, this.MakeTableSegment(exp.Table), this.MakeNewExpression(exp.Condition)); ;
            for (int i = 0; i < exp.JoinTables.Count; i++)
            {
                DbJoinTableExpression dbJoinTable = (DbJoinTableExpression)this.MakeNewExpression(exp.JoinTables[i]);
                dbJoinTable.AppendTo(ret);
            }

            return ret;
        }
        public override DbExpression VisitAggregate(DbAggregateExpression exp)
        {
            List<DbExpression> arguments = new List<DbExpression>(exp.Arguments.Count);

            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                arguments.Add(this.MakeNewExpression(exp.Arguments[i]));
            }

            return new DbAggregateExpression(exp.Type, exp.Method, arguments);
        }

        public override DbExpression VisitInsert(DbInsertExpression exp)
        {
            DbInsertExpression ret = new DbInsertExpression(exp.Table, exp.InsertColumns.Count, exp.Returns.Count);

            for (int i = 0; i < exp.InsertColumns.Count; i++)
            {
                ret.AppendInsertColumn(exp.InsertColumns[i].Column, this.MakeNewExpression(exp.InsertColumns[i].Value));
            }

            for (int i = 0; i < exp.Returns.Count; i++)
            {
                ret.Returns.Add(exp.Returns[i]);
            }

            return ret;
        }
        public override DbExpression VisitUpdate(DbUpdateExpression exp)
        {
            DbUpdateExpression ret = new DbUpdateExpression(exp.Table, this.MakeNewExpression(exp.Condition), exp.UpdateColumns.Count, exp.Returns.Count);

            for (int i = 0; i < exp.UpdateColumns.Count; i++)
            {
                ret.AppendUpdateColumn(exp.UpdateColumns[i].Column, this.MakeNewExpression(exp.UpdateColumns[i].Value));
            }

            for (int i = 0; i < exp.Returns.Count; i++)
            {
                ret.Returns.Add(exp.Returns[i]);
            }

            return ret;
        }
        public override DbExpression VisitDelete(DbDeleteExpression exp)
        {
            return new DbDeleteExpression(exp.Table, this.MakeNewExpression(exp.Condition));
        }

        public override DbExpression VisitExists(DbExistsExpression exp)
        {
            return new DbExistsExpression((DbSqlQueryExpression)this.MakeNewExpression(exp.SqlQuery));
        }

        DbExpression MakeNewExpression(DbExpression exp)
        {
            if (exp == null)
                return null;

            return exp.Accept(this);
        }

        DbTableSegment MakeTableSegment(DbTableSegment tableSegment)
        {
            DbExpression tableSegmentBody = this.MakeNewExpression(tableSegment.Body);
            if (tableSegmentBody == tableSegment.Body)
            {
                return tableSegment;
            }

            return new DbTableSegment(tableSegmentBody, tableSegment.Alias, tableSegment.Lock);
        }

        DbColumnSegment MakeColumnSegment(DbColumnSegment columnSegment)
        {
            DbExpression columnSegmentBody = this.MakeNewExpression(columnSegment.Body);
            if (columnSegmentBody == columnSegment.Body)
            {
                return columnSegment;
            }

            return new DbColumnSegment(columnSegmentBody, columnSegment.Alias);
        }

        DbOrdering MakeOrdering(DbOrdering ordering)
        {
            DbExpression orderingBody = this.MakeNewExpression(ordering.Expression);
            if (orderingBody == ordering.Expression)
            {
                return ordering;
            }

            return new DbOrdering(orderingBody, ordering.OrderType);
        }


    }
}
