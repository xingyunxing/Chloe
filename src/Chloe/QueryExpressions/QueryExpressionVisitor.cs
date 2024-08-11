using System.Linq.Expressions;

namespace Chloe.QueryExpressions
{
    public class QueryExpressionVisitor : ExpressionVisitor, IQueryExpressionVisitor<QueryExpression>
    {
        public QueryExpressionVisitor()
        {

        }

        public virtual QueryExpression Visit(RootQueryExpression exp)
        {
            RootQueryExpression rootQueryExpression = new RootQueryExpression(exp.ElementType, exp.ExplicitTable, exp.Lock, exp.GlobalFilters.Count, exp.ContextFilters.Count);

            List<LambdaExpression> globalFilters = exp.GlobalFilters;
            for (int i = 0; i < globalFilters.Count; i++)
            {
                rootQueryExpression.GlobalFilters.Add((LambdaExpression)this.Visit(globalFilters[i]));
            }

            List<LambdaExpression> contextFilters = exp.ContextFilters;
            for (int i = 0; i < contextFilters.Count; i++)
            {
                rootQueryExpression.ContextFilters.Add((LambdaExpression)this.Visit(contextFilters[i]));
            }

            return rootQueryExpression;
        }
        public virtual QueryExpression Visit(WhereExpression exp)
        {
            QueryExpression prevExp = exp.PrevExpression.Accept(this);
            LambdaExpression predicate = (LambdaExpression)this.Visit(exp.Predicate);
            return new WhereExpression(exp.ElementType, prevExp, predicate);
        }
        public virtual QueryExpression Visit(SelectExpression exp)
        {
            QueryExpression prevExp = exp.PrevExpression.Accept(this);
            LambdaExpression selector = (LambdaExpression)this.Visit(exp.Selector);
            return new SelectExpression(exp.ElementType, prevExp, selector);
        }
        public virtual QueryExpression Visit(TakeExpression exp)
        {
            QueryExpression prevExp = exp.PrevExpression.Accept(this);
            return new TakeExpression(exp.ElementType, prevExp, exp.Count);
        }
        public virtual QueryExpression Visit(SkipExpression exp)
        {
            QueryExpression prevExp = exp.PrevExpression.Accept(this);
            return new SkipExpression(exp.ElementType, prevExp, exp.Count);
        }
        public virtual QueryExpression Visit(OrderExpression exp)
        {
            QueryExpression prevExp = exp.PrevExpression.Accept(this);
            LambdaExpression keySelector = (LambdaExpression)this.Visit(exp.KeySelector);
            return new OrderExpression(exp.ElementType, prevExp, exp.NodeType, keySelector);
        }
        public virtual QueryExpression Visit(AggregateQueryExpression exp)
        {
            QueryExpression prevExp = exp.PrevExpression.Accept(this);

            List<Expression> arguments = new List<Expression>(exp.Arguments.Count);

            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                arguments.Add(this.Visit(exp.Arguments[i]));
            }

            return new AggregateQueryExpression(prevExp, exp.Method, arguments);
        }
        public virtual QueryExpression Visit(JoinQueryExpression exp)
        {
            QueryExpression prevExp = exp.PrevExpression.Accept(this);
            LambdaExpression selector = (LambdaExpression)this.Visit(exp.Selector);

            List<JoinQueryInfo> joinedQueries = new List<JoinQueryInfo>(exp.JoinedQueries.Count);

            for (int i = 0; i < exp.JoinedQueries.Count; i++)
            {
                LambdaExpression condition = (LambdaExpression)this.Visit(exp.JoinedQueries[i].Condition);
                JoinQueryInfo joinedQuery = new JoinQueryInfo(exp.JoinedQueries[i].Query, exp.JoinedQueries[i].JoinType, condition);
                joinedQueries.Add(joinedQuery);
            }

            return new JoinQueryExpression(exp.ElementType, prevExp, joinedQueries, selector);
        }
        public virtual QueryExpression Visit(GroupingQueryExpression exp)
        {
            QueryExpression prevExp = exp.PrevExpression.Accept(this);
            LambdaExpression selector = (LambdaExpression)this.Visit(exp.Selector);

            List<LambdaExpression> groupKeySelectors = new List<LambdaExpression>(exp.GroupKeySelectors.Count);
            for (int i = 0; i < exp.GroupKeySelectors.Count; i++)
            {
                groupKeySelectors.Add((LambdaExpression)this.Visit(exp.GroupKeySelectors[i]));
            }

            List<LambdaExpression> havingPredicates = new List<LambdaExpression>(exp.HavingPredicates.Count);
            for (int i = 0; i < exp.HavingPredicates.Count; i++)
            {
                havingPredicates.Add((LambdaExpression)this.Visit(exp.HavingPredicates[i]));
            }

            List<GroupingQueryOrdering> orderings = new List<GroupingQueryOrdering>(exp.Orderings.Count);
            for (int i = 0; i < exp.Orderings.Count; i++)
            {
                orderings.Add(new GroupingQueryOrdering((LambdaExpression)this.Visit(exp.Orderings[i].KeySelector), exp.Orderings[i].OrderType));
            }

            return new GroupingQueryExpression(exp.ElementType, prevExp, groupKeySelectors, havingPredicates, orderings, selector);
        }
        public virtual QueryExpression Visit(DistinctExpression exp)
        {
            return new DistinctExpression(exp.ElementType, exp.PrevExpression.Accept(this));
        }

        public virtual QueryExpression Visit(IncludeExpression exp)
        {
            return new IncludeExpression(exp.ElementType, exp.PrevExpression.Accept(this), this.ProcessNavigationNode(exp.NavigationNode));
        }
        public virtual QueryExpression Visit(BindTwoWayExpression exp)
        {
            return new BindTwoWayExpression(exp.ElementType, exp.PrevExpression.Accept(this));
        }

        public virtual QueryExpression Visit(ExcludeExpression exp)
        {
            return new ExcludeExpression(exp.ElementType, exp.PrevExpression.Accept(this), (LambdaExpression)this.Visit(exp.Field));
        }

        public virtual QueryExpression Visit(IgnoreAllFiltersExpression exp)
        {
            return new IgnoreAllFiltersExpression(exp.ElementType, exp.PrevExpression.Accept(this));
        }

        public virtual QueryExpression Visit(TrackingExpression exp)
        {
            return new IgnoreAllFiltersExpression(exp.ElementType, exp.PrevExpression.Accept(this));
        }

        public virtual QueryExpression Visit(PagingExpression exp)
        {
            return new PagingExpression(exp.ElementType, exp.PrevExpression.Accept(this), exp.PageNumber, exp.PageSize);
        }

        NavigationNode ProcessNavigationNode(NavigationNode navigationNode)
        {
            NavigationNode node = new NavigationNode(navigationNode.Property, navigationNode.GlobalFilters.Count, navigationNode.ContextFilters.Count);

            for (int i = 0; i < navigationNode.ExcludedFields.Count; i++)
            {
                node.ExcludedFields.Add((LambdaExpression)this.Visit(navigationNode.ExcludedFields[i]));
            }

            node.Condition = (LambdaExpression)this.Visit(navigationNode.Condition);

            List<LambdaExpression> globalFilters = navigationNode.GlobalFilters;
            for (int i = 0; i < globalFilters.Count; i++)
            {
                node.GlobalFilters.Add((LambdaExpression)this.Visit(globalFilters[i]));
            }

            List<LambdaExpression> contextFilters = navigationNode.ContextFilters;
            for (int i = 0; i < contextFilters.Count; i++)
            {
                node.ContextFilters.Add((LambdaExpression)this.Visit(contextFilters[i]));
            }

            if (navigationNode.Next != null)
            {
                node.Next = this.ProcessNavigationNode(navigationNode.Next);
            }

            return node;
        }

    }
}
