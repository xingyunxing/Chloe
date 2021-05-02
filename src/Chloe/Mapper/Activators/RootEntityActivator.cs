using Chloe.Data;
using System.Data;
using System.Threading.Tasks;

#if netfx
using ObjectResultTask = System.Threading.Tasks.Task<object>;
#else
using ObjectResultTask = System.Threading.Tasks.ValueTask<object>;
#endif

namespace Chloe.Mapper.Activators
{
    public class RootEntityActivator : IObjectActivator
    {
        IObjectActivator _entityActivator;
        IFitter _fitter;
        IEntityRowComparer _entityRowComparer;

        public RootEntityActivator(IObjectActivator entityActivator, IFitter fitter, IEntityRowComparer entityRowComparer)
        {
            this._entityActivator = entityActivator;
            this._fitter = fitter;
            this._entityRowComparer = entityRowComparer;
        }

        public void Prepare(IDataReader reader)
        {
            this._entityActivator.Prepare(reader);
            this._fitter.Prepare(reader);
        }

        public async ObjectResultTask CreateInstance(IDataReader reader, bool @async)
        {
            var entity = await this._entityActivator.CreateInstance(reader, @async);

            //导航属性
            await this._fitter.Fill(entity, null, reader, @async);

            IQueryDataReader queryDataReader = (IQueryDataReader)reader;
            queryDataReader.AllowReadNextRecord = true;

            while (await queryDataReader.Read(true))
            {
                if (!_entityRowComparer.IsEntityRow(entity, reader))
                {
                    queryDataReader.AllowReadNextRecord = false;
                    break;
                }

                await this._fitter.Fill(entity, null, reader, @async);
            }

            return entity;
        }
    }
}
