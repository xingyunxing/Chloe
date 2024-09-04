
namespace Chloe.Oracle
{
    public class OracleOptions : DbOptions
    {
        public OracleOptions()
        {
            this.MaxNumberOfParameters = 32767;
            this.MaxInItems = 1000;
            this.TreatEmptyStringAsNull = true;
        }

        /// <summary>
        /// 是否将 sql 中的表名/字段名转成大写。默认为 true。
        /// </summary>
        public bool ConvertToUppercase { get; set; } = true;

        public OracleOptions Clone()
        {
            OracleOptions options = new OracleOptions()
            {
                DbConnectionFactory = this.DbConnectionFactory,
                InsertStrategy = this.InsertStrategy,
                MaxNumberOfParameters = this.MaxNumberOfParameters,
                MaxInItems = this.MaxInItems,
                DefaultBatchSizeForInsertRange = this.DefaultBatchSizeForInsertRange,
                TreatEmptyStringAsNull = this.TreatEmptyStringAsNull,
                ConvertToUppercase = this.ConvertToUppercase
            };

            return options;
        }
    }
}
