using Chloe.Descriptors;
using Chloe.Extensions;
using System.Collections;
using System.Reflection;

namespace Chloe.Query.SplitQuery
{
    public class RootQueryExecutor : SplitQueryExecutor
    {
        QueryContext _queryContext;
        IList _entities;

        SplitQueryRootNode _queryNode;

        public RootQueryExecutor(QueryContext queryContext, SplitQueryRootNode queryNode, List<SplitQueryExecutor> navigationQueryExecutors) : base(navigationQueryExecutors)
        {
            this._queryContext = queryContext;
            this._queryNode = queryNode;
        }

        public override IEnumerable<object> Entities { get { return this._entities.AsGenericEnumerable(); } }

        public override int EntityCount { get { return this._entities.Count; } }

        public async Task Execute(bool @async)
        {
            //执行查询
            await this.ExecuteQuery(@async);

            //将实体填入对应 owner 的属性集合中
            this.ExecuteBackFill();
        }

        public override async Task ExecuteQuery(bool @async)
        {
            this._entities = await this.LoadEntities(@async);
            await base.ExecuteQuery(@async);
        }


        public override IQuery GetDependQuery(SplitQueryNode fromNode)
        {
            IQuery query = this.MakeQuery(true, true);

            TypeDescriptor entityTypeDescriptor = this._queryNode.ElementTypeDescriptor;
            var collectionPropertyDescriptor = entityTypeDescriptor.CollectionPropertyDescriptors.Where(a => a.ElementType == fromNode.ElementType).FirstOrDefault();

            PropertyInfo property;
            if (collectionPropertyDescriptor != null)
            {
                //thisNode:fromNode 的关系是 1:N
                ComplexPropertyDescriptor fromNodeElementDotOwner_Descriptor = fromNode.ElementTypeDescriptor.GetComplexPropertyDescriptorByPropertyType(this._queryNode.ElementType);

                property = fromNodeElementDotOwner_Descriptor.GetOtherSideProperty(entityTypeDescriptor);  //this.AssociatedKey
            }
            else
            {
                //thisNode:fromNode 的关系是 N:1
                ComplexPropertyDescriptor complexPropertyDescriptor = entityTypeDescriptor.GetComplexPropertyDescriptorByPropertyType(fromNode.ElementType);
                property = complexPropertyDescriptor.ForeignKeyProperty.Property;  //this.OwnerId
            }

            //a => a.Id | a => a.OwnerId
            var selector = ExpressionExtension.MakeMemberAccessLambda(entityTypeDescriptor.EntityType, property);
            query = query.Select(selector);
            return query;
        }


        async Task<IList> LoadEntities(bool @async)
        {
            IQuery query = this.MakeQuery(false, false);

            IList entities;
            if (@async)
            {
                entities = await query.ToListAsync();
            }
            else
            {
                entities = query.ToList();
            }

            return entities;
        }

        IQuery MakeQuery(bool ignoreOrder, bool ignoreIncludedNavigations)
        {
            SplitQueryRootNode queryNode = this._queryNode;

            IQuery query = this._queryContext.DbContextProvider.Query(queryNode.ElementType, queryNode.TableName, queryNode.Lock);

            if (queryNode.IsTrackingQuery)
            {
                query = query.AsTracking();
            }

            if (queryNode.IgnoreAllFilters)
            {
                query = query.IgnoreAllFilters();
            }

            if (queryNode.BindTwoWay)
            {
                query = query.BindTwoWay();
            }

            for (int i = 0; i < queryNode.Conditions.Count; i++)
            {
                query = query.Where(queryNode.Conditions[i]);
            }

            for (int i = 0; i < queryNode.ExcludedFields.Count; i++)
            {
                query = query.Exclude(queryNode.ExcludedFields[i]);
            }

            if (!ignoreIncludedNavigations)
            {
                query = IncludeNavigation(query, queryNode, false);
            }

            if (!ignoreOrder)
            {
                for (int i = 0; i < queryNode.Orderings.Count; i++)
                {
                    var ordering = queryNode.Orderings[i];
                    if (i == 0)
                    {
                        if (ordering.SortType == SortType.Asc)
                        {
                            query = query.OrderBy(ordering.KeySelector);
                        }
                        else
                        {
                            query = query.OrderByDesc(ordering.KeySelector);
                        }

                        continue;
                    }

                    if (ordering.SortType == SortType.Asc)
                    {
                        query = query.ThenBy(ordering.KeySelector);
                    }
                    else
                    {
                        query = query.ThenByDesc(ordering.KeySelector);
                    }
                }
            }


            /* 
             * 注：这里没处理 Skip 和 Take 先后问题。如果开发者先 Take 后 Skip，那么这里处理就是个 bug~，不过一般不会有人先 Take 后 Skip 吧？？
             */
            if (queryNode.Skip != null)
            {
                query = query.Skip(queryNode.Skip.Value);
            }

            if (queryNode.Take != null)
            {
                query = query.Take(queryNode.Take.Value);
            }

            return query;
        }

    }
}
