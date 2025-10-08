using Chloe.Descriptors;
using System.Collections;
using System.Reflection;

namespace Chloe.Core
{
    public interface IEntityState
    {
        object Entity { get; }
        TypeDescriptor TypeDescriptor { get; }
        bool HasChanged(PrimitivePropertyDescriptor propertyDescriptor, object val);
        void Refresh();
    }

    class EntityState : IEntityState
    {
        Dictionary<MemberInfo, object> _copies;
        object _entity;
        TypeDescriptor _typeDescriptor;

        public EntityState(TypeDescriptor typeDescriptor, object entity)
        {
            this._typeDescriptor = typeDescriptor;
            this._entity = entity;
            this.Refresh();
        }

        public object Entity { get { return this._entity; } }
        public TypeDescriptor TypeDescriptor { get { return this._typeDescriptor; } }

        public bool HasChanged(PrimitivePropertyDescriptor propertyDescriptor, object val)
        {
            object oldVal;
            if (!this._copies.TryGetValue(propertyDescriptor.Property, out oldVal))
            {
                return true;
            }

            if (propertyDescriptor.PropertyType == PublicConstants.TypeOfByteArray)
            {
                //byte[] is a big big hole~
                return !AreEqual((byte[])oldVal, (byte[])val);
            }

            return !object.Equals(oldVal, val);
        }
        public void Refresh()
        {
            if (this._copies == null)
            {
                this._copies = new Dictionary<MemberInfo, object>(this.TypeDescriptor.PrimitivePropertyDescriptors.Count);
            }
            else
            {
                this._copies.Clear();
            }

            object entity = this._entity;
            foreach (PrimitivePropertyDescriptor propertyDescriptor in this.TypeDescriptor.PrimitivePropertyDescriptors)
            {
                var val = propertyDescriptor.GetValue(entity);

                //I hate the byte[].
                if (propertyDescriptor.PropertyType == PublicConstants.TypeOfByteArray)
                {
                    val = Clone((byte[])val);
                }

                this._copies[propertyDescriptor.Definition.Property] = val;
            }
        }

        static byte[] Clone(byte[] arr)
        {
            if (arr == null)
                return null;

            return (byte[])arr.Clone();
        }
        static bool AreEqual(byte[] obj1, byte[] obj2)
        {
            if (obj1 == obj2)
                return true;

            if (obj1 == null || obj2 == null)
                return false;

            return (obj1 as IStructuralEquatable).Equals(obj2, StructuralComparisons.StructuralEqualityComparer);
        }
    }
}
