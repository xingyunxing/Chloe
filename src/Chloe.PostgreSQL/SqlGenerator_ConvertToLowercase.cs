using Chloe.RDBMS;

namespace Chloe.PostgreSQL
{
    class SqlGenerator_ConvertToLowercase : SqlGenerator
    {
        public SqlGenerator_ConvertToLowercase(SqlGeneratorOptions options) : base(options)
        {

        }

        public override void QuoteName(string name)
        {
            base.QuoteName(name.ToLower());
        }
    }
}
