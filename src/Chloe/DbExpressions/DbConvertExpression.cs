namespace Chloe.DbExpressions
{
    public class DbConvertExpression : DbUnaryExpression
    {
        public DbConvertExpression(Type type, DbExpression operand) : base(DbExpressionType.Convert, type, operand)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.VisitConvert(this);
        }
    }
}
