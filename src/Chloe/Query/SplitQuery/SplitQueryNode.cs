using Chloe.Descriptors;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query.SplitQuery
{
    public class SplitQueryNode
    {
        /// <summary>
        /// 如果是根节点，则表示查询的实体类型描述；
        /// 如果是 Complex 导航属性，则表示 Complex 属性的实体类型描述；
        /// 如果是 Collection 导航属性，则表示集合内元素的类型描述
        /// </summary>
        public TypeDescriptor ElementTypeDescriptor { get; set; }

        public Type ElementType { get { return this.ElementTypeDescriptor.EntityType; } }

        public LockType Lock { get; set; }

        public List<LambdaExpression> ExcludedFields { get; private set; } = new List<LambdaExpression>();
        public List<LambdaExpression> Conditions { get; set; } = new List<LambdaExpression>();

        public List<SplitQueryNavigationNode> IncludeNodes { get; set; } = new List<SplitQueryNavigationNode>();
        public bool IgnoreAllFilters { get; set; }
        public bool IsTrackingQuery { get; set; }
        public bool BindTwoWay { get; set; }
    }

    public class SplitQueryRootNode : SplitQueryNode
    {
        public string TableName { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public List<Ordering> Orderings { get; set; } = new List<Ordering>();

        public RootQueryExecutor CreateQueryExecutor(QueryContext queryContext)
        {
            RootQueryExecutor queryExecutor = new RootQueryExecutor(queryContext, this, new List<SplitQueryExecutor>(this.IncludeNodes.Count));
            for (int i = 0; i < this.IncludeNodes.Count; i++)
            {
                queryExecutor.NavigationQueryExecutors.Add(this.IncludeNodes[i].CreateQueryExecutor(queryContext, queryExecutor));
            }

            return queryExecutor;
        }
    }

    public class SplitQueryNavigationNode : SplitQueryNode
    {
        public SplitQueryNavigationNode()
        {

        }

        public SplitQueryNode Owner { get; set; }

        public PropertyDescriptor PropertyDescriptor { get; set; }

        public PropertyInfo Property
        {
            get
            {
                return this.PropertyDescriptor.Property;
            }
        }

        public bool IsCollectionNavigation
        {
            get
            {
                return this.PropertyDescriptor is CollectionPropertyDescriptor;
            }
        }

        public SplitQueryExecutor CreateQueryExecutor(QueryContext queryContext, SplitQueryExecutor ownerQueryExecutor)
        {
            SplitQueryExecutor queryExecutor;
            if (this.IsCollectionNavigation)
            {
                queryExecutor = new CollectionNavigationQueryExecutor(queryContext, this, ownerQueryExecutor, new List<SplitQueryExecutor>(this.IncludeNodes.Count));
            }
            else
            {
                queryExecutor = new ComplexNavigationQueryExecutor(queryContext, this, ownerQueryExecutor, new List<SplitQueryExecutor>(this.IncludeNodes.Count));
            }

            for (int i = 0; i < this.IncludeNodes.Count; i++)
            {
                queryExecutor.NavigationQueryExecutors.Add(this.IncludeNodes[i].CreateQueryExecutor(queryContext, queryExecutor));
            }

            return queryExecutor;
        }
    }

    public class Ordering
    {
        public LambdaExpression KeySelector { get; set; }
        public SortType SortType { get; set; }
    }

    public enum SortType
    {
        Asc,
        Desc
    }
}
