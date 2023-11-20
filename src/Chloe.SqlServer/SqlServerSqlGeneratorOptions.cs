using Chloe.RDBMS;

namespace Chloe.SqlServer
{
    class SqlServerSqlGeneratorOptions : SqlGeneratorOptions
    {
        public SqlServerSqlGeneratorOptions()
        {

        }

        public PagingMode PagingMode { get; set; }
        public bool BindParameterByName { get; set; }
    }
}
