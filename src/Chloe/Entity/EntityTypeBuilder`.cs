using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Entity
{
    public class EntityTypeBuilder<TEntity> : EntityTypeBuilder, IEntityTypeBuilder<TEntity>, IEntityTypeBuilder
    {
        public EntityTypeBuilder() : base(typeof(TEntity))
        {

        }

        IEntityTypeBuilder AsNonGenericBuilder()
        {
            return this;
        }

        public new IEntityTypeBuilder<TEntity> MapTo(string table)
        {
            this.AsNonGenericBuilder().MapTo(table);
            return this;
        }

        public new IEntityTypeBuilder<TEntity> HasSchema(string schema)
        {
            this.AsNonGenericBuilder().HasSchema(schema);
            return this;
        }

        public new IEntityTypeBuilder<TEntity> HasAnnotation(object value)
        {
            this.AsNonGenericBuilder().HasAnnotation(value);
            return this;
        }

        public IEntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> property)
        {
            string propertyName = PropertyNameExtractor.Extract(property);
            this.Ignore(propertyName);
            return this;
        }

        public IEntityTypeBuilder<TEntity> HasQueryFilter(Expression<Func<TEntity, bool>> filter)
        {
            this.EntityType.Filters.Add(filter);
            return this;
        }

        public IPrimitivePropertyBuilder<TProperty, TEntity> Property<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            string propertyName = PropertyNameExtractor.Extract(property);

            PrimitiveProperty entityProperty = this.GetPrimitiveProperty(propertyName);
            IPrimitivePropertyBuilder<TProperty, TEntity> propertyBuilder = new PrimitivePropertyBuilder<TProperty, TEntity>(entityProperty, this);
            return propertyBuilder;
        }

        public IComplexPropertyBuilder<TProperty, TEntity> HasOne<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            string propertyName = PropertyNameExtractor.Extract(property);
            ComplexProperty complexProperty = this.GetComplexProperty(propertyName);

            IComplexPropertyBuilder<TProperty, TEntity> propertyBuilder = new ComplexPropertyBuilder<TProperty, TEntity>(complexProperty, this);
            return propertyBuilder;
        }
        public IComplexPropertyBuilder<TProperty, TEntity> HasOne<TProperty, TForeignKey>(Expression<Func<TEntity, TProperty>> property, Expression<Func<TEntity, TForeignKey>> foreignKey)
        {
            return this.HasOne(property).WithForeignKey(foreignKey);
        }

        public ICollectionPropertyBuilder<TProperty, TEntity> HasMany<TProperty>(Expression<Func<TEntity, TProperty>> property)
        {
            string propertyName = PropertyNameExtractor.Extract(property);
            CollectionProperty collectionProperty = this.GetCollectionProperty(propertyName);

            ICollectionPropertyBuilder<TProperty, TEntity> propertyBuilder = new CollectionPropertyBuilder<TProperty, TEntity>(collectionProperty, this);
            return propertyBuilder;
        }
    }
}
