using System;
using Chloe.Annotations;

namespace ChloeDemo
{
    [Table("TestEntity")]
    public class TestEntity
    {
        [Column(IsPrimaryKey = true)]
        [AutoIncrement]
        public int Id { get; set; }
        public byte? F_Byte { get; set; }
        public Int16? F_Int16 { get; set; }
        public int? F_Int32 { get; set; }
        public long? F_Int64 { get; set; }
        public float? F_Float { get; set; }
        public double? F_Double { get; set; }
        public decimal? F_Decimal { get; set; }
        public bool? F_Bool { get; set; }
        public DateTime? F_DateTime { get; set; }
        public Guid? F_Guid { get; set; }
        public string F_String { get; set; }
        public Gender? F_Enum { get; set; }
    }
}
