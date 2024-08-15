using Chloe.Data;
using Chloe.Query;
using System.Data;

namespace Chloe.Mapper.Activators
{
    public class RootEntityActivator : IObjectActivator
    {
        IObjectActivator _entityActivator;
        IFitter _fitter;
        IEntityKey _entityKey;

        public RootEntityActivator(IObjectActivator entityActivator, IFitter fitter, IEntityKey entityKey)
        {
            this._entityActivator = entityActivator;
            this._fitter = fitter;
            this._entityKey = entityKey;
        }

        public void Prepare(IDataReader reader)
        {
            this._entityActivator.Prepare(reader);
            this._fitter.Prepare(reader);
        }

        public async ObjectResultTask CreateInstance(QueryContext queryContext, IDataReader reader, bool @async)
        {
            var entity = await this._entityActivator.CreateInstance(queryContext, reader, @async);

            //导航属性
            await this._fitter.Fill(queryContext, entity, null, reader, @async);

            IQueryDataReader queryDataReader = (IQueryDataReader)reader;
            queryDataReader.AllowReadNextRecord = true;

            while (await queryDataReader.Read(@async))
            {
                if (!this._entityKey.IsEntityRow(entity, reader))
                {
                    queryDataReader.AllowReadNextRecord = false;
                    break;
                }

                await this._fitter.Fill(queryContext, entity, null, reader, @async);
            }

            return entity;
        }

        public IObjectActivator Clone()
        {
            RootEntityActivator rootEntityActivator = new RootEntityActivator(this._entityActivator.Clone(), this._fitter.Clone(), this._entityKey.Clone());
            return rootEntityActivator;
        }
    }
}
