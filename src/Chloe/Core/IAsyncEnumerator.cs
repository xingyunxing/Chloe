#if NETFX
using System;

namespace System
{
    internal interface IAsyncEnumerator<out T> : IAsyncDisposable
    {
        T Current { get; }

        BoolResultTask MoveNextAsync();
    }
}
#endif
