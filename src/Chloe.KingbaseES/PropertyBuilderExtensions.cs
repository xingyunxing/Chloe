using Chloe.Entity;

namespace Chloe.KingbaseES
{
    public static class PrimitivePropertyBuilderExtension
    {
        public static IPrimitivePropertyBuilder HasKingbaseESDataType(this IPrimitivePropertyBuilder propertyBuilder, string name)
        {
            DataTypeAttribute dataTypeAttribute = new DataTypeAttribute(name);
            propertyBuilder.HasAnnotation(dataTypeAttribute);
            return propertyBuilder;
        }

        public static IPrimitivePropertyBuilder<TProperty, TEntity> HasKingbaseESDataType<TProperty, TEntity>(this IPrimitivePropertyBuilder<TProperty, TEntity> propertyBuilder, string name)
        {
            (propertyBuilder as IPrimitivePropertyBuilder).HasKingbaseESDataType(name);
            return propertyBuilder;
        }
    }
}
