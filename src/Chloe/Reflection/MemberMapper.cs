using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe.Reflection
{
    public delegate void MemberMapper(object instance, IDataReader dataReader, int ordinal);
}
