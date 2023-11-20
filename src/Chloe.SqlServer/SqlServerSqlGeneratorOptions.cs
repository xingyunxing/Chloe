using Chloe.RDBMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.SqlServer
{
    internal class SqlServerSqlGeneratorOptions : SqlGeneratorOptions
    {
        public SqlServerSqlGeneratorOptions()
        {

        }

        public bool BindParameterByName { get; set; }
    }
}
