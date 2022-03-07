using Chloe.Core.Visitors;
using Chloe.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding
{
    internal class ShardingTablePeeker : ExpressionVisitor<IEnumerable<RouteTable>>
    {
        public ShardingTablePeeker(IShardingContext shardingContext)
        {
            this.ShardingContext = shardingContext;
        }

        IShardingContext ShardingContext { get; set; }

        public static IEnumerable<RouteTable> Peek(Expression exp, IShardingContext shardingContext)
        {
            if (exp == null)
                return shardingContext.GetTables();

            ShardingTablePeeker peeker = new ShardingTablePeeker(shardingContext);
            return peeker.Visit(exp);
        }

        protected override IEnumerable<RouteTable> VisitExpression(Expression exp)
        {
            return this.ShardingContext.GetTables();
        }

        protected override IEnumerable<RouteTable> VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }

        IEnumerable<RouteTable> VisitComparison(BinaryExpression exp, ShardingOperator shardingOperator, ShardingOperator inversiveShardingOperator)
        {
            MemberInfo member = null;
            IShardingStrategy shardingStrategy = this.GetShardingStrategy(exp, out member);

            if (shardingStrategy != null)
            {
                //TODO: 考虑是否可以翻译成sql的情况
                // a.CreateTime == ???
                if (exp.Right.IsEvaluable())
                {
                    // a.CreateTime == dt
                    object value = exp.Right.Evaluate();
                    return shardingStrategy.GetTables(value, shardingOperator);
                }
            }

            shardingStrategy = this.GetShardingStrategy(exp.Right, out member);
            if (shardingStrategy != null)
            {
                // ??? == a.CreateTime
                if (exp.Left.IsEvaluable())
                {
                    // dt == a.CreateTime
                    object value = exp.Left.Evaluate();
                    return shardingStrategy.GetTables(value, inversiveShardingOperator);
                }
            }

            return base.VisitBinary(exp);
        }

        protected override IEnumerable<RouteTable> VisitBinary(BinaryExpression exp)
        {
            //TODO 考虑 Equal 方法
            switch (exp.NodeType)
            {
                case ExpressionType.LessThan:
                    return this.VisitComparison(exp, ShardingOperator.LessThan, ShardingOperator.GreaterThan);
                case ExpressionType.LessThanOrEqual:
                    return this.VisitComparison(exp, ShardingOperator.LessThanOrEqual, ShardingOperator.GreaterThanOrEqual);
                case ExpressionType.GreaterThan:
                    return this.VisitComparison(exp, ShardingOperator.GreaterThan, ShardingOperator.LessThan);
                case ExpressionType.GreaterThanOrEqual:
                    return this.VisitComparison(exp, ShardingOperator.GreaterThanOrEqual, ShardingOperator.LessThanOrEqual);
                case ExpressionType.Equal:
                    return this.VisitComparison(exp, ShardingOperator.Equal, ShardingOperator.Equal);
                case ExpressionType.NotEqual:
                    return this.VisitComparison(exp, ShardingOperator.NotEqual, ShardingOperator.NotEqual);
                default:
                    return base.VisitBinary(exp);
            }
        }

        protected override IEnumerable<RouteTable> VisitBinary_AndAlso(BinaryExpression exp)
        {
            return this.Visit(exp.Left).Intersect(this.Visit(exp.Right), RouteTableEqualityComparer.Instance).ToList();
        }
        protected override IEnumerable<RouteTable> VisitBinary_OrElse(BinaryExpression exp)
        {
            return this.Visit(exp.Left).Union(this.Visit(exp.Right), RouteTableEqualityComparer.Instance).ToList();
        }

        IShardingStrategy GetShardingStrategy(Expression exp, out MemberInfo member)
        {
            member = null;

            exp = exp.StripConvert();
            if (exp.NodeType != ExpressionType.MemberAccess)
            {
                return null;
            }

            var memberExp = exp as MemberExpression;
            if (memberExp.Expression.NodeType == ExpressionType.Parameter)
            {
                // a.CreateTime
                member = memberExp.Member;
                return this.ShardingContext.Route.GetShardingStrategy(member);
            }

            return null;
        }
    }

    public class RouteTableEqualityComparer : IEqualityComparer<RouteTable>
    {
        public static readonly RouteTableEqualityComparer Instance = new RouteTableEqualityComparer();

        RouteTableEqualityComparer()
        {

        }

        public bool Equals(RouteTable x, RouteTable y)
        {
            return x.Name == y.Name && x.Schema == y.Schema && x.DataSource.Name == y.DataSource.Name;
        }

        public int GetHashCode(RouteTable obj)
        {
            return $"{obj.Name}_{obj.Schema}_{obj.DataSource.Name}".GetHashCode();
            //return obj.GetHashCode();
        }
    }
}
