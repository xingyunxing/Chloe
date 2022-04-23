﻿using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;

namespace Chloe.Sharding.QueryState
{
    internal class ShardingSkipQueryState : ShardingQueryStateBase
    {
        int _count;

        public ShardingSkipQueryState(ShardingQueryContext context, ShardingQueryModel queryModel, int count) : base(context, queryModel)
        {
            this.Count = count;
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
            return new ShardingLimitQueryState(this.Context, this.QueryModel, this.Count, exp.Count);
        }
    }
}
