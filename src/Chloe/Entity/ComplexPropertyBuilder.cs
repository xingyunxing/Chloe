using System.Linq.Expressions;

namespace Chloe.Entity
{
    public class ComplexPropertyBuilder : IComplexPropertyBuilder
    {
        public ComplexPropertyBuilder(ComplexProperty property, IEntityTypeBuilder declaringBuilder)
        {
            this.Property = property;
            this.DeclaringBuilder = declaringBuilder;
        }

        public ComplexProperty Property { get; private set; }
        public IEntityTypeBuilder DeclaringBuilder { get; private set; }


        public IComplexPropertyBuilder WithForeignKey(string foreignKey)
        {
            this.Property.ForeignKey = foreignKey;
            return this;
        }

        public IComplexPropertyBuilder AssociateWithOtherSide(string otherSideKey)
        {
            this.Property.OtherSideKey = otherSideKey;
            return this;
        }
    }
}
