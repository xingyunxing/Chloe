namespace Chloe.DbExpressions
{
    public abstract class DbExpressionVisitor<T>
    {
        public virtual T VisitEqual(DbEqualExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitNotEqual(DbNotEqualExpression exp)
        {
            throw new NotImplementedException();
        }
        // +
        public virtual T VisitAdd(DbAddExpression exp)
        {
            throw new NotImplementedException();
        }
        // -
        public virtual T VisitSubtract(DbSubtractExpression exp)
        {
            throw new NotImplementedException();
        }
        // *
        public virtual T VisitMultiply(DbMultiplyExpression exp)
        {
            throw new NotImplementedException();
        }
        // /
        public virtual T VisitDivide(DbDivideExpression exp)
        {
            throw new NotImplementedException();
        }
        // %
        public virtual T VisitModulo(DbModuloExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitNegate(DbNegateExpression exp)
        {
            throw new NotImplementedException();
        }

        // <
        public virtual T VisitLessThan(DbLessThanExpression exp)
        {
            throw new NotImplementedException();
        }
        // <=
        public virtual T VisitLessThanOrEqual(DbLessThanOrEqualExpression exp)
        {
            throw new NotImplementedException();
        }
        // >
        public virtual T VisitGreaterThan(DbGreaterThanExpression exp)
        {
            throw new NotImplementedException();
        }
        // >=
        public virtual T VisitGreaterThanOrEqual(DbGreaterThanOrEqualExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitBitAnd(DbBitAndExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitAnd(DbAndExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitBitOr(DbBitOrExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitOr(DbOrExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitConstant(DbConstantExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitMemberAccess(DbMemberAccessExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitNot(DbNotExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitConvert(DbConvertExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitCoalesce(DbCoalesceExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitCaseWhen(DbCaseWhenExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitMethodCall(DbMethodCallExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitTable(DbTableExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitColumnAccess(DbColumnAccessExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitParameter(DbParameterExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitSubQuery(DbSubQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitSqlQuery(DbSqlQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitFromTable(DbFromTableExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitJoinTable(DbJoinTableExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitAggregate(DbAggregateExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitInsert(DbInsertExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitUpdate(DbUpdateExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T VisitDelete(DbDeleteExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T VisitExists(DbExistsExpression exp)
        {
            throw new NotImplementedException();
        }
    }
}
