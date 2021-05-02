using System;
using System.Collections;

#if netfx
using BoolResultTask = System.Threading.Tasks.Task<bool>;
#else
using BoolResultTask = System.Threading.Tasks.ValueTask<bool>;
#endif

namespace Chloe.Collections.Generic
{
    internal interface IAsyncEnumerator : IEnumerator
    {
        BoolResultTask MoveNextAsync();
    }
    internal interface IAsyncEnumerator<out T> : IAsyncEnumerator, IDisposable
    {
        new T Current { get; }
    }
}
