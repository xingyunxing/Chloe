using Chloe.DbExpressions;
using System.Linq.Expressions;

namespace Chloe.QueryExpressions
{
    public class GroupingQueryExpression : QueryExpression
    {
        List<LambdaExpression> _groupKeySelectors;
        List<LambdaExpression> _havingPredicates;
        List<GroupingQueryOrdering> _orderings;
        LambdaExpression _selector;

        public GroupingQueryExpression(Type elementType, QueryExpression prevExpression, LambdaExpression selector)
            : this(elementType, prevExpression, new List<LambdaExpression>(), new List<LambdaExpression>(), new List<GroupingQueryOrdering>(), selector)
        {

        }

        public GroupingQueryExpression(Type elementType, QueryExpression prevExpression, List<LambdaExpression> groupKeySelectors, List<LambdaExpression> havingPredicates, List<GroupingQueryOrdering> orderings, LambdaExpression selector) : base(QueryExpressionType.GroupingQuery, elementType, prevExpression)
        {
            this._groupKeySelectors = groupKeySelectors;
            this._havingPredicates = havingPredicates;
            this._orderings = orderings;
            this._selector = selector;
        }

        public List<LambdaExpression> GroupKeySelectors { get { return this._groupKeySelectors; } }
        public List<LambdaExpression> HavingPredicates { get { return this._havingPredicates; } }
        public List<GroupingQueryOrdering> Orderings { get { return this._orderings; } }
        public LambdaExpression Selector { get { return this._selector; } }

        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class GroupingQueryOrdering
    {
        LambdaExpression _keySelector;
        DbOrderType _orderType;
        public GroupingQueryOrdering(LambdaExpression keySelector, DbOrderType orderType)
        {
            this._keySelector = keySelector;
            this._orderType = orderType;
        }
        public LambdaExpression KeySelector { get { return this._keySelector; } }
        public DbOrderType OrderType { get { return this._orderType; } }
    }
}
