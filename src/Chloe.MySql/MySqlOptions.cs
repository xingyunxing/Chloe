
namespace Chloe.MySql
{
    public class MySqlOptions : DbOptions
    {
        public MySqlOptions()
        {

        }

        public MySqlOptions Clone()
        {
            MySqlOptions options = new MySqlOptions()
            {
                DbConnectionFactory = this.DbConnectionFactory,
                InsertStrategy = this.InsertStrategy,
                MaxNumberOfParameters = this.MaxNumberOfParameters,
                MaxInItems = this.MaxInItems,
                DefaultBatchSizeForInsertRange = this.DefaultBatchSizeForInsertRange,
                TreatEmptyStringAsNull = this.TreatEmptyStringAsNull
            };

            return options;
        }
    }
}
