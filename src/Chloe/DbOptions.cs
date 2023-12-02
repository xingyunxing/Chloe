using Chloe.Infrastructure;

namespace Chloe
{
    public class DbOptions
    {
        public IDbConnectionFactory DbConnectionFactory { get; set; }

        /// <summary>
        /// in 条件参数最大个数
        /// </summary>
        public int MaxInItems { get; set; } = 1000;
    }
}
