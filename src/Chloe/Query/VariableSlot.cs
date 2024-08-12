
#if !NET46 && !NETSTANDARD2

using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe.Query
{
    /// <summary>
    /// lambda 表达式树中的变量插槽
    /// </summary>
    public abstract class VariableSlot
    {
        protected VariableSlot(int index)
        {
            this.Index = index;
        }

        public int Index { get; private set; }
    }

    public class VariableSlot<TVariableType> : VariableSlot
    {
        public VariableSlot(int index) : base(index)
        {

        }

        public TVariableType Value { get; private set; }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(typeof(TVariableType));
            hash.Add(this.Index);
            return hash.ToHashCode();
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            if (obj == null)
                return false;

            if (obj.GetType() != this.GetType())
                return false;

            VariableSlot<TVariableType> slot = (VariableSlot<TVariableType>)obj;
            if (this.Index != slot.Index)
                return false;

            return true;
        }

    }
}

#endif