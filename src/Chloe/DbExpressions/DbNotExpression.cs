
namespace Chloe.DbExpressions
{
    public class DbNotExpression : DbUnaryExpression
    {
        public DbNotExpression(DbExpression operand) : base(DbExpressionType.Not, PublicConstants.TypeOfBoolean, operand)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.VisitNot(this);
        }
    }

}
