using System.Reflection;

namespace Chloe.Entity
{
    public class ComplexProperty : PropertyBase
    {
        public ComplexProperty(PropertyInfo property) : base(property)
        {

        }

        /// <summary>
        /// 外键
        /// </summary>
        public string ForeignKey { get; set; }

        /// <summary>
        /// 关联到对端的属性。不设置则默认关联到对端的主键
        /// </summary>
        public string OtherSideKey { get; set; }

        public ComplexPropertyDefinition MakeDefinition()
        {
            ComplexPropertyDefinition definition = new ComplexPropertyDefinition(this.Property, this.Annotations, this.ForeignKey, this.OtherSideKey);
            return definition;
        }
    }
}
