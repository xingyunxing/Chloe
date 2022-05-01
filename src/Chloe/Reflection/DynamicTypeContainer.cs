using Chloe.Reflection.Emit;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Chloe.Reflection
{
    public class DynamicTypeContainer
    {
        static readonly Dictionary<string, DynamicType> Cache = new Dictionary<string, DynamicType>();
        public static DynamicType Get(List<Type> typeProperties)
        {
            string key = GenKey(typeProperties);
            DynamicType dynamicType = null;
            if (!Cache.TryGetValue(key, out dynamicType))
            {
                lock (Cache)
                {
                    if (!Cache.TryGetValue(key, out dynamicType))
                    {
                        Type type = ClassGenerator.CreateDynamicType(typeProperties);
                        var properties = type.GetProperties();

                        List<DynamicTypeProperty> dynamicTypeProperties = new List<DynamicTypeProperty>(properties.Length);
                        foreach (var property in properties)
                        {
                            DynamicTypeProperty dynamicTypeProperty = new DynamicTypeProperty();
                            dynamicTypeProperty.Property = property;
                            dynamicTypeProperty.Getter = MemberGetterContainer.Get(property);
                            dynamicTypeProperties.Add(dynamicTypeProperty);
                        }

                        dynamicType = new DynamicType() { Type = type };
                        dynamicType.Properties = dynamicTypeProperties.AsReadOnly();

                        Cache.Add(key, dynamicType);
                    }
                }
            }

            return dynamicType;
        }

        static string GenKey(List<Type> typeProperties)
        {
            string key = "";
            string c = "";
            foreach (Type type in typeProperties)
            {
                key = key + c + type.GetHashCode().ToString();
                c = "-";
            }

            return key;
        }
    }

    public class DynamicType
    {
        public Type Type { get; internal set; }
        public ReadOnlyCollection<DynamicTypeProperty> Properties { get; internal set; }
    }

    public class DynamicTypeProperty
    {
        public PropertyInfo Property { get; internal set; }
        public MemberGetter Getter { get; internal set; }
    }

    internal static class DynamicTypeExtension
    {
        public static MemberGetter GetPrimaryKeyGetter(this DynamicType dynamicType)
        {
            return dynamicType.Properties[0].Getter;
        }

        public static MemberGetter GetTableIndexGetter(this DynamicType dynamicType)
        {
            return dynamicType.Properties[1].Getter;
        }
    }
}
