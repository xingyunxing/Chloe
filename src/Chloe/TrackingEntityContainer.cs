using Chloe.Core;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.Reflection;

namespace Chloe
{
    internal class TrackingEntityContainer
    {
        Dictionary<Type, TrackEntityCollection> _entityCollections = new Dictionary<Type, TrackEntityCollection>();

        public void Add(object entity)
        {
            PublicHelper.CheckNull(entity);
            Type entityType = entity.GetType();

            if (ReflectionExtension.IsAnonymousType(entityType))
                return;

            TrackEntityCollection collection;
            if (!this._entityCollections.TryGetValue(entityType, out collection))
            {
                TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entityType);

                if (!typeDescriptor.HasPrimaryKey())
                    return;

                collection = new TrackEntityCollection(typeDescriptor);
                this._entityCollections.Add(entityType, collection);
            }

            collection.TryAddEntity(entity);
        }

        public virtual IEntityState GetEntityState(object entity)
        {
            PublicHelper.CheckNull(entity);
            Type entityType = entity.GetType();

            if (this._entityCollections == null)
                return null;

            TrackEntityCollection collection;
            if (!this._entityCollections.TryGetValue(entityType, out collection))
            {
                return null;
            }

            IEntityState ret = collection.TryGetEntityState(entity);
            return ret;
        }

        class TrackEntityCollection
        {
            public TrackEntityCollection(TypeDescriptor typeDescriptor)
            {
                this.TypeDescriptor = typeDescriptor;
                this.Entities = new Dictionary<object, IEntityState>(1);
            }
            public TypeDescriptor TypeDescriptor { get; private set; }
            public Dictionary<object, IEntityState> Entities { get; private set; }
            public bool TryAddEntity(object entity)
            {
                if (this.Entities.ContainsKey(entity))
                {
                    return false;
                }

                IEntityState entityState = new EntityState(this.TypeDescriptor, entity);
                this.Entities.Add(entity, entityState);

                return true;
            }
            public IEntityState TryGetEntityState(object entity)
            {
                IEntityState ret;
                if (!this.Entities.TryGetValue(entity, out ret))
                    ret = null;

                return ret;
            }
        }
    }
}
