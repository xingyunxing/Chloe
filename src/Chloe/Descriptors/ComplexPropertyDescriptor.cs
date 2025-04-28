using Chloe.Entity;
using Chloe.Exceptions;
using System.Reflection;

namespace Chloe.Descriptors
{
    public class ComplexPropertyDescriptor : PropertyDescriptor
    {
        public ComplexPropertyDescriptor(ComplexPropertyDefinition definition, TypeDescriptor declaringTypeDescriptor) : base(definition, declaringTypeDescriptor)
        {
            this.Definition = definition;

            PrimitivePropertyDescriptor foreignKeyProperty = declaringTypeDescriptor.PrimitivePropertyDescriptors.Where(a => a.Property.Name == definition.ForeignKey).FirstOrDefault();

            if (foreignKeyProperty == null)
                throw new ChloeException($"Can not find property named '{definition.ForeignKey}'");

            this.ForeignKeyProperty = foreignKeyProperty;

            if (!string.IsNullOrEmpty(definition.OtherSideKey))
            {
                PropertyInfo otherSideProperty = definition.Property.DeclaringType.GetProperty(definition.OtherSideKey);
                this.OtherSideProperty = otherSideProperty;
            }
        }

        public new ComplexPropertyDefinition Definition { get; private set; }

        /// <summary>
        /// 外键
        /// </summary>
        public PrimitivePropertyDescriptor ForeignKeyProperty { get; private set; }

        /// <summary>
        /// 关联到对端的属性
        /// </summary>
        public PropertyInfo OtherSideProperty { get; private set; }

        /// <summary>
        /// 查找关联的属性
        /// </summary>
        /// <param name="otherSideTypeDescriptor"></param>
        /// <returns></returns>
        /// <exception cref="ChloeException"></exception>
        public PropertyInfo GetOtherSideProperty(TypeDescriptor otherSideTypeDescriptor)
        {
            if (this.OtherSideProperty != null)
                return this.OtherSideProperty;

            PropertyInfo associatedKeyProperty = otherSideTypeDescriptor.PrimaryKeys[0].Property;
            return associatedKeyProperty;
        }

        /// <summary>
        /// 查找关联的属性
        /// </summary>
        /// <param name="otherSideTypeDescriptor"></param>
        /// <returns></returns>
        /// <exception cref="ChloeException"></exception>
        public PrimitivePropertyDescriptor GetOtherSidePropertyDescriptor(TypeDescriptor otherSideTypeDescriptor)
        {
            PrimitivePropertyDescriptor associatedKeyProperty = null;
            if (this.OtherSideProperty == null)
            {
                //取主键
                associatedKeyProperty = otherSideTypeDescriptor.PrimaryKeys[0];
            }
            else
            {
                //如果指定了关联的属性，则从 owner 中查找关联的属性值
                associatedKeyProperty = otherSideTypeDescriptor.GetPrimitivePropertyDescriptor(this.OtherSideProperty);
                if (associatedKeyProperty == null)
                {
                    throw new ChloeException($"Cannot find associated key '{this.OtherSideProperty.Name}' from type '{otherSideTypeDescriptor.EntityType.FullName}'");
                }
            }

            return associatedKeyProperty;
        }
    }
}
