using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Visitors
{
    public class ExpressionTraversal
    {
        #region Expression

        protected ExpressionTraversal()
        {

        }

        public virtual void Visit(Expression exp)
        {
            if (exp == null)
                return;

            switch (exp.NodeType)
            {
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Quote:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.TypeAs:
                    this.VisitUnary((UnaryExpression)exp);
                    return;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    this.VisitBinary((BinaryExpression)exp);
                    return;
                case ExpressionType.Lambda:
                    this.VisitLambda((LambdaExpression)exp);
                    return;
                //case ExpressionType.TypeIs:
                //    return this.VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    this.VisitConditional((ConditionalExpression)exp);
                    return;
                case ExpressionType.Constant:
                    this.VisitConstant((ConstantExpression)exp);
                    return;
                case ExpressionType.Parameter:
                    this.VisitParameter((ParameterExpression)exp);
                    return;
                case ExpressionType.MemberAccess:
                    this.VisitMemberAccess((MemberExpression)exp);
                    return;
                case ExpressionType.Call:
                    this.VisitMethodCall((MethodCallExpression)exp);
                    return;
                case ExpressionType.New:
                    this.VisitNew((NewExpression)exp);
                    return;
                case ExpressionType.NewArrayInit:
                    //case ExpressionType.NewArrayBounds:
                    this.VisitNewArray((NewArrayExpression)exp);
                    return;
                //case ExpressionType.Invoke:
                //    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    this.VisitMemberInit((MemberInitExpression)exp);
                    return;
                case ExpressionType.ListInit:
                    this.VisitListInit((ListInitExpression)exp);
                    return;
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        protected virtual void VisitUnary(UnaryExpression exp)
        {
            if (exp == null)
                return;

            this.Visit(exp.Operand);
        }

        protected virtual void VisitBinary(BinaryExpression exp)
        {
            if (exp == null)
                return;

            this.Visit(exp.Left);
            this.Visit(exp.Right);
        }

        protected virtual void VisitConstant(ConstantExpression exp)
        {

        }
        protected virtual void VisitParameter(ParameterExpression exp)
        {

        }

        protected virtual void VisitLambda(LambdaExpression exp)
        {
            for (int i = 0; i < exp.Parameters.Count; i++)
            {
                this.Visit(exp.Parameters[i]);
            }
            this.Visit(exp.Body);
        }
        protected virtual void VisitMemberAccess(MemberExpression exp)
        {
            this.Visit(exp.Expression);
        }
        protected virtual void VisitConditional(ConditionalExpression exp)
        {
            this.Visit(exp.Test);
            this.Visit(exp.IfTrue);
            this.Visit(exp.IfFalse);
        }
        protected virtual void VisitMethodCall(MethodCallExpression exp)
        {
            this.Visit(exp.Object);
            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                this.Visit(exp.Arguments[i]);
            }
        }
        protected virtual void VisitNew(NewExpression exp)
        {
            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                this.Visit(exp.Arguments[i]);
            }
        }
        protected virtual void VisitNewArray(NewArrayExpression exp)
        {
            for (int i = 0; i < exp.Expressions.Count; i++)
            {
                this.Visit(exp.Expressions[i]);
            }
        }
        protected virtual void VisitMemberInit(MemberInitExpression exp)
        {
            this.Visit(exp.NewExpression);

            var memberBindings = exp.Bindings;
            for (var i = 0; i < memberBindings.Count; i++)
            {
                var memberBinding = memberBindings[i];

                switch (memberBinding)
                {
                    case MemberAssignment memberAssignment:
                        this.Visit(memberAssignment.Expression);
                        break;
                    case MemberListBinding memberListBinding:
                        for (int j = 0; j < memberListBinding.Initializers.Count; j++)
                        {
                            for (int k = 0; k < memberListBinding.Initializers[j].Arguments.Count; k++)
                            {
                                this.Visit(memberListBinding.Initializers[j].Arguments[k]);
                            }
                        }
                        break;
                    case MemberMemberBinding memberMemberBinding:

                        break;
                }
            }

        }
        protected virtual void VisitListInit(ListInitExpression exp)
        {
            this.Visit(exp.NewExpression);

            for (int i = 0; i < exp.Initializers.Count; i++)
            {
                for (int j = 0; j < exp.Initializers[i].Arguments.Count; j++)
                {
                    this.Visit(exp.Initializers[i].Arguments[j]);
                }

            }
        }

        #endregion



        #region QueryExpression

        public virtual void Visit(QueryExpression exp)
        {
            if (exp == null)
                return;

            if (exp.PrevExpression != null)
            {
                this.Visit(exp.PrevExpression);
            }

            switch (exp.NodeType)
            {
                case QueryExpressionType.Root:
                    this.VisitRootQuery((RootQueryExpression)exp);
                    return;
                case QueryExpressionType.Where:
                    this.VisitWhere((WhereExpression)exp);
                    return;
                case QueryExpressionType.Take:
                    this.VisitTake((TakeExpression)exp);
                    return;
                case QueryExpressionType.Skip:
                    this.VisitSkip((SkipExpression)exp);
                    return;
                case QueryExpressionType.Paging:
                    this.VisitPaging((PagingExpression)exp);
                    return;
                case QueryExpressionType.OrderBy:
                case QueryExpressionType.OrderByDesc:
                case QueryExpressionType.ThenBy:
                case QueryExpressionType.ThenByDesc:
                    this.VisitOrder((OrderExpression)exp);
                    return;
                case QueryExpressionType.Select:
                    this.VisitSelect((SelectExpression)exp);
                    return;
                case QueryExpressionType.Include:
                    this.VisitInclude((IncludeExpression)exp);
                    return;
                case QueryExpressionType.BindTwoWay:
                    this.VisitBindTwoWay((BindTwoWayExpression)exp);
                    return;
                case QueryExpressionType.Exclude:
                    this.VisitExclude((ExcludeExpression)exp);
                    return;
                case QueryExpressionType.Aggregate:
                    this.VisitAggregateQuery((AggregateQueryExpression)exp);
                    return;
                case QueryExpressionType.JoinQuery:
                    this.VisitJoinQuery((JoinQueryExpression)exp);
                    return;
                case QueryExpressionType.GroupingQuery:
                    this.VisitGroupingQuery((GroupingQueryExpression)exp);
                    return;
                case QueryExpressionType.Distinct:
                    this.VisitDistinct((DistinctExpression)exp);
                    return;
                case QueryExpressionType.IgnoreAllFilters:
                    this.VisitIgnoreAllFilters((IgnoreAllFiltersExpression)exp);
                    return;
                case QueryExpressionType.Tracking:
                    this.VisitTracking((TrackingExpression)exp);
                    return;
                default:
                    throw new NotSupportedException(exp.NodeType.ToString());
            }
        }

        protected virtual void VisitRootQuery(RootQueryExpression exp)
        {
            List<LambdaExpression> globalFilters = exp.GlobalFilters;
            for (int i = 0; i < globalFilters.Count; i++)
            {
                this.Visit(globalFilters[i]);
            }

            List<LambdaExpression> contextFilters = exp.ContextFilters;
            for (int i = 0; i < contextFilters.Count; i++)
            {
                this.Visit(contextFilters[i]);
            }
        }
        protected virtual void VisitWhere(WhereExpression exp)
        {
            this.Visit(exp.Predicate);
        }
        protected virtual void VisitSelect(SelectExpression exp)
        {
            this.Visit(exp.Selector);
        }
        protected virtual void VisitTake(TakeExpression exp)
        {

        }
        protected virtual void VisitSkip(SkipExpression exp)
        {

        }
        protected virtual void VisitOrder(OrderExpression exp)
        {
            this.Visit(exp.KeySelector);
        }
        protected virtual void VisitAggregateQuery(AggregateQueryExpression exp)
        {
            List<Expression> arguments = new List<Expression>(exp.Arguments.Count);

            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                this.Visit(exp.Arguments[i]);
            }
        }
        protected virtual void VisitJoinQuery(JoinQueryExpression exp)
        {
            this.Visit(exp.Selector);

            for (int i = 0; i < exp.JoinedQueries.Count; i++)
            {
                this.Visit(exp.JoinedQueries[i].Condition);
            }
        }
        protected virtual void VisitGroupingQuery(GroupingQueryExpression exp)
        {
            this.Visit(exp.Selector);

            for (int i = 0; i < exp.GroupKeySelectors.Count; i++)
            {
                this.Visit(exp.GroupKeySelectors[i]);
            }

            for (int i = 0; i < exp.HavingPredicates.Count; i++)
            {
                this.Visit(exp.HavingPredicates[i]);
            }

            List<GroupingQueryOrdering> orderings = new List<GroupingQueryOrdering>(exp.Orderings.Count);
            for (int i = 0; i < exp.Orderings.Count; i++)
            {
                this.Visit(exp.Orderings[i].KeySelector);
            }
        }
        protected virtual void VisitDistinct(DistinctExpression exp)
        {

        }
        protected virtual void VisitInclude(IncludeExpression exp)
        {
            this.ProcessNavigationNode(exp.NavigationNode);
        }
        protected virtual void VisitBindTwoWay(BindTwoWayExpression exp)
        {

        }
        protected virtual void VisitExclude(ExcludeExpression exp)
        {
            this.Visit(exp.Field);
        }
        protected virtual void VisitIgnoreAllFilters(IgnoreAllFiltersExpression exp)
        {

        }
        protected virtual void VisitTracking(TrackingExpression exp)
        {

        }
        protected virtual void VisitPaging(PagingExpression exp)
        {

        }
        void ProcessNavigationNode(NavigationNode navigationNode)
        {
            for (int i = 0; i < navigationNode.ExcludedFields.Count; i++)
            {
                this.Visit(navigationNode.ExcludedFields[i]);
            }

            this.Visit(navigationNode.Condition);

            List<LambdaExpression> globalFilters = navigationNode.GlobalFilters;
            for (int i = 0; i < globalFilters.Count; i++)
            {
                this.Visit(globalFilters[i]);
            }

            List<LambdaExpression> contextFilters = navigationNode.ContextFilters;
            for (int i = 0; i < contextFilters.Count; i++)
            {
                this.Visit(contextFilters[i]);
            }

            if (navigationNode.Next != null)
            {
                this.ProcessNavigationNode(navigationNode.Next);
            }
        }

        #endregion
    }
}
