using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.Query.QueryExpressions;
using Chloe.Sharding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Sharding
{
    internal class ShardingQueryModelPeeker : QueryExpressionVisitor<QueryExpression>
    {
        ShardingQueryModel QueryModel = new ShardingQueryModel();

        public static ShardingQueryModel Peek(QueryExpression queryExpression)
        {
            var peeker = new ShardingQueryModelPeeker();
            queryExpression.Accept(peeker);

            return peeker.QueryModel;
        }

        public override QueryExpression Visit(RootQueryExpression exp)
        {
            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(exp.ElementType);

            this.QueryModel.GlobalFilters.AddRange(typeDescriptor.Definition.Filters);
            this.QueryModel.ContextFilters.AddRange(exp.ContextFilters);
            return exp;
        }
        public override QueryExpression Visit(WhereExpression exp)
        {
            exp.PrevExpression.Accept(this);
            this.QueryModel.Conditions.Add(exp.Predicate);
            return exp;
        }
        public override QueryExpression Visit(SelectExpression exp)
        {
            throw new NotImplementedException();
        }
        public override QueryExpression Visit(TakeExpression exp)
        {
            exp.PrevExpression.Accept(this);
            this.QueryModel.Take = exp.Count;
            return exp;
        }
        public override QueryExpression Visit(SkipExpression exp)
        {
            exp.PrevExpression.Accept(this);
            this.QueryModel.Skip = exp.Count;
            return exp;
        }
        public override QueryExpression Visit(OrderExpression exp)
        {
            exp.PrevExpression.Accept(this);

            if (exp.NodeType == QueryExpressionType.OrderBy || exp.NodeType == QueryExpressionType.OrderByDesc)
            {
                this.QueryModel.Orderings.Clear();
            }

            Ordering ordering = new Ordering();
            ordering.KeySelector = exp.KeySelector;

            var memberExp = exp.KeySelector.Body as System.Linq.Expressions.MemberExpression;
            if (memberExp == null || memberExp.Expression.NodeType != System.Linq.Expressions.ExpressionType.Parameter)
            {
                throw new NotSupportedException(exp.KeySelector.ToString());
            }

            ordering.Member = memberExp.Member;

            if (exp.NodeType == QueryExpressionType.OrderBy || exp.NodeType == QueryExpressionType.ThenBy)
            {
                ordering.Ascending = true;
            }
            else if (exp.NodeType == QueryExpressionType.OrderByDesc || exp.NodeType == QueryExpressionType.ThenByDesc)
            {
                ordering.Ascending = false;
            }

            this.QueryModel.Orderings.Add(ordering);

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
            throw new NotImplementedException();
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
            this.QueryModel.IgnoreAllFilters = true;
            return exp;
        }

    }
}
