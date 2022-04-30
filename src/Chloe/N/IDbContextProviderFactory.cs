namespace Chloe
{
    public interface IDbContextProviderFactory
    {
        IDbContextProvider CreateDbContextProvider();
    }

    //class RouteDbContextProviderFactoryWrapperFacade : IDbContextProviderFactory
    //{
    //    IDbContextProviderFactory _dbContextProviderFactory;
    //    ShardingDbContextProvider _shardingDbContextProvider;

    //    public RouteDbContextProviderFactoryWrapperFacade(IDbContextProviderFactory dbContextProviderFactory, ShardingDbContextProvider shardingDbContextProvider)
    //    {
    //        this._dbContextProviderFactory = dbContextProviderFactory;
    //        this._shardingDbContextProvider = shardingDbContextProvider;
    //    }

    //    public IDbContextProvider CreateDbContextProvider()
    //    {
    //        var dbContextProvider = this._dbContextProviderFactory.CreateDbContextProvider();
    //        //TODO
    //        throw new NotImplementedException();
    //        //foreach (var kv in (this._shardingDbContext as IDbContextInternal).QueryFilters)
    //        //{
    //        //    foreach (var filter in kv.Value)
    //        //    {
    //        //        dbContextProvider.HasQueryFilter(kv.Key, filter);
    //        //    }
    //        //}

    //        return dbContextProvider;
    //    }
    //}
}
