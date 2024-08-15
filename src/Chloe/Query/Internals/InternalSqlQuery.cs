using Chloe.Annotations;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.Mapper;
using Chloe.Mapper.Activators;
using Chloe.Mapper.Binders;
using Chloe.Query.Mapping;
using Chloe.Reflection;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Threading;

namespace Chloe.Query.Internals
{
    class InternalSqlQuery<T> : IEnumerable<T>, IAsyncEnumerable<T>
    {
        QueryContext _queryContext;
        string _sql;
        CommandType _cmdType;
        DbParam[] _parameters;

        public InternalSqlQuery(QueryContext queryContext, string sql, CommandType cmdType, DbParam[] parameters)
        {
            this._queryContext = queryContext;
            this._sql = sql;
            this._cmdType = cmdType;
            this._parameters = parameters;
        }

        public IEnumerable<T> AsIEnumerable()
        {
            return this;
        }
        public IAsyncEnumerable<T> AsIAsyncEnumerable()
        {
            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new QueryEnumerator<T>(this._queryContext, this.ExecuteReader, this.CreateObjectActivator, CancellationToken.None);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            IAsyncEnumerator<T> enumerator = this.GetEnumerator() as IAsyncEnumerator<T>;
            return enumerator;
        }

        IObjectActivator CreateObjectActivator(IDataReader dataReader)
        {
            Type type = typeof(T);

            if (type != PublicConstants.TypeOfObject && MappingTypeSystem.IsMappingType(type))
            {
                PrimitiveObjectActivatorCreator activatorCreator = new PrimitiveObjectActivatorCreator(type, 0);
                return activatorCreator.CreateObjectActivator(false);
            }

            return GetObjectActivator(type, dataReader);
        }

        async Task<IDataReader> ExecuteReader(bool @async)
        {
            IDataReader reader = await this._queryContext.DbContextProvider.AdoSession.ExecuteReader(this._sql, this._parameters, this._cmdType, @async);
            return reader;
        }

        static IObjectActivator GetObjectActivator(Type type, IDataReader reader)
        {
            if (type == PublicConstants.TypeOfObject || type == typeof(DapperRow))
            {
                return new DapperRowObjectActivator();
            }

            List<CacheInfo> caches;
            if (!ObjectActivatorCache.TryGetValue(type, out caches))
            {
                if (!Monitor.TryEnter(type))
                {
                    return CreateObjectActivator(type, reader);
                }

                try
                {
                    caches = ObjectActivatorCache.GetOrAdd(type, new List<CacheInfo>(1));
                }
                finally
                {
                    Monitor.Exit(type);
                }
            }

            CacheInfo cache = TryGetCacheInfoFromList(caches, reader);

            if (cache == null)
            {
                lock (caches)
                {
                    cache = TryGetCacheInfoFromList(caches, reader);
                    if (cache == null)
                    {
                        ComplexObjectActivator activator = CreateObjectActivator(type, reader);
                        cache = new CacheInfo(activator, reader);
                        caches.Add(cache);
                    }
                }
            }

            return cache.ObjectActivator;
        }
        static ComplexObjectActivator CreateObjectActivator(Type type, IDataReader reader)
        {
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                throw new ArgumentException(string.Format("The type of '{0}' does't define a none parameter constructor.", type.FullName));

            ConstructorDescriptor constructorDescriptor = ConstructorDescriptor.GetInstance(constructor);
            ObjectMemberMapper mapper = constructorDescriptor.GetEntityMemberMapper();
            InstanceCreator instanceCreator = constructorDescriptor.GetInstanceCreator();
            List<MemberMap> memberMaps = PrepareMemberMaps(type, reader);

            ComplexObjectActivator objectActivator = new ComplexObjectActivator(type, instanceCreator, new List<IObjectActivator>(), memberMaps, new List<IMemberBinder>(), null, false);
            objectActivator.Prepare(reader);

            return objectActivator;
        }
        static List<MemberMap> PrepareMemberMaps(Type type, IDataReader reader)
        {
            List<MemberMap> memberMaps = new List<MemberMap>(reader.FieldCount);

            MemberInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            MemberInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField);
            List<MemberInfo> members = new List<MemberInfo>(properties.Length + fields.Length);
            members.AppendRange(properties);
            members.AppendRange(fields);

            TypeDescriptor typeDescriptor = EntityTypeContainer.TryGetDescriptor(type);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string name = reader.GetName(i);
                MemberInfo mapMember = TryGetMapMember(members, name, typeDescriptor);

                if (mapMember == null)
                    continue;

                Infrastructure.MappingType mappingType;
                if (!MappingTypeSystem.IsMappingType(mapMember.GetMemberType(), out mappingType))
                {
                    continue;
                }

                MemberMap memberBinder = new MemberMap(mapMember, i, mappingType.DbValueConverter);
                memberMaps.Add(memberBinder);
            }

            return memberMaps;
        }

        static MemberInfo TryGetMapMember(List<MemberInfo> members, string readerName, TypeDescriptor typeDescriptor)
        {
            MemberInfo mapMember = null;

            foreach (MemberInfo member in members)
            {
                string columnName = null;
                if (typeDescriptor != null)
                {
                    PrimitivePropertyDescriptor propertyDescriptor = typeDescriptor.FindPrimitivePropertyDescriptor(member);
                    if (propertyDescriptor != null)
                        columnName = propertyDescriptor.Column.Name;
                }

                if (string.IsNullOrEmpty(columnName))
                {
                    ColumnAttribute columnAttribute = member.GetCustomAttribute<ColumnAttribute>();
                    if (columnAttribute != null)
                        columnName = columnAttribute.Name;
                }

                if (string.IsNullOrEmpty(columnName))
                    continue;

                if (!string.Equals(columnName, readerName, StringComparison.OrdinalIgnoreCase))
                    continue;

                mapMember = member;
                break;
            }

            if (mapMember == null)
            {
                mapMember = members.Find(a => a.Name == readerName);
            }

            if (mapMember == null)
            {
                mapMember = members.Find(a => string.Equals(a.Name, readerName, StringComparison.OrdinalIgnoreCase));
            }

            return mapMember;
        }

        static CacheInfo TryGetCacheInfoFromList(List<CacheInfo> caches, IDataReader reader)
        {
            CacheInfo cache = null;
            for (int i = 0; i < caches.Count; i++)
            {
                var item = caches[i];
                if (item.IsTheSameFields(reader))
                {
                    cache = item;
                    break;
                }
            }

            return cache;
        }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, List<CacheInfo>> ObjectActivatorCache = new System.Collections.Concurrent.ConcurrentDictionary<Type, List<CacheInfo>>();

        public class CacheInfo
        {
            ReaderFieldInfo[] _readerFields;
            ComplexObjectActivator _objectActivator;
            public CacheInfo(ComplexObjectActivator activator, IDataReader reader)
            {
                int fieldCount = reader.FieldCount;
                var readerFields = new ReaderFieldInfo[fieldCount];

                for (int i = 0; i < fieldCount; i++)
                {
                    readerFields[i] = new ReaderFieldInfo(reader.GetName(i), reader.GetFieldType(i));
                }

                this._readerFields = readerFields;
                this._objectActivator = activator;
            }

            public ComplexObjectActivator ObjectActivator { get { return this._objectActivator; } }

            public bool IsTheSameFields(IDataReader reader)
            {
                ReaderFieldInfo[] readerFields = this._readerFields;
                int fieldCount = reader.FieldCount;

                if (fieldCount != readerFields.Length)
                    return false;

                for (int i = 0; i < fieldCount; i++)
                {
                    ReaderFieldInfo readerField = readerFields[i];
                    if (reader.GetFieldType(i) != readerField.Type || reader.GetName(i) != readerField.Name)
                    {
                        return false;
                    }
                }

                return true;
            }

            class ReaderFieldInfo
            {
                string _name;
                Type _type;
                public ReaderFieldInfo(string name, Type type)
                {
                    this._name = name;
                    this._type = type;
                }

                public string Name { get { return this._name; } }
                public Type Type { get { return this._type; } }
            }
        }
    }
}
