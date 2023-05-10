using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Infrastructure;

namespace Chloe.SqlServer
{
    class DbExpressionTranslator : IDbExpressionTranslator
    {
        protected MsSqlContextProvider ContextProvider { get; set; }

        public DbExpressionTranslator(MsSqlContextProvider contextProvider)
        {
            this.ContextProvider = contextProvider;
        }

        public virtual DbCommandInfo Translate(DbExpression expression)
        {
            SqlGenerator generator = this.CreateSqlGenerator();
            expression = EvaluableDbExpressionTransformer.Transform(expression);
            expression.Accept(generator);

            DbCommandInfo dbCommandInfo = new DbCommandInfo();
            dbCommandInfo.Parameters = generator.Parameters;
            dbCommandInfo.CommandText = generator.SqlBuilder.ToSql();

            return dbCommandInfo;
        }
        public virtual SqlGenerator CreateSqlGenerator()
        {
            return new SqlGenerator(this.ContextProvider);
        }
    }

    class DbExpressionTranslator_OffsetFetch : DbExpressionTranslator
    {
        public DbExpressionTranslator_OffsetFetch(MsSqlContextProvider contextProvider) : base(contextProvider)
        {

        }

        public override SqlGenerator CreateSqlGenerator()
        {
            return new SqlGenerator_OffsetFetch(this.ContextProvider);
        }
    }
}
