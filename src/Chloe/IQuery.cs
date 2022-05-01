using Chloe.QueryExpressions;

namespace Chloe
{
    public interface IQuery
    {
        Type ElementType { get; }
        QueryExpression QueryExpression { get; }
    }
}
