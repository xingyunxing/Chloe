using Chloe.RDBMS;

namespace Chloe.Oracle
{
    class SqlGenerator_ConvertToUppercase : SqlGenerator
    {
        public SqlGenerator_ConvertToUppercase(SqlGeneratorOptions options) : base(options)
        {

        }

        public override void QuoteName(string name)
        {
            base.QuoteName(name.ToUpper());
        }
    }
}
