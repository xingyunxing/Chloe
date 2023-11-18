using System.Linq.Expressions;

namespace Chloe.Entity
{
    public class ComplexPropertyBuilder<TProperty, TEntity> : ComplexPropertyBuilder, IComplexPropertyBuilder<TProperty, TEntity>
    {
        public ComplexPropertyBuilder(ComplexProperty property, IEntityTypeBuilder<TEntity> declaringBuilder) : base(property, declaringBuilder)
        {

        }

        public new IEntityTypeBuilder<TEntity> DeclaringBuilder { get { return (IEntityTypeBuilder<TEntity>)base.DeclaringBuilder; } }

        IComplexPropertyBuilder AsNonGenericBuilder()
        {
            return this;
        }

        public new IComplexPropertyBuilder<TProperty, TEntity> WithForeignKey(string foreignKey)
        {
            this.AsNonGenericBuilder().WithForeignKey(foreignKey);
            return this;
        }
        public IComplexPropertyBuilder<TProperty, TEntity> WithForeignKey<TForeignKey>(Expression<Func<TEntity, TForeignKey>> foreignKey)
        {
            string propertyName = PropertyNameExtractor.Extract(foreignKey);
            return this.WithForeignKey(propertyName);
        }
    }
}
