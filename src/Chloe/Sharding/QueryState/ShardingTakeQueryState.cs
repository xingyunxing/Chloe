using Chloe.Query;
using Chloe.QueryExpressions;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingTakeQueryState : ShardingQueryStateBase
    {
        int _count;

        public ShardingTakeQueryState(ShardingQueryStateBase prevQueryState, TakeExpression exp) : base(prevQueryState)
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
                this.QueryModel.Take = this._count;
            }
        }
        void CheckInputCount(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("The take count could not less than 0.");
            }
        }

        public override IQueryState Accept(TakeExpression exp)
        {
            if (exp.Count < this.Count)
            {
                this.Count = exp.Count;
            }

            return this;
        }
    }
}
