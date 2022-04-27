using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.Query.QueryExpressions;

namespace Chloe.Sharding
{
    internal class ShardingQueryModelPeeker : QueryExpressionVisitor<QueryExpression>
    {
        ShardingQueryModel _queryModel = new ShardingQueryModel();

        public static ShardingQueryModel Peek(QueryExpression queryExpression)
        {
            var peeker = new ShardingQueryModelPeeker();
            queryExpression.Accept(peeker);

            return peeker._queryModel;
        }

        public override QueryExpression Visit(RootQueryExpression exp)
        {
            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(exp.ElementType);

            this._queryModel.GlobalFilters.AddRange(typeDescriptor.Definition.Filters);
            this._queryModel.ContextFilters.AddRange(exp.ContextFilters);

            this._queryModel.RootEntityType = exp.ElementType;

            return exp;
        }
        public override QueryExpression Visit(WhereExpression exp)
        {
            exp.PrevExpression.Accept(this);
            this._queryModel.Conditions.Add(exp.Predicate);
            return exp;
        }
        public override QueryExpression Visit(SelectExpression exp)
        {
            throw new NotImplementedException();
            //this._queryModel.Selector = exp.Selector;
            //return exp;
        }
        public override QueryExpression Visit(TakeExpression exp)
        {
            exp.PrevExpression.Accept(this);
            this._queryModel.Take = exp.Count;
            return exp;
        }
        public override QueryExpression Visit(SkipExpression exp)
        {
            exp.PrevExpression.Accept(this);
            this._queryModel.Skip = exp.Count;
            return exp;
        }
        public override QueryExpression Visit(OrderExpression exp)
        {
            exp.PrevExpression.Accept(this);

            if (exp.NodeType == QueryExpressionType.OrderBy || exp.NodeType == QueryExpressionType.OrderByDesc)
            {
                this._queryModel.Orderings.Clear();
            }

            Ordering ordering = new Ordering();
            ordering.KeySelector = exp.KeySelector;

            if (exp.NodeType == QueryExpressionType.OrderBy || exp.NodeType == QueryExpressionType.ThenBy)
            {
                ordering.Ascending = true;
            }
            else if (exp.NodeType == QueryExpressionType.OrderByDesc || exp.NodeType == QueryExpressionType.ThenByDesc)
            {
                ordering.Ascending = false;
            }

            this._queryModel.Orderings.Add(ordering);

            return exp;
        }
        public override QueryExpression Visit(AggregateQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public override QueryExpression Visit(JoinQueryExpression exp)
        {
            throw new NotImplementedException();
        }
        public override QueryExpression Visit(GroupingQueryExpression exp)
        {
            exp.PrevExpression.Accept(this);
            this._queryModel.GroupKeySelectors.AddRange(exp.GroupKeySelectors);
            this._queryModel.Selector = exp.Selector;
            return exp;
        }
        public override QueryExpression Visit(DistinctExpression exp)
        {
            throw new NotImplementedException();
        }

        public override QueryExpression Visit(IncludeExpression exp)
        {
            throw new NotImplementedException();
        }

        public override QueryExpression Visit(IgnoreAllFiltersExpression exp)
        {
            exp.PrevExpression.Accept(this);
            this._queryModel.IgnoreAllFilters = true;
            return exp;
        }

    }
}
