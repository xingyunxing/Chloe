namespace Chloe.Annotations
{
    /// <summary>
    /// Marks a property as navigation property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NavigationAttribute : Attribute
    {
        public NavigationAttribute()
        {
        }

        public NavigationAttribute(string foreignKey)
        {
            this.ForeignKey = foreignKey;
        }

        public NavigationAttribute(string foreignKey, string otherSideKey)
        {
            this.ForeignKey = foreignKey;
            this.OtherSideKey = otherSideKey;
        }

        /// <summary>
        /// 外键
        /// </summary>
        public string ForeignKey { get; private set; }

        /// <summary>
        /// 关联到对端的属性。不设置则默认关联到对端的主键
        /// </summary>
        public string OtherSideKey { get; set; }
    }
}
