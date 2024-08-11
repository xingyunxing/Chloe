using Chloe.Exceptions;
using Chloe.Mapper.Binders;
using Chloe.Query;
using Chloe.Reflection;
using System.Collections.Generic;
using System.Data;

namespace Chloe.Mapper.Activators
{
    public class ComplexObjectActivator : ObjectActivatorBase, IObjectActivator
    {
        InstanceCreator _instanceCreator;
        List<IObjectActivator> _argumentActivators;
        List<IMemberBinder> _memberBinders;
        int? _checkNullOrdinal;

        bool _shouldTrackEntity;

        public ComplexObjectActivator(InstanceCreator instanceCreator, List<IObjectActivator> argumentActivators, List<IMemberBinder> memberBinders, int? checkNullOrdinal, bool shouldTrackEntity)
        {
            this._instanceCreator = instanceCreator;
            this._argumentActivators = argumentActivators;
            this._memberBinders = memberBinders;
            this._checkNullOrdinal = checkNullOrdinal;
            this._shouldTrackEntity = shouldTrackEntity;
        }

        public override void Prepare(IDataReader reader)
        {
            for (int i = 0; i < this._argumentActivators.Count; i++)
            {
                IObjectActivator argumentActivator = this._argumentActivators[i];
                argumentActivator.Prepare(reader);
            }
            for (int i = 0; i < this._memberBinders.Count; i++)
            {
                IMemberBinder binder = this._memberBinders[i];
                binder.Prepare(reader);
            }
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

            IMemberBinder memberBinder = null;
            try
            {
                int count = this._memberBinders.Count;
                for (int i = 0; i < count; i++)
                {
                    memberBinder = this._memberBinders[i];
                    await memberBinder.Bind(queryContext, obj, reader, @async);
                }
            }
            catch (ChloeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                PrimitiveMemberBinder binder = memberBinder as PrimitiveMemberBinder;
                if (binder != null)
                {
                    throw new ChloeException(AppendErrorMsg(reader, binder.Ordinal, ex), ex);
                }

                throw;
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

            List<IMemberBinder> memberBinders = new List<IMemberBinder>(this._memberBinders.Count);
            memberBinders.AddRange(this._memberBinders.Select(a => a.Clone()));

            ComplexObjectActivator complexObjectActivator = new ComplexObjectActivator(this._instanceCreator, argumentActivators, memberBinders, this._checkNullOrdinal, this._shouldTrackEntity);

            return complexObjectActivator;
        }
    }

}
