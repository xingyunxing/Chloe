namespace Chloe
{
    public interface IDbContextProviderFactory
    {
        IDbContextProvider CreateDbContextProvider();
    }
}
