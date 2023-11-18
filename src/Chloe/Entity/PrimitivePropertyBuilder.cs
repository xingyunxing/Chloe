using System.Data;

namespace Chloe.Entity
{
    public class PrimitivePropertyBuilder : IPrimitivePropertyBuilder
    {
        public PrimitivePropertyBuilder(PrimitiveProperty property, IEntityTypeBuilder declaringBuilder)
        {
            this.Property = property;
            this.DeclaringBuilder = declaringBuilder;
        }

        public PrimitiveProperty Property { get; private set; }

        public IEntityTypeBuilder DeclaringBuilder { get; private set; }


        public IPrimitivePropertyBuilder MapTo(string column)
        {
            this.Property.ColumnName = column;
            return this;
        }

        public IPrimitivePropertyBuilder HasAnnotation(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            this.Property.Annotations.Add(value);
            return this;
        }

        public IPrimitivePropertyBuilder IsPrimaryKey(bool isPrimaryKey)
        {
            this.Property.IsPrimaryKey = isPrimaryKey;
            return this;
        }

        public IPrimitivePropertyBuilder IsAutoIncrement(bool isAutoIncrement)
        {
            this.Property.IsAutoIncrement = isAutoIncrement;
            if (isAutoIncrement)
            {
                this.Property.SequenceName = null;
                this.Property.SequenceSchema = null;
            }

            return this;
        }

        public IPrimitivePropertyBuilder IsNullable(bool isNullable)
        {
            this.Property.IsNullable = isNullable;
            return this;
        }

        public IPrimitivePropertyBuilder IsRowVersion(bool isRowVersion)
        {
            this.Property.IsRowVersion = isRowVersion;
            return this;
        }

        public IPrimitivePropertyBuilder IsUniqueIndex(bool isUniqueIndex)
        {
            this.Property.IsUniqueIndex = isUniqueIndex;
            return this;
        }

        public IPrimitivePropertyBuilder HasDbType(DbType dbType)
        {
            this.Property.DbType = dbType;
            return this;
        }

        public IPrimitivePropertyBuilder HasSize(int? size)
        {
            this.Property.Size = size;
            return this;
        }

        public IPrimitivePropertyBuilder HasScale(byte? scale)
        {
            this.Property.Scale = scale;
            return this;
        }

        public IPrimitivePropertyBuilder HasPrecision(byte? precision)
        {
            this.Property.Precision = precision;
            return this;
        }

        public IPrimitivePropertyBuilder HasSequence(string name, string schema)
        {
            this.Property.SequenceName = name;
            this.Property.SequenceSchema = schema;
            if (!string.IsNullOrEmpty(name))
            {
                this.Property.IsAutoIncrement = false;
            }

            return this;
        }

        public IPrimitivePropertyBuilder UpdateIgnore(bool updateIgnore)
        {
            this.Property.UpdateIgnore = updateIgnore;
            return this;
        }
    }
}
