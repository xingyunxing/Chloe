using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.MySql
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
