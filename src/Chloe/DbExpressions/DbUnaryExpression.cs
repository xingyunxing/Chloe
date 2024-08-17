using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.DbExpressions
{
    public abstract class DbUnaryExpression : DbExpression
    {
        DbExpression _operand;

        protected DbUnaryExpression(DbExpressionType nodeType, Type type, DbExpression operand) : base(nodeType, type)
        {
            this._operand = operand;
        }

        public DbExpression Operand { get { return this._operand; } }
    }
}
