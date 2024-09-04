
namespace Chloe.Dameng
{
    public class DamengOptions : DbOptions
    {
        public DamengOptions()
        {
            this.MaxNumberOfParameters = 32767;
            this.MaxInItems = 2048;
        }

        public DamengOptions Clone()
        {
            DamengOptions options = new DamengOptions()
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
