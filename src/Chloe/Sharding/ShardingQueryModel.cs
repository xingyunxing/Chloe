using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Chloe.Extensions;

namespace Chloe.Sharding
{
    public class Ordering
    {
        /// <summary>
        /// 必须是属性成员，不然没办法在内存里计算排序值
        /// </summary>
        public MemberInfo Member { get; set; }
        public LambdaExpression KeySelector { get; set; }
        public bool Ascending { get; set; }
    }
    public enum OrderType
    {
        Asc,
        Desc
    }

    internal class ShardingQueryModel
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }

        public bool IgnoreAllFilters { get; set; }

        public List<LambdaExpression> Conditions { get; set; } = new List<LambdaExpression>();
        public List<Ordering> Orderings { get; set; } = new List<Ordering>();

        public List<LambdaExpression> GlobalFilters { get; set; } = new List<LambdaExpression>();
        public List<LambdaExpression> ContextFilters { get; set; } = new List<LambdaExpression>();

        //public void AppendCondition(LambdaExpression condition)
        //{
        //    if (this.Condition == null)
        //    {
        //        this.Condition = condition;
        //        return;
        //    }

        //    this.Condition.AndAlso(condition);
        //}
    }

    internal class DataQueryModel
    {
        public PhysicTable Table { get; set; }

        public int? Skip { get; set; }
        public int? Take { get; set; }

        public bool IgnoreAllFilters { get; set; }

        public List<LambdaExpression> Conditions { get; set; } = new List<LambdaExpression>();
        public List<Ordering> Orderings { get; set; } = new List<Ordering>();
        public LambdaExpression Selector { get; set; }
    }
}
