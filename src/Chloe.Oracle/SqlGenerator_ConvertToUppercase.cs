namespace Chloe.Oracle
{
    class SqlGenerator_ConvertToUppercase : SqlGenerator
    {
        public override void QuoteName(string name)
        {
            base.QuoteName(name.ToUpper());
        }
    }
}
