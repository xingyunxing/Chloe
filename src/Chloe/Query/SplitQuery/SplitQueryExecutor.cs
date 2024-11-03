using System.Linq.Expressions;

namespace Chloe.Query.SplitQuery
{
    public abstract class SplitQueryExecutor
    {
        List<SplitQueryExecutor> _navigationQueryExecutors;

        protected SplitQueryExecutor(List<SplitQueryExecutor> navigationQueryExecutors)
        {
            this._navigationQueryExecutors = navigationQueryExecutors;
        }

        public List<SplitQueryExecutor> NavigationQueryExecutors { get { return this._navigationQueryExecutors; } }

        public abstract IEnumerable<object> Entities { get; }

        public abstract int EntityCount { get; }

        public virtual async Task ExecuteQuery(bool @async)
        {
            for (int i = 0; i < this._navigationQueryExecutors.Count; i++)
            {
                await this._navigationQueryExecutors[i].ExecuteQuery(@async);
            }
        }

        public virtual void ExecuteBackFill()
        {
            for (int i = 0; i < this._navigationQueryExecutors.Count; i++)
            {
                this._navigationQueryExecutors[i].ExecuteBackFill();
            }
        }

        public abstract IQuery GetDependQuery(SplitQueryNode fromNode);

        public static IQuery IncludeNavigation(IQuery query, SplitQueryNode node, bool isThenInclude)
        {
            for (int i = 0; i < node.IncludeNodes.Count; i++)
            {
                SplitQueryNavigationNode includeNode = (SplitQueryNavigationNode)node.IncludeNodes[i];

                if (includeNode.IsCollectionNavigation)
                    continue;

                var a = Expression.Parameter(node.ElementType, "a");
                var includeProperty = Expression.MakeMemberAccess(a, includeNode.Property); //a.XXX
                var navigationProperty = Expression.Lambda(includeProperty, a); //a => a.XXX

                if (isThenInclude)
                {
                    query = query.ThenInclude(navigationProperty);
                }
                else
                {
                    query = query.Include(navigationProperty);
                }

                for (int j = 0; j < includeNode.Conditions.Count; j++)
                {
                    query = query.Filter(includeNode.Conditions[j]);
                }

                for (int j = 0; j < includeNode.ExcludedFields.Count; j++)
                {
                    query = query.ExcludeField(includeNode.ExcludedFields[j]);
                }

                query = IncludeNavigation(query, includeNode, true);
            }

            return query;
        }

    }
}
