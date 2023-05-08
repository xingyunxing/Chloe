using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Infrastructure;

namespace Chloe.Dameng
{
    class DbExpressionTranslator : IDbExpressionTranslator
    {
        public static readonly DbExpressionTranslator Instance = new DbExpressionTranslator();
        public virtual DbCommandInfo Translate(DbExpression expression)
        {
            SqlGenerator generator = SqlGenerator.CreateInstance();
            expression = EvaluableDbExpressionTransformer.Transform(expression);
            expression.Accept(generator);

            var dbCommandInfo = new DbCommandInfo
            {
                Parameters = generator.Parameters,
                CommandText = generator.SqlBuilder.ToSql()
            };

            return dbCommandInfo;
        }
    }
}
