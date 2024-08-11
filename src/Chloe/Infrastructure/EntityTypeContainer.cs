using Chloe.Descriptors;
using Chloe.Entity;

namespace Chloe.Infrastructure
{
    public class EntityTypeContainer
    {
        static readonly Dictionary<Type, TypeDescriptor> Cache = new Dictionary<Type, TypeDescriptor>();

        public static TypeDescriptor GetDescriptor(Type type)
        {
            PublicHelper.CheckNull(type, nameof(type));

            TypeDescriptor typeDescriptor;
            if (!Cache.TryGetValue(type, out typeDescriptor))
            {
                EntityTypeBuilder entityTypeBuilder = new EntityTypeBuilder(type, true);
                TypeDefinition typeDefinition = entityTypeBuilder.EntityType.MakeDefinition();
                lock (Cache)
                {
                    if (!Cache.TryGetValue(type, out typeDescriptor))
                    {
                        typeDescriptor = new TypeDescriptor(typeDefinition);
                        Cache.Add(type, typeDescriptor);
                    }
                }
            }

            return typeDescriptor;
        }

        public static TypeDescriptor TryGetDescriptor(Type type)
        {
            PublicHelper.CheckNull(type, nameof(type));

            TypeDescriptor typeDescriptor;
            Cache.TryGetValue(type, out typeDescriptor);

            return typeDescriptor;
        }

        /// <summary>
        /// Fluent Mapping
        /// </summary>
        /// <param name="entityTypeBuilders"></param>
        public static void UseBuilders(params IEntityTypeBuilder[] entityTypeBuilders)
        {
            if (entityTypeBuilders == null)
                return;

            Configure(entityTypeBuilders.Select(a => a.EntityType.MakeDefinition()).ToArray());
        }
        /// <summary>
        /// Fluent Mapping
        /// </summary>
        /// <param name="entityTypeBuilderTypes"></param>
        public static void UseBuilders(params Type[] entityTypeBuilderTypes)
        {
            if (entityTypeBuilderTypes == null)
                return;

            List<TypeDefinition> typeDefinitions = new List<TypeDefinition>(entityTypeBuilderTypes.Length);

            foreach (Type entityTypeBuilderType in entityTypeBuilderTypes)
            {
                IEntityTypeBuilder entityTypeBuilder = Activator.CreateInstance(entityTypeBuilderType) as IEntityTypeBuilder;
                typeDefinitions.Add(entityTypeBuilder.EntityType.MakeDefinition());
            }

            Configure(typeDefinitions.ToArray());
        }
        public static void Configure(params TypeDefinition[] typeDefinitions)
        {
            if (typeDefinitions == null)
                return;

            lock (Cache)
            {
                foreach (var typeDefinition in typeDefinitions)
                {
                    Cache[typeDefinition.Type] = new TypeDescriptor(typeDefinition);
                }
            }
        }

        public static Type[] GetRegisteredTypes()
        {
            return Cache.Keys.ToArray();
        }
        public static TypeDescriptor[] GetRegisteredTypeDescriptors()
        {
            return Cache.Values.ToArray();
        }
    }
}
