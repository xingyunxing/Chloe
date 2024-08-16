using Chloe.Exceptions;
using Chloe.Query;
using Chloe.Reflection;
using Chloe.Reflection.Emit;
using System.Data;

namespace Chloe.Mapper.Activators
{
    public class ComplexObjectActivator : ObjectActivatorBase, IObjectActivator, IReadingOrdinal
    {
        Type _objectType;
        InstanceCreator _instanceCreator;
        List<IObjectActivator> _argumentActivators;
        List<MemberMap> _primitiveMemberMaps;  //映射属性
        List<IMemberBinder> _objectMemberBinders; //非映射属性，如导航属性等
        int? _checkNullOrdinal;

        bool _shouldTrackEntity;

        MultMemberMapper _primitiveMemberMapper;

        public ComplexObjectActivator(Type objectType, InstanceCreator instanceCreator, List<IObjectActivator> argumentActivators, List<MemberMap> primitiveMemberMaps, List<IMemberBinder> objectMemberBinders, int? checkNullOrdinal, bool shouldTrackEntity)
        {
            this._objectType = objectType;
            this._instanceCreator = instanceCreator;
            this._primitiveMemberMaps = primitiveMemberMaps;
            this._argumentActivators = argumentActivators;
            this._objectMemberBinders = objectMemberBinders;
            this._checkNullOrdinal = checkNullOrdinal;
            this._shouldTrackEntity = shouldTrackEntity;
        }

        /// <summary>
        /// 记录当前读取的 dataReader 序号，方便抛错
        /// </summary>
        public int Ordinal { get; set; }

        public override void Prepare(IDataReader reader)
        {
            this._primitiveMemberMapper = this.GetPrimitiveMapper(reader);

            for (int i = 0; i < this._argumentActivators.Count; i++)
            {
                IObjectActivator argumentActivator = this._argumentActivators[i];
                argumentActivator.Prepare(reader);
            }
            for (int i = 0; i < this._objectMemberBinders.Count; i++)
            {
                IMemberBinder binder = this._objectMemberBinders[i];
                binder.Prepare(reader);
            }
        }
        public MultMemberMapper GetPrimitiveMapper(IDataReader reader)
        {
            FieldMemberMap[] mapInfos = new FieldMemberMap[this._primitiveMemberMaps.Count];
            for (int i = 0; i < this._primitiveMemberMaps.Count; i++)
            {
                mapInfos[i] = new FieldMemberMap() { MemberMap = this._primitiveMemberMaps[i], ReaderDataType = reader.GetFieldType(this._primitiveMemberMaps[i].Ordinal) };
            }
            MultMemberMapper mapper = MultMemberMapperContainer.GetOrAdd(new MultMemberMapperCacheKey(this._objectType, mapInfos), () =>
            {
                MultMemberMapper mapper = DelegateGenerator.CreateMultMemberMapper(this._objectType, mapInfos);
                return mapper;
            });

            return mapper;
        }


        public override async ObjectResultTask CreateInstance(QueryContext queryContext, IDataReader reader, bool @async)
        {
            if (this._checkNullOrdinal != null)
            {
                if (reader.IsDBNull(this._checkNullOrdinal.Value))
                    return null;
            }

            object[] arguments = this._argumentActivators.Count == 0 ? PublicConstants.EmptyArray : new object[this._argumentActivators.Count];

            for (int i = 0; i < this._argumentActivators.Count; i++)
            {
                arguments[i] = await this._argumentActivators[i].CreateInstance(queryContext, reader, @async);
            }

            object obj = this._instanceCreator(arguments);

            try
            {
                this._primitiveMemberMapper(obj, reader, this);
            }
            catch (ChloeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ChloeException(AppendErrorMsg(reader, this.Ordinal, ex), ex);
            }

            IMemberBinder memberBinder = null;
            int count = this._objectMemberBinders.Count;
            for (int i = 0; i < count; i++)
            {
                memberBinder = this._objectMemberBinders[i];
                await memberBinder.Bind(queryContext, obj, reader, @async);
            }

            if (this._shouldTrackEntity)
            {
                queryContext.DbContextProvider.TrackEntity(obj);
            }

            return obj;
        }

        public static string AppendErrorMsg(IDataReader reader, int ordinal, Exception ex)
        {
            string msg = null;
            if (reader.IsDBNull(ordinal))
            {
                msg = string.Format("Please make sure that the member of the column '{0}'({1},{2},{3}) map is nullable.", reader.GetName(ordinal), ordinal.ToString(), reader.GetDataTypeName(ordinal), reader.GetFieldType(ordinal).FullName);
            }
            else if (ex is InvalidCastException)
            {
                msg = string.Format("Please make sure that the member of the column '{0}'({1},{2},{3}) map is the correct type.", reader.GetName(ordinal), ordinal.ToString(), reader.GetDataTypeName(ordinal), reader.GetFieldType(ordinal).FullName);
            }
            else
                msg = string.Format("An error occurred while mapping the column '{0}'({1},{2},{3}). For details please see the inner exception.", reader.GetName(ordinal), ordinal.ToString(), reader.GetDataTypeName(ordinal), reader.GetFieldType(ordinal).FullName);
            return msg;
        }

        public override IObjectActivator Clone()
        {
            List<IObjectActivator> argumentActivators = new List<IObjectActivator>(this._argumentActivators.Count);
            argumentActivators.AddRange(this._argumentActivators.Select(a => a.Clone()));

            List<IMemberBinder> objectMemberBinders = new List<IMemberBinder>(this._objectMemberBinders.Count);
            objectMemberBinders.AddRange(this._objectMemberBinders.Select(a => a.Clone()));

            ComplexObjectActivator complexObjectActivator = new ComplexObjectActivator(this._objectType, this._instanceCreator, argumentActivators, this._primitiveMemberMaps, objectMemberBinders, this._checkNullOrdinal, this._shouldTrackEntity);

            return complexObjectActivator;
        }
    }

}
