namespace Chloe.KingbaseES
{
    public class KingbaseESOptions : DbOptions
    {
        public KingbaseESOptions()
        {
        }

        /// <summary>
        /// 是否将 sql 中的表名/字段名转成小写。默认为 true。
        /// </summary>
        public bool ConvertToLowercase { get; set; } = true;

        public KingbaseESOptions Clone()
        {
            KingbaseESOptions options = new KingbaseESOptions()
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