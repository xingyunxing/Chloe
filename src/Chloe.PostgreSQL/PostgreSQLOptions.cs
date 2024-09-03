
namespace Chloe.PostgreSQL
{
    public class PostgreSQLOptions : DbOptions
    {
        public PostgreSQLOptions()
        {
            this.MaxNumberOfParameters = 32767;
            this.MaxInItems = 1000;
        }

        /// <summary>
        /// 是否将 sql 中的表名/字段名转成小写。默认为 true。
        /// </summary>
        public bool ConvertToLowercase { get; set; } = true;

        public PostgreSQLOptions Clone()
        {
            PostgreSQLOptions options = new PostgreSQLOptions()
            {
                DbConnectionFactory = this.DbConnectionFactory,
                InsertStrategy = this.InsertStrategy,
                MaxNumberOfParameters = this.MaxNumberOfParameters,
                MaxInItems = this.MaxInItems,
                DefaultBatchSizeForInsertRange = this.DefaultBatchSizeForInsertRange,
                RegardEmptyStringAsNull = this.RegardEmptyStringAsNull,
                ConvertToLowercase = this.ConvertToLowercase
            };

            return options;
        }
    }
}
