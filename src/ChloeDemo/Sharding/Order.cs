using System;
using System.Collections.Generic;
using System.Text;

namespace ChloeDemo.Sharding
{
    public class Order
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string UserId { get; set; } = "chloe";
        public decimal Amount { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
