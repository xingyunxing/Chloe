using Chloe.DbExpressions;
using Chloe.Query.Mapping;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query
{
    public interface IObjectModel
    {
        Type ObjectType { get; }
        TypeKind TypeKind { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columns">生成新的 ObjectActivatorCreator 的同时会将相应的列填充到 colums 集合中</param>
        /// <param name="aliasSet"></param>
        /// <returns></returns>
        IObjectActivatorCreator GenarateObjectActivatorCreator(List<DbColumnSegment> columns, HashSet<string> aliasSet);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columns">生成新的 ObjectModel 的同时会将相应的列填充到 colums 集合中</param>
        /// <param name="aliasSet"></param>
        /// <param name="table"></param>
        /// <param name="dependentTable"></param>
        /// <returns></returns>
        IObjectModel ToNewObjectModel(List<DbColumnSegment> columns, HashSet<string> aliasSet, DbTable table, DbMainTableExpression dependentTable);
        void AddConstructorParameter(ParameterInfo p, DbExpression primitiveExp);
        void AddConstructorParameter(ParameterInfo p, ComplexObjectModel complexModel);

        void AddPrimitiveMember(MemberInfo memberInfo, DbExpression exp);
        DbExpression GetPrimitiveMember(MemberInfo memberInfo);

        void AddComplexMember(MemberInfo memberInfo, ComplexObjectModel model);
        ComplexObjectModel GetComplexMember(MemberInfo memberInfo);

        void AddCollectionMember(MemberInfo memberInfo, CollectionObjectModel model);
        CollectionObjectModel GetCollectionMember(MemberInfo memberInfo);

        DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter);
        IObjectModel GetComplexMember(MemberExpression exp);

        /// <summary>
        /// 排除字段
        /// </summary>
        /// <param name="memberLink"></param>
        void ExcludePrimitiveMember(LinkeNode<MemberInfo> memberLink);
        /// <summary>
        /// 排除字段
        /// </summary>
        /// <param name="memberLinks"></param>
        void ExcludePrimitiveMembers(IEnumerable<LinkeNode<MemberInfo>> memberLinks);

        void SetNullChecking(DbExpression exp);
    }

    public static class ObjectModelHelper
    {
        public static DbExpression AddNullCheckingColumn(List<DbColumnSegment> columns, HashSet<string> aliasSet, DbTable table, DbExpression nullCheckingExp)
        {
            DbColumnSegment columnSeg = columns.Where(a => DbExpressionEqualityComparer.EqualsCompare(a.Body, nullCheckingExp)).FirstOrDefault();

            if (columnSeg == null)
            {
                string alias = Utils.GenerateUniqueColumnAlias(aliasSet);
                columnSeg = new DbColumnSegment(nullCheckingExp, alias);

                columns.Add(columnSeg);
            }

            DbColumnAccessExpression cae = new DbColumnAccessExpression(table, DbColumn.MakeColumn(columnSeg.Body, columnSeg.Alias));
            return cae;
        }

        public static int AddColumn(List<DbColumnSegment> columns, HashSet<string> aliasSet, DbExpression exp, string addDefaultAlias = UtilConstants.DefaultColumnAlias)
        {
            string alias = Utils.GenerateUniqueColumnAlias(aliasSet, true, addDefaultAlias);
            DbColumnSegment columnSeg = new DbColumnSegment(exp, alias);

            columns.Add(columnSeg);
            int ordinal = columns.Count - 1;

            return ordinal;
        }

        public static DbColumnAccessExpression ParseColumnAccessExpression(List<DbColumnSegment> columns, HashSet<string> aliasSet, DbTable table, DbExpression exp, string defaultAlias = UtilConstants.DefaultColumnAlias)
        {
            string alias = Utils.GenerateUniqueColumnAlias(aliasSet, true, defaultAlias);
            DbColumnSegment columnSeg = new DbColumnSegment(exp, alias);

            columns.Add(columnSeg);

            DbColumnAccessExpression cae = new DbColumnAccessExpression(table, DbColumn.MakeColumn(exp, alias));
            return cae;
        }
    }
}
