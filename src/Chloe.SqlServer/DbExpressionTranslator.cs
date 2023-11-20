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
            SqlServerSqlGeneratorOptions options = this.CreateOptions();
            return new SqlGenerator(options);
        }

        protected SqlServerSqlGeneratorOptions CreateOptions()
        {
            var options = new SqlServerSqlGeneratorOptions()
            {
                LeftQuoteChar = UtilConstants.LeftQuoteChar,
                RightQuoteChar = UtilConstants.RightQuoteChar,
                MaxInItems = UtilConstants.MaxInItems,
                BindParameterByName = this.ContextProvider.BindParameterByName
            };

            return options;
        }
    }

    class DbExpressionTranslator_OffsetFetch : DbExpressionTranslator
    {
        public DbExpressionTranslator_OffsetFetch(MsSqlContextProvider contextProvider) : base(contextProvider)
        {

        }

        public override SqlGenerator CreateSqlGenerator()
        {
            SqlServerSqlGeneratorOptions options = this.CreateOptions();
            return new SqlGenerator_OffsetFetch(options);
        }
    }
}
