using System.Data;

namespace Chloe.Entity
{
    public class PrimitivePropertyBuilder<TProperty, TEntity> : PrimitivePropertyBuilder, IPrimitivePropertyBuilder<TProperty, TEntity>
    {
        public PrimitivePropertyBuilder(PrimitiveProperty property, IEntityTypeBuilder<TEntity> declaringBuilder) : base(property, declaringBuilder)
        {

        }

        public new IEntityTypeBuilder<TEntity> DeclaringBuilder { get { return (IEntityTypeBuilder<TEntity>)base.DeclaringBuilder; } }


        IPrimitivePropertyBuilder AsNonGenericBuilder()
        {
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> MapTo(string column)
        {
            this.AsNonGenericBuilder().MapTo(column);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> HasAnnotation(object value)
        {
            this.AsNonGenericBuilder().HasAnnotation(value);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> IsPrimaryKey(bool isPrimaryKey = true)
        {
            this.AsNonGenericBuilder().IsPrimaryKey(isPrimaryKey);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> IsAutoIncrement(bool isAutoIncrement = true)
        {
            this.AsNonGenericBuilder().IsAutoIncrement(isAutoIncrement);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> IsNullable(bool isNullable = true)
        {
            this.AsNonGenericBuilder().IsNullable(isNullable);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> IsRowVersion(bool isRowVersion = true)
        {
            this.AsNonGenericBuilder().IsRowVersion(isRowVersion);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> IsUniqueIndex(bool isUniqueIndex = true)
        {
            this.AsNonGenericBuilder().IsUniqueIndex(isUniqueIndex);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> HasDbType(DbType dbType)
        {
            this.AsNonGenericBuilder().HasDbType(dbType);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> HasSize(int? size)
        {
            this.AsNonGenericBuilder().HasSize(size);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> HasScale(byte? scale)
        {
            this.AsNonGenericBuilder().HasScale(scale);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> HasPrecision(byte? precision)
        {
            this.AsNonGenericBuilder().HasPrecision(precision);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> HasSequence(string name, string schema)
        {
            this.AsNonGenericBuilder().HasSequence(name, schema);
            return this;
        }

        public new IPrimitivePropertyBuilder<TProperty, TEntity> UpdateIgnore(bool updateIgnore = true)
        {
            this.AsNonGenericBuilder().UpdateIgnore(updateIgnore);
            return this;
        }

    }
}
