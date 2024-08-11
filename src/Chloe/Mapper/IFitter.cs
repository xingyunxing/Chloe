using Chloe.Descriptors;
using Chloe.Query;
using System.Collections;
using System.Data;

namespace Chloe.Mapper
{
    /// <summary>
    /// 集合填充器
    /// </summary>
    public interface IFitter
    {
        void Prepare(IDataReader reader);
        ValueTask Fill(QueryContext queryContext, object obj, object owner, IDataReader reader, bool @async);
        IFitter Clone();
    }

    public class ComplexObjectFitter : IFitter
    {
        List<Tuple<PropertyDescriptor, IFitter>> _includings;

        public ComplexObjectFitter(List<Tuple<PropertyDescriptor, IFitter>> includings)
        {
            this._includings = includings;
        }

        public void Prepare(IDataReader reader)
        {
            for (int i = 0; i < this._includings.Count; i++)
            {
                var kv = this._includings[i];
                kv.Item2.Prepare(reader);
            }
        }

        public async ValueTask Fill(QueryContext queryContext, object entity, object owner, IDataReader reader, bool @async)
        {
            for (int i = 0; i < this._includings.Count; i++)
            {
                var kv = this._includings[i];

                var propertyValue = kv.Item1.GetValue(entity);
                if (propertyValue == null)
                    continue;

                await kv.Item2.Fill(queryContext, propertyValue, entity, reader, @async);
            }
        }

        public IFitter Clone()
        {
            List<Tuple<PropertyDescriptor, IFitter>> includings = new List<Tuple<PropertyDescriptor, IFitter>>(this._includings.Count);
            includings.AddRange(this._includings.Select(a => new Tuple<PropertyDescriptor, IFitter>(a.Item1, a.Item2.Clone())));
            ComplexObjectFitter complexObjectFitter = new ComplexObjectFitter(includings);
            return complexObjectFitter;
        }
    }

    public class CollectionObjectFitter : IFitter
    {
        IObjectActivator _elementActivator;
        IEntityKey _entityKey;
        IFitter _elementFitter;
        PropertyDescriptor _elementOwnerProperty;

        HashSet<object> _keySet = new HashSet<object>();
        object _collection;

        public CollectionObjectFitter(IObjectActivator elementActivator, IEntityKey entityKey, IFitter elementFitter, PropertyDescriptor elementOwnerProperty)
        {
            this._elementActivator = elementActivator;
            this._entityKey = entityKey;
            this._elementFitter = elementFitter;
            this._elementOwnerProperty = elementOwnerProperty;
        }

        public void Prepare(IDataReader reader)
        {
            this._elementActivator.Prepare(reader);
            this._elementFitter.Prepare(reader);
        }

        public async ValueTask Fill(QueryContext queryContext, object collection, object owner, IDataReader reader, bool @async)
        {
            if (this._collection != collection)
            {
                this._keySet.Clear();
                this._collection = collection;
            }

            IList entityContainer = collection as IList;

            object entity = null;

            var keyValue = this._entityKey.GetKeyValue(reader);
            if (!this._keySet.Contains(keyValue))
            {
                entity = await this._elementActivator.CreateInstance(queryContext, reader, @async);
                if (entity != null)
                {
                    if (this._elementOwnerProperty != null)
                    {
                        this._elementOwnerProperty.SetValue(entity, owner); //entity.XX = owner
                    }

                    entityContainer.Add(entity);
                    this._keySet.Add(keyValue);
                }
            }

            if (entityContainer.Count > 0)
            {
                entity = entityContainer[entityContainer.Count - 1];
                await this._elementFitter.Fill(queryContext, entity, null, reader, @async);
            }
        }

        public IFitter Clone()
        {
            CollectionObjectFitter collectionObjectFitter = new CollectionObjectFitter(this._elementActivator.Clone(), this._entityKey.Clone(), this._elementFitter.Clone(), this._elementOwnerProperty);
            return collectionObjectFitter;
        }
    }
}
