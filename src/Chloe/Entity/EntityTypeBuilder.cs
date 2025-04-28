using Chloe.Annotations;
using Chloe.Infrastructure;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Entity
{
    public class EntityTypeBuilder : IEntityTypeBuilder
    {
        public EntityTypeBuilder(Type type, bool initializeWithAnnotationMapping)
        {
            this.EntityType = new EntityType(type);

            if (initializeWithAnnotationMapping)
            {
                this.ConfigureTableMapping();
                this.ConfigureColumnMapping();
                this.ConfigureNavigationProperty();
            }
        }

        public EntityType EntityType { get; private set; }

        public IEntityTypeBuilder MapTo(string table)
        {
            this.EntityType.TableName = table;
            return this;
        }

        public IEntityTypeBuilder HasSchema(string schema)
        {
            this.EntityType.SchemaName = schema;
            return this;
        }

        public IEntityTypeBuilder HasAnnotation(object value)
        {
            if (value == null)
                throw new ArgumentNullException();

            this.EntityType.Annotations.Add(value);
            return this;
        }

        public IEntityTypeBuilder Ignore(string property)
        {
            this.EntityType.PrimitiveProperties.RemoveAll(a => a.Property.Name == property);
            return this;
        }

        public IEntityTypeBuilder HasQueryFilter(LambdaExpression filter)
        {
            this.EntityType.Filters.Add(filter);
            return this;
        }

        public virtual IPrimitivePropertyBuilder Property(string property)
        {
            PrimitiveProperty entityProperty = this.GetPrimitiveProperty(property);

            IPrimitivePropertyBuilder propertyBuilder = new PrimitivePropertyBuilder(entityProperty, this);
            return propertyBuilder;
        }

        public virtual IComplexPropertyBuilder HasOne(string property)
        {
            ComplexProperty complexProperty = this.GetComplexProperty(property);

            IComplexPropertyBuilder propertyBuilder = new ComplexPropertyBuilder(complexProperty, this);
            return propertyBuilder;
        }
        public virtual IComplexPropertyBuilder HasOne(string property, string foreignKey)
        {
            return this.HasOne(property).WithForeignKey(foreignKey);
        }
        public virtual IComplexPropertyBuilder HasOne(string property, string foreignKey, string otherSideKey)
        {
            return this.HasOne(property).WithForeignKey(foreignKey).AssociateWithOtherSide(otherSideKey);
        }

        public virtual ICollectionPropertyBuilder HasMany(string property)
        {
            CollectionProperty collectionProperty = this.GetCollectionProperty(property);

            ICollectionPropertyBuilder propertyBuilder = new CollectionPropertyBuilder(collectionProperty, this);
            return propertyBuilder;
        }

        internal PrimitiveProperty GetPrimitiveProperty(string property)
        {
            PrimitiveProperty entityProperty = this.EntityType.PrimitiveProperties.FirstOrDefault(a => a.Property.Name == property);

            if (entityProperty == null)
                throw new ArgumentException($"The mapping property list doesn't contain property named '{property}'.");

            return entityProperty;
        }
        internal ComplexProperty GetComplexProperty(string property)
        {
            ComplexProperty complexProperty = this.EntityType.ComplexProperties.Where(a => a.Property.Name == property).FirstOrDefault();

            if (complexProperty == null)
            {
                PropertyInfo propertyInfo = this.GetProperty(property);
                complexProperty = new ComplexProperty(propertyInfo);
                this.EntityType.ComplexProperties.Add(complexProperty);
            }

            return complexProperty;
        }
        internal CollectionProperty GetCollectionProperty(string property)
        {
            CollectionProperty collectionProperty = this.EntityType.CollectionProperties.Where(a => a.Property.Name == property).FirstOrDefault();

            if (collectionProperty == null)
            {
                PropertyInfo propertyInfo = this.GetProperty(property);
                collectionProperty = new CollectionProperty(propertyInfo);
                this.EntityType.CollectionProperties.Add(collectionProperty);
            }

            return collectionProperty;
        }


        internal PropertyInfo GetProperty(string property)
        {
            PropertyInfo entityProperty = this.EntityType.Type.GetProperty(property);

            if (entityProperty == null)
                throw new ArgumentException($"Cannot find property named '{property}'.");

            return entityProperty;
        }


        void ConfigureTableMapping()
        {
            var entityAttributes = this.EntityType.Type.GetCustomAttributes();
            foreach (Attribute entityAttribute in entityAttributes)
            {
                this.EntityType.Annotations.Add(entityAttribute);

                TableAttribute tableAttribute = entityAttribute as TableAttribute;
                if (tableAttribute != null)
                {
                    if (!string.IsNullOrEmpty(tableAttribute.Name))
                        this.MapTo(tableAttribute.Name);

                    this.HasSchema(tableAttribute.Schema);
                }
            }
        }
        void ConfigureColumnMapping()
        {
            var propertyInfos = this.EntityType.PrimitiveProperties.Select(a => a.Property).ToList();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                IPrimitivePropertyBuilder propertyBuilder = this.Property(propertyInfo.Name);

                propertyBuilder.IsPrimaryKey(false);
                propertyBuilder.IsAutoIncrement(false);

                var propertyAttributes = propertyInfo.GetCustomAttributes();
                foreach (Attribute propertyAttribute in propertyAttributes)
                {
                    propertyBuilder.HasAnnotation(propertyAttribute);

                    if (propertyAttribute is ColumnAttribute)
                    {
                        ColumnAttribute columnAttribute = (ColumnAttribute)propertyAttribute;

                        if (!string.IsNullOrEmpty(columnAttribute.Name))
                            propertyBuilder.MapTo(columnAttribute.Name);

                        if (columnAttribute.HasDbType())
                            propertyBuilder.HasDbType(columnAttribute.DbType);

                        propertyBuilder.IsPrimaryKey(columnAttribute.IsPrimaryKey);
                        propertyBuilder.IsRowVersion(columnAttribute.IsRowVersion);
                        propertyBuilder.HasSize(columnAttribute.GetSize());
                        propertyBuilder.HasScale(columnAttribute.GetScale());
                        propertyBuilder.HasPrecision(columnAttribute.GetPrecision());

                        continue;
                    }

                    if (propertyAttribute is NotMappedAttribute)
                    {
                        this.Ignore(propertyInfo.Name);
                        continue;
                    }

                    if (propertyAttribute is NotNullAttribute)
                    {
                        propertyBuilder.IsNullable(false);
                        continue;
                    }

                    if (propertyAttribute is AutoIncrementAttribute)
                    {
                        propertyBuilder.IsAutoIncrement(true);
                        continue;
                    }

                    if (propertyAttribute is UniqueIndexAttribute)
                    {
                        propertyBuilder.IsUniqueIndex(true);
                        continue;
                    }

                    if (propertyAttribute is UpdateIgnoreAttribute)
                    {
                        propertyBuilder.UpdateIgnore(true);
                        continue;
                    }

                    SequenceAttribute sequenceAttribute = propertyAttribute as SequenceAttribute;
                    if (sequenceAttribute != null)
                    {
                        propertyBuilder.HasSequence(sequenceAttribute.Name, sequenceAttribute.Schema);
                    }
                }
            }

            List<PrimitiveProperty> primaryKeys = this.EntityType.PrimitiveProperties.Where(a => a.IsPrimaryKey).ToList();
            if (primaryKeys.Count == 0)
            {
                //如果没有定义任何主键，则从所有映射的属性中查找名为 id 的属性作为主键
                PrimitiveProperty idNameProperty = this.EntityType.PrimitiveProperties.Find(a => string.Equals(a.Property.Name, "id", StringComparison.OrdinalIgnoreCase) && !a.Property.IsDefined(typeof(ColumnAttribute)));

                if (idNameProperty != null)
                {
                    this.Property(idNameProperty.Property.Name).IsPrimaryKey();
                    primaryKeys.Add(idNameProperty);
                }
            }

            if (primaryKeys.Count == 1 && this.EntityType.PrimitiveProperties.Count(a => a.IsAutoIncrement) == 0)
            {
                /* 如果没有显示定义自增成员，并且主键只有 1 个，如果该主键满足一定条件，则默认其是自增列 */
                PrimitiveProperty primaryKey = primaryKeys[0];

                if (string.IsNullOrEmpty(primaryKey.SequenceName) && Utils.IsAutoIncrementType(primaryKey.Property.PropertyType) && !primaryKey.Property.IsDefined(typeof(NonAutoIncrementAttribute)))
                {
                    this.Property(primaryKey.Property.Name).IsAutoIncrement();
                }
            }
        }
        void ConfigureNavigationProperty()
        {
            PropertyInfo[] properties = this.EntityType.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(a => a.GetSetMethod() != null && a.GetGetMethod() != null).ToArray();

            foreach (PropertyInfo property in properties)
            {
                if (MappingTypeSystem.IsMappingType(property.PropertyType))
                    continue;

                NavigationAttribute navigationAttribute = property.GetCustomAttribute<NavigationAttribute>();

                if (navigationAttribute == null)
                    continue;

                if (this.IsSupportedCollectionType(property.PropertyType))
                {
                    this.EntityType.CollectionProperties.Add(new CollectionProperty(property));
                    continue;
                }

                ComplexProperty complexProperty = new ComplexProperty(property);
                complexProperty.ForeignKey = navigationAttribute.ForeignKey;
                complexProperty.OtherSideKey = navigationAttribute.OtherSideKey;
                this.EntityType.ComplexProperties.Add(complexProperty);
            }
        }

        bool IsSupportedCollectionType(Type type)
        {
            if (!type.IsGenericType)
                return false;

            type = type.GetGenericTypeDefinition();
            return type.IsAssignableFrom(typeof(List<>)) || type.IsAssignableFrom(typeof(Collection<>));
        }
    }
}
