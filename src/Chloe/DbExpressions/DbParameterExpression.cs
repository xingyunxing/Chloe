using System.Data;

namespace Chloe.DbExpressions
{
    [System.Diagnostics.DebuggerDisplay("Value = {Value}")]
    public class DbParameterExpression : DbExpression
    {
        Type _type;

        public DbParameterExpression(object value) : base(DbExpressionType.Parameter)
        {
            this.Value = value;

            if (value != null)
                this._type = value.GetType();
            else
                this._type = PublicConstants.TypeOfObject;
        }

        public DbParameterExpression(object value, Type type) : this(value, type, null)
        {

        }

        public DbParameterExpression(object value, Type type, DbType? dbType) : base(DbExpressionType.Parameter)
        {
            PublicHelper.CheckNull(type);

            if (value != null)
            {
                Type t = value.GetType();

                if (!type.IsAssignableFrom(t))
                    throw new ArgumentException();
            }

            this.Value = value;
            this._type = type;
            this.DbType = dbType;
        }

        public override Type Type { get { return this._type; } }
        public object Value { get; private set; }
        public DbType? DbType { get; private set; }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.VisitParameter(this);
        }
    }
}
