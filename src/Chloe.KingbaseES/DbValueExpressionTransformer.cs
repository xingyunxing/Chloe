using Chloe.DbExpressions;

namespace Chloe.KingbaseES
{
    class DbValueExpressionTransformer : DbExpressionVisitor
    {
        static DbValueExpressionTransformer _transformer = new DbValueExpressionTransformer();

        public static DbExpression Transform(DbExpression exp)
        {
            return exp.Accept(_transformer);
        }

        DbExpression ConvertDbBooleanExpression(DbExpression exp)
        {
            DbCaseWhenExpression caseWhenExpression = PublicHelper.ConstructReturnCSharpBooleanCaseWhenExpression(exp);
            return caseWhenExpression;
        }

        public override DbExpression VisitEqual(DbEqualExpression exp)
        {
            return this.ConvertDbBooleanExpression(exp);
        }
        public override DbExpression VisitNotEqual(DbNotEqualExpression exp)
        {
            return this.ConvertDbBooleanExpression(exp);
        }
        public override DbExpression VisitNot(DbNotExpression exp)
        {
            return this.ConvertDbBooleanExpression(exp);
        }

        public override DbExpression VisitBitAnd(DbBitAndExpression exp)
        {
            return exp;
        }
        public override DbExpression VisitAnd(DbAndExpression exp)
        {
            return this.ConvertDbBooleanExpression(exp);
        }
        public override DbExpression VisitBitOr(DbBitOrExpression exp)
        {
            return exp;
        }
        public override DbExpression VisitOr(DbOrExpression exp)
        {
            return this.ConvertDbBooleanExpression(exp);
        }

        public override DbExpression VisitConvert(DbConvertExpression exp)
        {
            return exp;
        }
        // +
        public override DbExpression VisitAdd(DbAddExpression exp)
        {
            return exp;
        }
        // -
        public override DbExpression VisitSubtract(DbSubtractExpression exp)
        {
            return exp;
        }
        // *
        public override DbExpression VisitMultiply(DbMultiplyExpression exp)
        {
            return exp;
        }
        // /
        public override DbExpression VisitDivide(DbDivideExpression exp)
        {
            return exp;
        }
        // %
        public override DbExpression VisitModulo(DbModuloExpression exp)
        {
            return exp;
        }
        public override DbExpression VisitNegate(DbNegateExpression exp)
        {
            return exp;
        }
        // <
        public override DbExpression VisitLessThan(DbLessThanExpression exp)
        {
            return this.ConvertDbBooleanExpression(exp);
        }
        // <=
        public override DbExpression VisitLessThanOrEqual(DbLessThanOrEqualExpression exp)
        {
            return this.ConvertDbBooleanExpression(exp);
        }
        // >
        public override DbExpression VisitGreaterThan(DbGreaterThanExpression exp)
        {
            return this.ConvertDbBooleanExpression(exp);
        }
        // >=
        public override DbExpression VisitGreaterThanOrEqual(DbGreaterThanOrEqualExpression exp)
        {
            return this.ConvertDbBooleanExpression(exp);
        }

        public override DbExpression VisitConstant(DbConstantExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitCoalesce(DbCoalesceExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitCaseWhen(DbCaseWhenExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitTable(DbTableExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitColumnAccess(DbColumnAccessExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitMemberAccess(DbMemberAccessExpression exp)
        {
            return exp;
        }
        public override DbExpression VisitParameter(DbParameterExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitSubquery(DbSubqueryExpression exp)
        {
            return exp;
        }
        public override DbExpression VisitSqlQuery(DbSqlQueryExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitMethodCall(DbMethodCallExpression exp)
        {
            if (exp.Type == PublicConstants.TypeOfBoolean || exp.Type == PublicConstants.TypeOfBoolean_Nullable)
                return this.ConvertDbBooleanExpression(exp);
            else
                return exp;
        }

        public override DbExpression VisitFromTable(DbFromTableExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitJoinTable(DbJoinTableExpression exp)
        {
            return exp;
        }
        public override DbExpression VisitAggregate(DbAggregateExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitInsert(DbInsertExpression exp)
        {
            return exp;
        }
        public override DbExpression VisitUpdate(DbUpdateExpression exp)
        {
            return exp;
        }
        public override DbExpression VisitDelete(DbDeleteExpression exp)
        {
            return exp;
        }

        public override DbExpression VisitExists(DbExistsExpression exp)
        {
            return this.ConvertDbBooleanExpression(exp);
        }
    }
}
