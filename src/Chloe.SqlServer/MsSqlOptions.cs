
namespace Chloe.SqlServer
{
    public class MsSqlOptions : DbOptions
    {
        public MsSqlOptions()
        {
            this.MaxNumberOfParameters = 2100;
            this.MaxInItems = 2100;
        }

        /// <summary>
        /// 分页模式。
        /// </summary>
        public PagingMode PagingMode { get; set; } = PagingMode.ROW_NUMBER;

        /// <summary>
        /// 设置参数绑定方式。有些驱动（如ODBC驱动）不支持命名参数，只支持参数占位符，严格要求实际参数的顺序和个数，参数化 sql 只能如：select * from person where id=? 等形式。
        /// 默认值为 true。
        /// </summary>
        public bool BindParameterByName { get; set; } = true;
    }
}
