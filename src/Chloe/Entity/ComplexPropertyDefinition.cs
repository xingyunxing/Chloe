using System.Reflection;

namespace Chloe.Entity
{
    public class ComplexPropertyDefinition : PropertyDefinition
    {
        public ComplexPropertyDefinition(PropertyInfo property, IList<object> annotations, string foreignKey, string otherSideKey) : base(property, annotations)
        {
            if (string.IsNullOrEmpty(foreignKey))
                throw new ArgumentException("'foreignKey' can not be null.");

            this.ForeignKey = foreignKey;
            this.OtherSideKey = otherSideKey;
        }
        public override TypeKind Kind { get { return TypeKind.Complex; } }

        /// <summary>
        /// 外键
        /// </summary>
        public string ForeignKey { get; private set; }

        /// <summary>
        /// 关联到对端的属性。不设置则默认关联到对端的主键
        /// </summary>
        public string OtherSideKey { get; private set; }
    }
}
