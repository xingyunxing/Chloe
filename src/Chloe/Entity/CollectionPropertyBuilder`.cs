namespace Chloe.Entity
{
    public class CollectionPropertyBuilder<TProperty, TEntity> : CollectionPropertyBuilder, ICollectionPropertyBuilder<TProperty, TEntity>
    {
        public CollectionPropertyBuilder(CollectionProperty property, IEntityTypeBuilder<TEntity> declaringBuilder) : base(property, declaringBuilder)
        {

        }

        public new IEntityTypeBuilder<TEntity> DeclaringBuilder { get { return (IEntityTypeBuilder<TEntity>)base.DeclaringBuilder; } }
    }
}
