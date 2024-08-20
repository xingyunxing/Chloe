using Chloe.Query.Internals;
using Chloe.QueryExpressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query
{
    partial class Query<T> : IQuery<T>, IQuery
    {
        static readonly List<Expression> EmptyArgumentList = new List<Expression>(0);

        FeatureEnumerable<T> GenerateIterator()
        {
            if (IsSplitQueryJudgment.IsSplitQuery(this.QueryExpression))
            {
                return new InternalSplitQuery<T>(this);
            }

            InternalQuery<T> internalQuery = new InternalQuery<T>(this);
            return internalQuery;
        }

        TResult ExecuteAggregateQuery<TResult>(MethodInfo method, Expression argument, bool checkArgument = true)
        {
            var q = this.CreateAggregateQuery<TResult>(method, argument, checkArgument);
            IEnumerable<TResult> iterator = q.GenerateIterator();
            return iterator.Single();
        }
        async Task<TResult> ExecuteAggregateQueryAsync<TResult>(MethodInfo method, Expression argument, bool checkArgument = true)
        {
            var q = this.CreateAggregateQuery<TResult>(method, argument, checkArgument);
            var iterator = q.GenerateIterator();
            return await iterator.SingleAsync();
        }

        Query<TResult> CreateAggregateQuery<TResult>(MethodInfo method, Expression argument, bool checkArgument)
        {
            if (checkArgument)
                PublicHelper.CheckNull(argument);

            List<Expression> arguments = argument == null ? EmptyArgumentList : new List<Expression>(1) { argument };
            var q = this.CreateAggregateQueryCore<TResult>(method, arguments);
            return q;
        }

        internal Query<TResult> CreateAggregateQueryCore<TResult>(MethodInfo method, List<Expression> arguments)
        {
            AggregateQueryExpression e = new AggregateQueryExpression(this._expression, method, arguments);
            var q = new Query<TResult>(this._dbContextProvider, e);
            return q;
        }
        MethodInfo GetCalledMethod<TResult>(Expression<Func<TResult>> exp)
        {
            var body = (MethodCallExpression)exp.Body;
            return body.Method;
        }

        public override string ToString()
        {
            IEnumerable<T> internalQuery = this.GenerateIterator();
            return internalQuery.ToString();
        }
    }
}
