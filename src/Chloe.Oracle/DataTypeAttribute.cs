using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Oracle
{
    public class DataTypeAttribute : Attribute
    {
        public DataTypeAttribute()
        {
        }
        public DataTypeAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; set; }
    }
}
