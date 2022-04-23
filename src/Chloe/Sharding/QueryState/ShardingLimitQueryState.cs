﻿using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingLimitQueryState : ShardingQueryStateBase
    {
        int _skipCount;
        int _takeCount;
        public ShardingLimitQueryState(ShardingQueryContext context, ShardingQueryModel queryModel, int skipCount, int takeCount)
            : base(context, queryModel)
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
