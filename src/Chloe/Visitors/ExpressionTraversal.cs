using Chloe.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Chloe.Visitors
{
    public class ExpressionTraversal : ExpressionVisitor<bool>
    {
        #region Expression

        bool CheckMany(IList<Expression> exps)
        {
            for (int i = 0; i < exps.Count; i++)
            {
                if (this.Visit(exps[i]))
                {
                    return true;
                }
            }

            return false;
        }

        protected override bool VisitExpression(Expression exp)
        {
            return false;
        }
        protected override bool VisitUnary(UnaryExpression exp)
        {
            return this.Visit(exp.Operand);
        }
        protected override bool VisitBinary(BinaryExpression exp)
        {
            if (this.Visit(exp.Left))
                return true;

            if (this.Visit(exp.Right))
                return true;

            return false;
        }
        protected override bool VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }
        protected override bool VisitMemberAccess(MemberExpression exp)
        {
            return this.Visit(exp.Expression);
        }
        protected override bool VisitConditional(ConditionalExpression exp)
        {
            if (this.Visit(exp.Test))
                return true;

            if (this.Visit(exp.IfTrue))
                return true;

            if (this.Visit(exp.IfFalse))
                return true;

            return false;
        }
        protected override bool VisitMethodCall(MethodCallExpression exp)
        {
            if (this.Visit(exp.Object))
                return true;

            if (this.CheckMany(exp.Arguments))
                return true;

            return false;
        }
        protected override bool VisitNew(NewExpression exp)
        {
            if (this.CheckMany(exp.Arguments))
                return true;

            return false;
        }
        protected override bool VisitNewArray(NewArrayExpression exp)
        {
            if (this.CheckMany(exp.Expressions))
                return true;

            return false;
        }
        protected override bool VisitMemberInit(MemberInitExpression exp)
        {
            if (this.Visit(exp.NewExpression))
                return true;

            for (int i = 0; i < exp.Bindings.Count; i++)
            {
                switch (exp.Bindings[i])
                {
                    case MemberAssignment memberAssignment:
                        if (this.Visit(memberAssignment.Expression))
                            return true;
                        break;
                    default:
                        throw new NotSupportedException(exp.Bindings[i].ToString());
                }
            }

            return false;
        }
        protected override bool VisitListInit(ListInitExpression exp)
        {
            for (int i = 0; i < exp.Initializers.Count; i++)
            {
                for (int j = 0; j < exp.Initializers[i].Arguments.Count; j++)
                {
                    if (this.Visit(exp.Initializers[i].Arguments[j]))
                        return true;
                }
            }

            return false;
        }

        #endregion



        #region QueryExpression

        public virtual bool Visit(QueryExpression exp)
        {
            if (exp == null)
                return false;

            if (exp.PrevExpression != null)
            {
                if (this.Visit(exp.PrevExpression))
                    return true;
            }

            switch (exp.NodeType)
            {
                case QueryExpressionType.Root:
                    return this.VisitRootQuery((RootQueryExpression)exp);
                case QueryExpressionType.Where:
                    return this.VisitWhere((WhereExpression)exp);
                case QueryExpressionType.Take:
                    return this.VisitTake((TakeExpression)exp);
                case QueryExpressionType.Skip:
                    return this.VisitSkip((SkipExpression)exp);
                case QueryExpressionType.Paging:
                    return this.VisitPaging((PagingExpression)exp);
                case QueryExpressionType.OrderBy:
                case QueryExpressionType.OrderByDesc:
                case QueryExpressionType.ThenBy:
                case QueryExpressionType.ThenByDesc:
                    return this.VisitOrder((OrderExpression)exp);
                case QueryExpressionType.Select:
                    return this.VisitSelect((SelectExpression)exp);
                case QueryExpressionType.Include:
                    return this.VisitInclude((IncludeExpression)exp);
                case QueryExpressionType.BindTwoWay:
                    return this.VisitBindTwoWay((BindTwoWayExpression)exp);
                case QueryExpressionType.Exclude:
                    return this.VisitExclude((ExcludeExpression)exp);
                case QueryExpressionType.Aggregate:
                    return this.VisitAggregateQuery((AggregateQueryExpression)exp);
                case QueryExpressionType.JoinQuery:
                    return this.VisitJoinQuery((JoinQueryExpression)exp);
                case QueryExpressionType.GroupingQuery:
                    return this.VisitGroupingQuery((GroupingQueryExpression)exp);
                case QueryExpressionType.Distinct:
                    return this.VisitDistinct((DistinctExpression)exp);
                case QueryExpressionType.IgnoreAllFilters:
                    return this.VisitIgnoreAllFilters((IgnoreAllFiltersExpression)exp);
                case QueryExpressionType.Tracking:
                    return this.VisitTracking((TrackingExpression)exp);
                default:
                    throw new NotSupportedException(exp.NodeType.ToString());
            }
        }

        public bool VisitRootQuery(RootQueryExpression exp)
        {
            List<LambdaExpression> globalFilters = exp.GlobalFilters;
            for (int i = 0; i < globalFilters.Count; i++)
            {
                if (this.Visit(globalFilters[i]))
                    return true;
            }

            List<LambdaExpression> contextFilters = exp.ContextFilters;
            for (int i = 0; i < contextFilters.Count; i++)
            {
                if (this.Visit(contextFilters[i]))
                    return true;
            }

            return false;
        }
        public bool VisitWhere(WhereExpression exp)
        {
            if (this.Visit(exp.Predicate))
                return true;

            return false;
        }
        public bool VisitSelect(SelectExpression exp)
        {
            if (this.Visit(exp.Selector))
                return true;

            return false;
        }
        public bool VisitTake(TakeExpression exp)
        {
            return false;
        }
        public bool VisitSkip(SkipExpression exp)
        {
            return false;
        }
        public bool VisitOrder(OrderExpression exp)
        {
            if (this.Visit(exp.KeySelector))
                return true;

            return false;
        }
        public bool VisitAggregateQuery(AggregateQueryExpression exp)
        {
            List<Expression> arguments = new List<Expression>(exp.Arguments.Count);

            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                if (this.Visit(exp.Arguments[i]))
                    return true;
            }

            return false;
        }
        public bool VisitJoinQuery(JoinQueryExpression exp)
        {
            if (this.Visit(exp.Selector))
                return true;

            for (int i = 0; i < exp.JoinedQueries.Count; i++)
            {
                if (this.Visit(exp.JoinedQueries[i].Condition))
                    return true;
            }

            return false;
        }
        public bool VisitGroupingQuery(GroupingQueryExpression exp)
        {
            if (this.Visit(exp.Selector))
                return true;

            for (int i = 0; i < exp.GroupKeySelectors.Count; i++)
            {
                if (this.Visit(exp.GroupKeySelectors[i]))
                    return true;
            }

            for (int i = 0; i < exp.HavingPredicates.Count; i++)
            {
                if (this.Visit(exp.HavingPredicates[i]))
                    return true;
            }

            List<GroupingQueryOrdering> orderings = new List<GroupingQueryOrdering>(exp.Orderings.Count);
            for (int i = 0; i < exp.Orderings.Count; i++)
            {
                if (this.Visit(exp.Orderings[i].KeySelector))
                    return true;
            }

            return false;
        }
        public bool VisitDistinct(DistinctExpression exp)
        {
            return false;
        }
        public bool VisitInclude(IncludeExpression exp)
        {
            if (this.ProcessNavigationNode(exp.NavigationNode))
                return true;

            return false;
        }
        public bool VisitBindTwoWay(BindTwoWayExpression exp)
        {
            return false;
        }
        public bool VisitExclude(ExcludeExpression exp)
        {
            if (this.Visit(exp.Field))
                return true;

            return false;
        }
        public bool VisitIgnoreAllFilters(IgnoreAllFiltersExpression exp)
        {
            return false;
        }
        public bool VisitTracking(TrackingExpression exp)
        {
            return false;
        }
        public bool VisitPaging(PagingExpression exp)
        {
            return false;
        }
        bool ProcessNavigationNode(NavigationNode navigationNode)
        {
            for (int i = 0; i < navigationNode.ExcludedFields.Count; i++)
            {
                if (this.Visit(navigationNode.ExcludedFields[i]))
                    return true;
            }

            if (this.Visit(navigationNode.Condition))
                return true;

            List<LambdaExpression> globalFilters = navigationNode.GlobalFilters;
            for (int i = 0; i < globalFilters.Count; i++)
            {
                if (this.Visit(globalFilters[i]))
                    return true;
            }

            List<LambdaExpression> contextFilters = navigationNode.ContextFilters;
            for (int i = 0; i < contextFilters.Count; i++)
            {
                if (this.Visit(contextFilters[i]))
                    return true;
            }

            if (navigationNode.Next != null)
            {
                if (this.ProcessNavigationNode(navigationNode.Next))
                    return true;
            }

            return false;
        }

        #endregion
    }
}
