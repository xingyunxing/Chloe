
namespace Chloe.DbExpressions
{
    public abstract class DbExpression
    {
        DbExpressionType _nodeType;
        Type _type;

        protected DbExpression(DbExpressionType nodeType) : this(nodeType, PublicConstants.TypeOfVoid)
        {

        }

        protected DbExpression(DbExpressionType nodeType, Type type)
        {
            this._nodeType = nodeType;
            this._type = type;
        }

        public virtual DbExpressionType NodeType
        {
            get { return this._nodeType; }
        }

        public virtual Type Type
        {
            get { return this._type; }
        }

        public abstract T Accept<T>(DbExpressionVisitor<T> visitor);

    }
}
