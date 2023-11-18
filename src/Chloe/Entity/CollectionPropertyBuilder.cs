namespace Chloe.Entity
{
    public class CollectionPropertyBuilder : ICollectionPropertyBuilder
    {
        public CollectionPropertyBuilder(CollectionProperty property, IEntityTypeBuilder declaringBuilder)
        {
            this.Property = property;
            this.DeclaringBuilder = declaringBuilder;
        }

        public CollectionProperty Property { get; private set; }
        public IEntityTypeBuilder DeclaringBuilder { get; private set; }
    }
}
