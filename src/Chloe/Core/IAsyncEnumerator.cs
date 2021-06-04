#if netfx
using System;
using BoolResultTask = System.Threading.Tasks.Task<bool>;

namespace System.Collections.Generic
{
    internal interface IAsyncEnumerator<out T> : IDisposable
    {
        T Current { get; }

        BoolResultTask MoveNextAsync();
    }
}
#endif
