using Chloe.Infrastructure;
using Chloe.QueryExpressions;
using Chloe.Visitors;
using Chloe.Descriptors;

namespace Chloe.Query.SplitQuery
{
    public class SplitQueryNodeCollector : ExpressionTraversal
    {
        SplitQueryRootNode _rootNode;

        public SplitQueryNodeCollector()
        {

        }

        public static SplitQueryRootNode GetSplitQueryNode(QueryExpression exp)
        {
            SplitQueryNodeCollector collector = new SplitQueryNodeCollector();
            collector.Visit(exp);

            SyncNodeInfo(collector._rootNode);

            return collector._rootNode;
        }

        static void SyncNodeInfo(SplitQueryNode queryNode)
        {
            for (int i = 0; i < queryNode.IncludeNodes.Count; i++)
            {
                var includeNode = queryNode.IncludeNodes[i];
                includeNode.Lock = queryNode.Lock;
                includeNode.IgnoreAllFilters = queryNode.IgnoreAllFilters;
                includeNode.IsTrackingQuery = queryNode.IsTrackingQuery;
                includeNode.BindTwoWay = queryNode.BindTwoWay;

                SyncNodeInfo(includeNode);
            }
        }

        protected override void VisitRootQuery(RootQueryExpression exp)
        {
            this._rootNode = new SplitQueryRootNode();
            this._rootNode.TableName = exp.ExplicitTable;
            this._rootNode.Lock = exp.Lock;
            this._rootNode.ElementTypeDescriptor = EntityTypeContainer.GetDescriptor(exp.ElementType);
        }

        protected override void VisitInclude(IncludeExpression exp)
        {
            NavigationNode navigationNode = exp.NavigationNode;
            SplitQueryNode queryNode = this._rootNode;
            while (navigationNode != null)
            {
                SplitQueryNavigationNode node = queryNode.IncludeNodes.Where(a => a.Property == navigationNode.Property).FirstOrDefault();
                if (node == null)
                {
                    PropertyDescriptor propertyDescriptor = queryNode.ElementTypeDescriptor.GetPropertyDescriptor(navigationNode.Property);
                    Type elementType = propertyDescriptor.PropertyType;
                    if (propertyDescriptor is CollectionPropertyDescriptor collectionPropertyDescriptor)
                    {
                        elementType = collectionPropertyDescriptor.ElementType;
                    }

                    node = new SplitQueryNavigationNode();
                    node.ElementTypeDescriptor = EntityTypeContainer.GetDescriptor(elementType);
                    node.PrevNode = queryNode;
                    node.PropertyDescriptor = propertyDescriptor;
                }

                node.ExcludedFields.AddRange(navigationNode.ExcludedFields);
                if (navigationNode.Condition != null)
                    node.Conditions.Add(navigationNode.Condition);

                queryNode.IncludeNodes.Add(node);
                navigationNode = navigationNode.Next;
                queryNode = node;
            }
        }

        protected override void VisitWhere(WhereExpression exp)
        {
            this._rootNode.Conditions.Add(exp.Predicate);
        }

        protected override void VisitExclude(ExcludeExpression exp)
        {
            this._rootNode.ExcludedFields.Add(exp.Field);
        }

        protected override void VisitOrder(OrderExpression exp)
        {
            if (exp.NodeType == QueryExpressionType.OrderBy)
            {
                this._rootNode.Orderings.Clear();
                this._rootNode.Orderings.Add(new Ordering() { KeySelector = exp.KeySelector, SortType = SortType.Asc });
                return;
            }

            if (exp.NodeType == QueryExpressionType.OrderByDesc)
            {
                this._rootNode.Orderings.Clear();
                this._rootNode.Orderings.Add(new Ordering() { KeySelector = exp.KeySelector, SortType = SortType.Desc });
                return;
            }

            if (exp.NodeType == QueryExpressionType.ThenBy)
            {
                this._rootNode.Orderings.Add(new Ordering() { KeySelector = exp.KeySelector, SortType = SortType.Asc });
                return;
            }

            if (exp.NodeType == QueryExpressionType.ThenByDesc)
            {
                this._rootNode.Orderings.Add(new Ordering() { KeySelector = exp.KeySelector, SortType = SortType.Desc });
                return;
            }
        }

        protected override void VisitPaging(PagingExpression exp)
        {
            int skipCount = (exp.PageNumber - 1) * exp.PageSize;
            int takeCount = exp.PageSize;

            this._rootNode.Skip = skipCount;
            this._rootNode.Take = takeCount;
        }

        protected override void VisitSkip(SkipExpression exp)
        {
            this._rootNode.Skip = exp.Count;
        }

        protected override void VisitTake(TakeExpression exp)
        {
            this._rootNode.Take = exp.Count;
        }

        protected override void VisitTracking(TrackingExpression exp)
        {
            this._rootNode.IsTrackingQuery = true;
        }

        protected override void VisitIgnoreAllFilters(IgnoreAllFiltersExpression exp)
        {
            this._rootNode.IgnoreAllFilters = true;
        }

        protected override void VisitBindTwoWay(BindTwoWayExpression exp)
        {
            this._rootNode.BindTwoWay = true;
        }

    }
}
