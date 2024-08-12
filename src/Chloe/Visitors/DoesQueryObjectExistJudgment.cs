using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe.Visitors
{
    /// <summary>
    /// 判断表达式树中是否存在 IQuery 对象
    /// </summary>
    public class DoesQueryObjectExistJudgment : ExpressionVisitor<bool>, IQueryExpressionVisitor<bool>
    {
        public static DoesQueryObjectExistJudgment Instance { get; } = new DoesQueryObjectExistJudgment();


        public static bool ExistsQueryObject(QueryExpression queryExpression)
        {
            return queryExpression.Accept(Instance);
        }

        public static bool ExistsQueryObject(Expression expression)
        {
            return Instance.Visit(expression);
        }


        #region Expression

        protected override bool VisitExpression(Expression exp)
        {
            if (exp == null)
                return default(bool);

            return Utils.IsIQueryType(exp.Type);
        }
        protected override bool VisitUnary(UnaryExpression exp)
        {
            if (exp == null)
                return default(bool);

            if (Utils.IsIQueryType(exp.Type))
                return true;

            return this.Visit(exp.Operand);
        }
        protected override bool VisitBinary(BinaryExpression exp)
        {
            if (exp == null)
                return default(bool);

            if (this.Visit(exp.Left))
                return true;

            if (this.Visit(exp.Right))
                return true;

            return false;
        }
        protected override bool VisitLambda(LambdaExpression exp)
        {
            if (this.VisitExpression(exp))
                return true;

            return this.Visit(exp.Body);
        }
        protected override bool VisitMemberAccess(MemberExpression exp)
        {
            if (this.VisitExpression(exp))
                return true;

            return this.Visit(exp.Expression);
        }
        protected override bool VisitConditional(ConditionalExpression exp)
        {
            if (this.VisitExpression(exp))
                return true;

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
            if (this.VisitExpression(exp))
                return true;

            if (this.Visit(exp.Object))
                return true;

            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                if (this.Visit(exp.Arguments[i]))
                    return true;
            }

            return false;
        }
        protected override bool VisitNew(NewExpression exp)
        {
            if (this.VisitExpression(exp))
                return true;

            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                if (this.Visit(exp.Arguments[i]))
                    return true;
            }

            return false;
        }
        protected override bool VisitNewArray(NewArrayExpression exp)
        {
            for (int i = 0; i < exp.Expressions.Count; i++)
            {
                if (this.Visit(exp.Expressions[i]))
                    return true;
            }

            return false;
        }
        protected override bool VisitMemberInit(MemberInitExpression exp)
        {
            if (this.VisitExpression(exp))
                return true;

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

        public bool Visit(RootQueryExpression exp)
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
        public bool Visit(WhereExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            if (this.Visit(exp.Predicate))
                return true;

            return false;
        }
        public bool Visit(SelectExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            if (this.Visit(exp.Selector))
                return true;

            return false;
        }
        public bool Visit(TakeExpression exp)
        {
            return exp.PrevExpression.Accept(this);
        }
        public bool Visit(SkipExpression exp)
        {
            return exp.PrevExpression.Accept(this);
        }
        public bool Visit(OrderExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            if (this.Visit(exp.KeySelector))
                return true;

            return false;
        }
        public bool Visit(AggregateQueryExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            List<Expression> arguments = new List<Expression>(exp.Arguments.Count);

            for (int i = 0; i < exp.Arguments.Count; i++)
            {
                if (this.Visit(exp.Arguments[i]))
                    return true;
            }

            return false;
        }
        public bool Visit(JoinQueryExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            if (this.Visit(exp.Selector))
                return true;

            for (int i = 0; i < exp.JoinedQueries.Count; i++)
            {
                if (this.Visit(exp.JoinedQueries[i].Condition))
                    return true;
            }

            return false;
        }
        public bool Visit(GroupingQueryExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

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
        public bool Visit(DistinctExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            return false;
        }
        public bool Visit(IncludeExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            if (this.ProcessNavigationNode(exp.NavigationNode))
                return true;

            return false;
        }
        public bool Visit(BindTwoWayExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            return false;
        }
        public bool Visit(ExcludeExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            if (this.Visit(exp.Field))
                return true;

            return false;
        }
        public bool Visit(IgnoreAllFiltersExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            return false;
        }
        public bool Visit(TrackingExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

            return false;
        }
        public bool Visit(PagingExpression exp)
        {
            if (exp.PrevExpression.Accept(this))
                return true;

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
