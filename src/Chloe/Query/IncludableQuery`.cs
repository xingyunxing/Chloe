using Chloe.Extensions;
using Chloe.Infrastructure;
using Chloe.QueryExpressions;
using Chloe.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query
{
    class IncludableQuery<TEntity, TNavigation> : Query<TEntity>, IIncludableQuery<TEntity, TNavigation>
    {
        public IncludableQuery(DbContextProvider dbContextProvider, QueryExpression prevExpression, LambdaExpression navigationPath) : this(dbContextProvider, BuildIncludeExpression(dbContextProvider, prevExpression, navigationPath))
        {

        }
        IncludableQuery(DbContextProvider dbContextProvider, QueryExpression exp) : base(dbContextProvider, exp)
        {

        }

        static List<MemberExpression> ExtractMemberAccessChain(LambdaExpression navigationPath)
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
        static QueryExpression BuildIncludeExpression(DbContextProvider dbContextProvider, QueryExpression prevExpression, LambdaExpression navigationPath)
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
        static NavigationNode InitNavigationNode(PropertyInfo member, DbContextProvider dbContextProvider)
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

        IncludeExpression BuildThenIncludeExpression(LambdaExpression navigationPath)
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

        public IIncludableQuery<TEntity, TProperty> ThenInclude<TProperty>(Expression<Func<TNavigation, TProperty>> navigationPath)
        {
            IncludeExpression includeExpression = this.BuildThenIncludeExpression(navigationPath);
            return new IncludableQuery<TEntity, TProperty>(this.DbContextProvider, includeExpression);
        }

        public IIncludableQuery<TEntity, TCollectionItem> ThenIncludeMany<TCollectionItem>(Expression<Func<TNavigation, IEnumerable<TCollectionItem>>> navigationPath)
        {
            IncludeExpression includeExpression = this.BuildThenIncludeExpression(navigationPath);
            return new IncludableQuery<TEntity, TCollectionItem>(this.DbContextProvider, includeExpression);
        }

        public IIncludableQuery<TEntity, TNavigation> Filter(Expression<Func<TNavigation, bool>> predicate)
        {
            IncludeExpression prevIncludeExpression = this.QueryExpression as IncludeExpression;
            NavigationNode startNavigation = prevIncludeExpression.NavigationNode.Clone();
            NavigationNode lastNavigation = startNavigation.GetLast();
            lastNavigation.Condition = lastNavigation.Condition.AndAlso(predicate);

            IncludeExpression includeExpression = new IncludeExpression(typeof(TEntity), prevIncludeExpression.PrevExpression, startNavigation);

            return new IncludableQuery<TEntity, TNavigation>(this.DbContextProvider, includeExpression);
        }

        public IIncludableQuery<TEntity, TNavigation> ExcludeField<TField>(Expression<Func<TNavigation, TField>> field)
        {
            IncludeExpression prevIncludeExpression = this.QueryExpression as IncludeExpression;
            NavigationNode startNavigation = prevIncludeExpression.NavigationNode.Clone();
            NavigationNode lastNavigation = startNavigation.GetLast();
            lastNavigation.ExcludedFields.Add(field);

            IncludeExpression includeExpression = new IncludeExpression(typeof(TEntity), prevIncludeExpression.PrevExpression, startNavigation);

            return new IncludableQuery<TEntity, TNavigation>(this.DbContextProvider, includeExpression);
        }
    }
}
