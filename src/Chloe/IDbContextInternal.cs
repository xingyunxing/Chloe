using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Chloe
{
    internal interface IDbContextInternal : IDbContext
    {
        Dictionary<Type, List<LambdaExpression>> QueryFilters { get; }
    }
}
