using Chloe.DbExpressions;
using Chloe.Extensions;
using Chloe.Query.Mapping;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query
{
    public class PrimitiveObjectModel : ObjectModelBase
    {
        public PrimitiveObjectModel(QueryOptions queryOptions, Type primitiveType, DbExpression exp) : base(queryOptions, primitiveType)
        {
            this.Expression = exp;
        }

        public override TypeKind TypeKind { get { return TypeKind.Primitive; } }
        public DbExpression Expression { get; private set; }

        public DbExpression NullChecking { get; set; }

        public override DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter)
        {
            Stack<MemberExpression> memberExpressions = ExpressionExtension.Reverse(memberExpressionDeriveParameter);

            if (memberExpressions.Count == 0)
                throw new Exception();

            DbExpression ret = this.Expression;

            foreach (MemberExpression memberExpression in memberExpressions)
            {
                MemberInfo member = memberExpression.Member;
                ret = new DbMemberAccessExpression(member, ret);
            }

            if (ret == null)
                throw new Exception(memberExpressionDeriveParameter.ToString());

            return ret;
        }

        public override IObjectActivatorCreator GenarateObjectActivatorCreator(List<DbColumnSegment> columns, HashSet<string> aliasSet)
        {
            int ordinal = ObjectModelHelper.AddColumn(columns, aliasSet, this.Expression);

            PrimitiveObjectActivatorCreator activatorCreator = new PrimitiveObjectActivatorCreator(this.ObjectType, ordinal);

            if (this.NullChecking != null)
                activatorCreator.CheckNullOrdinal = ObjectModelHelper.AddColumn(columns, aliasSet, this.NullChecking);

            return activatorCreator;
        }

        public override IObjectModel ToNewObjectModel(List<DbColumnSegment> columns, HashSet<string> aliasSet, DbTable table, DbMainTableExpression dependentTable)
        {
            DbColumnAccessExpression cae = ObjectModelHelper.ParseColumnAccessExpression(columns, aliasSet, table, this.Expression);

            PrimitiveObjectModel objectModel = new PrimitiveObjectModel(this.QueryOptions, this.ObjectType, cae);

            if (this.NullChecking != null)
                objectModel.NullChecking = ObjectModelHelper.AddNullCheckingColumn(columns, aliasSet, table, this.NullChecking);

            return objectModel;
        }

        public override void SetNullChecking(DbExpression exp)
        {
            if (this.NullChecking == null)
                this.NullChecking = exp;
        }
    }
}
