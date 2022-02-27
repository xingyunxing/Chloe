using Chloe.Core.Visitors;
using Chloe.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Sharding
{
    internal class ShardingTablePeeker : ExpressionVisitor<List<PhysicTable>>
    {
        public ShardingTablePeeker(IShardingContext shardingContext)
        {
            this.ShardingContext = shardingContext;
        }

        IShardingContext ShardingContext { get; set; }

        public static List<PhysicTable> Peek(Expression exp, IShardingContext shardingContext)
        {
            if (exp == null)
                return shardingContext.GetPhysicTables();

            ShardingTablePeeker peeker = new ShardingTablePeeker(shardingContext);
            return peeker.Visit(exp);
        }

        protected override List<PhysicTable> VisitExpression(Expression exp)
        {
            return this.ShardingContext.GetPhysicTables();
        }

        protected override List<PhysicTable> VisitLambda(LambdaExpression exp)
        {
            return this.Visit(exp.Body);
        }

        List<PhysicTable> VisitComparison(BinaryExpression exp, ShardingOperator shardingOperator, ShardingOperator inversiveShardingOperator)
        {
            MemberInfo member = null;
            if (this.IsShardingMemberAccess(exp.Left, out member))
            {
                //TODO: 考虑是否可以翻译成sql的情况
                // a.CreateTime == ???
                if (exp.Right.IsEvaluable())
                {
                    // a.CreateTime == dt
                    object value = exp.Right.Evaluate();
                    return this.ShardingContext.GetPhysicTables(value, shardingOperator);
                }
            }

            if (this.IsShardingMemberAccess(exp.Right, out member))
            {
                // ??? == a.CreateTime
                if (exp.Left.IsEvaluable())
                {
                    // dt == a.CreateTime
                    object value = exp.Left.Evaluate();
                    return this.ShardingContext.GetPhysicTables(value, inversiveShardingOperator);
                }
            }

            //主键处理
            if (shardingOperator == ShardingOperator.Equal)
            {
                bool isPrimaryKeyMemberAccess = this.IsPrimaryKeyMemberAccess(exp.Left);
                Expression otherSideExp = exp.Right;

                if (!isPrimaryKeyMemberAccess)
                {
                    isPrimaryKeyMemberAccess = this.IsPrimaryKeyMemberAccess(exp.Right);
                    otherSideExp = exp.Left;
                }

                if (isPrimaryKeyMemberAccess)
                {
                    //TODO: 考虑 DateTime.Now 等可翻译情况
                    bool isEvaluable = otherSideExp.IsEvaluable();
                    if (isEvaluable)
                    {
                        var value = otherSideExp.Evaluate();
                        return this.ShardingContext.GetPhysicTableByKey(value);
                    }
                }
            }

            return base.VisitBinary(exp);
        }

        protected override List<PhysicTable> VisitBinary(BinaryExpression exp)
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

        protected override List<PhysicTable> VisitBinary_AndAlso(BinaryExpression exp)
        {
            return this.Visit(exp.Left).Intersect(this.Visit(exp.Right), PhysicTableEqualityComparer.Instance).ToList();
        }
        protected override List<PhysicTable> VisitBinary_OrElse(BinaryExpression exp)
        {
            return this.Visit(exp.Left).Union(this.Visit(exp.Right), PhysicTableEqualityComparer.Instance).ToList();
        }

        bool IsShardingMemberAccess(Expression exp, out MemberInfo member)
        {
            member = null;

            exp = exp.StripConvert();
            if (exp.NodeType != ExpressionType.MemberAccess)
            {
                return false;
            }

            var memberExp = exp as MemberExpression;
            if (memberExp.Expression.NodeType == ExpressionType.Parameter)
            {
                // a.CreateTime
                member = memberExp.Member;
                return this.ShardingContext.IsShardingMember(memberExp.Member);
            }

            return false;
        }
        bool IsPrimaryKeyMemberAccess(Expression exp)
        {
            exp = exp.StripConvert();
            if (exp.NodeType != ExpressionType.MemberAccess)
            {
                return false;
            }

            var memberExp = exp as MemberExpression;
            if (memberExp.Expression.NodeType == ExpressionType.Parameter)
            {
                // a.Id
                var member = memberExp.Member;
                return this.ShardingContext.IsPrimaryKey(member);
            }

            return false;
        }
    }

    public class PhysicTableEqualityComparer : IEqualityComparer<PhysicTable>
    {
        public static readonly PhysicTableEqualityComparer Instance = new PhysicTableEqualityComparer();

        PhysicTableEqualityComparer()
        {

        }

        public bool Equals(PhysicTable x, PhysicTable y)
        {
            return x.Name == y.Name && x.Schema == y.Schema && x.DataSource.Name == y.DataSource.Name;
        }

        public int GetHashCode(PhysicTable obj)
        {
            return $"{obj.Name}_{obj.Schema}_{obj.DataSource.Name}".GetHashCode();
            //return obj.GetHashCode();
        }
    }
}
