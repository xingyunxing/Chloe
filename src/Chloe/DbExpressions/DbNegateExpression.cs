namespace Chloe.DbExpressions
{
    public class DbNegateExpression : DbUnaryExpression
    {
        public DbNegateExpression(Type type, DbExpression operand) : base(DbExpressionType.Negate, type, operand)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.VisitNegate(this);
        }
    }
}
