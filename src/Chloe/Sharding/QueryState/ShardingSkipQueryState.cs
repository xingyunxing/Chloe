using Chloe.Query.QueryExpressions;
using Chloe.Query;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingSkipQueryState : ShardingQueryStateBase
    {
        int _count;

        public ShardingSkipQueryState(ShardingQueryStateBase prevQueryState, SkipExpression exp) : base(prevQueryState)
        {
            this.Count = exp.Count;
        }

        public int Count
        {
            get
            {
                return this._count;
            }
            set
            {
                this.CheckInputCount(value);
                this._count = value;
                this.QueryModel.Skip = this._count;
            }
        }
        void CheckInputCount(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("The skip count could not less than 0.");
            }
        }

        public override IQueryState Accept(SkipExpression exp)
        {
            if (exp.Count < 1)
            {
                return this;
            }

            this.Count += exp.Count;

            return this;
        }

        public override IQueryState Accept(TakeExpression exp)
        {
            return new ShardingLimitQueryState(this, this.Count, exp.Count);
        }
    }
}
