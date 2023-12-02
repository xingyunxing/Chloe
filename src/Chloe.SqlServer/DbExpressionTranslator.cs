using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Infrastructure;

namespace Chloe.SqlServer
{
    class DbExpressionTranslator : IDbExpressionTranslator
    {
        MsSqlContextProvider ContextProvider { get; set; }

        public DbExpressionTranslator(MsSqlContextProvider contextProvider)
        {
            this.ContextProvider = contextProvider;
        }

        public DbCommandInfo Translate(DbExpression expression)
        {
            SqlServerSqlGeneratorOptions options = this.CreateOptions();
            SqlGenerator generator = new SqlGenerator(options);
            expression = EvaluableDbExpressionTransformer.Transform(expression);
            expression.Accept(generator);

            DbCommandInfo dbCommandInfo = new DbCommandInfo();
            dbCommandInfo.Parameters = generator.Parameters;
            dbCommandInfo.CommandText = generator.SqlBuilder.ToSql();

            return dbCommandInfo;
        }

        SqlServerSqlGeneratorOptions CreateOptions()
        {
            var options = new SqlServerSqlGeneratorOptions()
            {
                LeftQuoteChar = UtilConstants.LeftQuoteChar,
                RightQuoteChar = UtilConstants.RightQuoteChar,
                MaxInItems = UtilConstants.MaxInItems,
                PagingMode = this.ContextProvider.Options.PagingMode,
                BindParameterByName = this.ContextProvider.Options.BindParameterByName
            };

            return options;
        }
    }
}
