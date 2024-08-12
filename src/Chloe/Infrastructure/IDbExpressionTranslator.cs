using Chloe.Core;
using Chloe.DbExpressions;

namespace Chloe.Infrastructure
{
    public interface IDbExpressionTranslator
    {
        /// <summary>
        /// 将 expression 翻译成 sql
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="variables">表示 expression 中插槽对应的变量。如果 expression 中未包含插槽，可传 null</param>
        /// <returns></returns>
        DbCommandInfo Translate(DbExpression expression, List<object>? variables = null);
    }
}
