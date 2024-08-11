using System.Linq.Expressions;

namespace Chloe.QueryExpressions
{
    public class JoinQueryExpression : QueryExpression
    {
        List<JoinQueryInfo> _joinedQueries;
        LambdaExpression _selector;
        public JoinQueryExpression(Type elementType, QueryExpression prevExpression, List<JoinQueryInfo> joinedQueries, LambdaExpression selector) : base(QueryExpressionType.JoinQuery, elementType, prevExpression)
        {
            this._joinedQueries = new List<JoinQueryInfo>(joinedQueries.Count);
            this._joinedQueries.AppendRange(joinedQueries);
            this._selector = selector;
        }

        public List<JoinQueryInfo> JoinedQueries { get { return this._joinedQueries; } }
        public LambdaExpression Selector { get { return this._selector; } }

        public override T Accept<T>(IQueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class JoinQueryInfo
    {
        public JoinQueryInfo(IQuery query, JoinType joinType, LambdaExpression condition)
        {
            this.Query = query;
            this.JoinType = joinType;
            this.Condition = condition;
        }

        //TODO 去掉
        public IQuery Query { get; set; }
        public JoinType JoinType { get; set; }
        public LambdaExpression Condition { get; set; }
    }
}
