using System.Linq.Expressions;

namespace Chloe
{
    internal interface IDbContextInternal : IDbContext
    {
        //Dictionary<Type, List<LambdaExpression>> QueryFilters { get; }
    }
}
