using Chloe.Infrastructure;
using Chloe.QueryExpressions;
using Chloe.Reflection;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query
{
    class IncludeQueryBase<TEntity, TItem> : Query<TEntity>
    {
        protected IncludeQueryBase(DbContextProvider dbContextProvider, QueryExpression exp) : base(dbContextProvider, exp)
        {

        }

        public IIncludedObjectQuery<TEntity, TProperty> ThenInclude<TProperty>(Expression<Func<TItem, TProperty>> navigationPath)
        {
            IncludeExpression includeExpression = this.BuildThenIncludeExpression(navigationPath);
            return new IncludedObjectQuery<TEntity, TProperty>(this.DbContextProvider, includeExpression);
        }

        public IIncludedCollectionQuery<TEntity, TCollectionItem> ThenIncludeMany<TCollectionItem>(Expression<Func<TItem, IEnumerable<TCollectionItem>>> navigationPath)
        {
            IncludeExpression includeExpression = this.BuildThenIncludeExpression(navigationPath);
            return new IncludedCollectionQuery<TEntity, TCollectionItem>(this.DbContextProvider, includeExpression);
        }


        protected IncludeExpression BuildThenIncludeExpression(LambdaExpression navigationPath)
        {
            IncludeExpression prevIncludeExpression = this.QueryExpression as IncludeExpression;
            NavigationNode startNavigation = prevIncludeExpression.NavigationNode.Clone();
            NavigationNode lastNavigation = startNavigation.GetLast();

            List<MemberExpression> memberExps = ExtractMemberAccessChain(navigationPath);

            for (int i = 0; i < memberExps.Count; i++)
            {
                PropertyInfo member = memberExps[i].Member as PropertyInfo;

                DbContextProvider dbContextProvider = this.DbContextProvider;
                NavigationNode navigation = InitNavigationNode(member, dbContextProvider);

                lastNavigation.Next = navigation;
                lastNavigation = navigation;
            }

            IncludeExpression includeExpression = new IncludeExpression(typeof(TEntity), prevIncludeExpression.PrevExpression, startNavigation);
            return includeExpression;
        }

        protected IncludeExpression BuildExcludeFieldIncludeExpression(LambdaExpression field)
        {
            IncludeExpression prevIncludeExpression = this.QueryExpression as IncludeExpression;
            NavigationNode startNavigation = prevIncludeExpression.NavigationNode.Clone();
            NavigationNode lastNavigation = startNavigation.GetLast();
            lastNavigation.ExcludedFields.Add(field);

            IncludeExpression includeExpression = new IncludeExpression(typeof(TEntity), prevIncludeExpression.PrevExpression, startNavigation);

            return includeExpression;
        }

        protected static List<MemberExpression> ExtractMemberAccessChain(LambdaExpression navigationPath)
        {
            List<MemberExpression> members = new List<MemberExpression>();

            Expression exp = navigationPath.Body;
            while (exp != null && exp.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression member = exp as MemberExpression;
                members.Add(member);
                exp = member.Expression;
            }

            if (exp != navigationPath.Parameters[0] || members.Count == 0)
            {
                throw new ArgumentException($"Not support inclue navigation path {navigationPath.Body.ToString()}");
            }

            members.Reverse();
            return members;
        }
        protected static QueryExpression BuildIncludeExpression(DbContextProvider dbContextProvider, QueryExpression prevExpression, LambdaExpression navigationPath)
        {
            List<MemberExpression> memberExps = ExtractMemberAccessChain(navigationPath);

            NavigationNode startNavigation = null;
            NavigationNode lastNavigation = null;
            for (int i = 0; i < memberExps.Count; i++)
            {
                PropertyInfo member = memberExps[i].Member as PropertyInfo;

                NavigationNode navigation = InitNavigationNode(member, dbContextProvider);

                if (startNavigation == null)
                {
                    startNavigation = navigation;
                    lastNavigation = navigation;
                    continue;
                }

                lastNavigation.Next = navigation;
                lastNavigation = navigation;
            }

            IncludeExpression ret = new IncludeExpression(typeof(TEntity), prevExpression, startNavigation);

            return ret;
        }
        protected static NavigationNode InitNavigationNode(PropertyInfo member, DbContextProvider dbContextProvider)
        {
            Type elementType = member.PropertyType;
            if (member.PropertyType.IsGenericCollection())
            {
                elementType = member.PropertyType.GetGenericArguments()[0];
            }

            var typeDescriptor = EntityTypeContainer.GetDescriptor(elementType);

            List<LambdaExpression> contextFilters = dbContextProvider.QueryFilters.FindValue(elementType);

            NavigationNode navigation = new NavigationNode(member, typeDescriptor.Definition.Filters.Count, contextFilters == null ? 0 : contextFilters.Count);

            navigation.GlobalFilters.AppendRange(typeDescriptor.Definition.Filters);
            if (contextFilters != null)
                navigation.ContextFilters.AppendRange(contextFilters);


            return navigation;
        }
    }
}
