using Chloe.Mapper;
using Chloe.Mapper.Activators;

namespace Chloe.Query.Mapping
{
    public class PrimitiveObjectActivatorCreator : IObjectActivatorCreator
    {
        public PrimitiveObjectActivatorCreator(Type type, int readerOrdinal)
        {
            this.ObjectType = type;
            this.ReaderOrdinal = readerOrdinal;
        }

        public Type ObjectType { get; private set; }
        public bool IsRoot { get; set; }
        public int ReaderOrdinal { get; private set; }
        public int? CheckNullOrdinal { get; set; }

        public IObjectActivator CreateObjectActivator(bool isTrackingQuery)
        {
            PrimitiveObjectActivator activator = new PrimitiveObjectActivator(this.ObjectType, this.ReaderOrdinal);
            return activator;
        }

        public IFitter CreateFitter(bool isTrackingQuery)
        {
            throw new NotSupportedException();
        }
    }
}
