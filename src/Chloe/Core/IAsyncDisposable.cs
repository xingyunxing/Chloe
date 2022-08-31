#if NETFX
using System;

namespace System
{
    internal interface IAsyncDisposable
    {
        ValueTask DisposeAsync();
    }
}
#endif
