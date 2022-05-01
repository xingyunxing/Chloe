using Chloe.Query;
using Chloe.QueryExpressions;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingLimitQueryState : ShardingQueryStateBase
    {
        int _skipCount;
        int _takeCount;
        public ShardingLimitQueryState(ShardingQueryStateBase prevQueryState, int skipCount, int takeCount) : base(prevQueryState)
        {
            this.SkipCount = skipCount;
            this.TakeCount = takeCount;
        }

        public int SkipCount
        {
            get
            {
                return this._skipCount;
            }
            set
            {
                this.CheckInputCount(value, "skip");
                this._skipCount = value;
                this.QueryModel.Skip = this._skipCount;
            }
        }
        public int TakeCount
        {
            get
            {
                return this._takeCount;
            }
            set
            {
                this.CheckInputCount(value, "take");
                this._takeCount = value;
                this.QueryModel.Take = this._takeCount;
            }
        }
        void CheckInputCount(int count, string name)
        {
            if (count < 0)
            {
                throw new ArgumentException(string.Format("The {0} count could not less than 0.", name));
            }
        }

        public override IQueryState Accept(TakeExpression exp)
        {
            if (exp.Count < this.TakeCount)
                this.TakeCount = exp.Count;

            return this;
        }
    }
}
