namespace Chloe.PostgreSQL
{
    class SqlGenerator_ConvertToLowercase : SqlGenerator
    {
        public override void QuoteName(string name)
        {
            base.QuoteName(name.ToLower());
        }
    }
}
