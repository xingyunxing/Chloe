using Chloe.Mapper;

namespace Chloe.Query.Mapping
{
    public interface IObjectActivatorCreator
    {
        Type ObjectType { get; }
        bool IsRoot { get; set; }
        IObjectActivator CreateObjectActivator();
        IObjectActivator CreateObjectActivator(IDbContextProvider dbContextProvider);
        IFitter CreateFitter(IDbContextProvider dbContextProvider);
    }
}
