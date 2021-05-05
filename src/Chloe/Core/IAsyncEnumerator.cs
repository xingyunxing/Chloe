using System;

#if netfx
using BoolResultTask = System.Threading.Tasks.Task<bool>;
#else
using BoolResultTask = System.Threading.Tasks.ValueTask<bool>;
#endif

namespace Chloe.Collections.Generic
{
    internal interface IAsyncEnumerator<out T> : IDisposable
    {
        T Current { get; }

        BoolResultTask MoveNext();
    }
}
