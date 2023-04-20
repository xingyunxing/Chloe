using Chloe.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChloeDemo
{
    public class Product
    {
        public long ID { get; set; }
        public string ProductName { get; set; }
        [Navigation]
        public virtual List<ProductTag>? RelationList { get; set; } = new();
    }
    public class ProductTag
    {
        public long ID { get; set; }
        public long ProductID { get; set; }
        [Navigation("ProductID")]
        public Product Product { get; set; }
        public long TagID { get; set; }
        [Navigation("TagID")]
        public Tag Tag { get; set; }
    }
    public class Tag
    {
        public long ID { get; set; }

        public string TagName { get; set; }
        [Navigation]
        public virtual List<ProductTag>? RelationList { get; set; } = new();
    }
}
