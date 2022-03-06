namespace System
{
    internal static class SystemExtension
    {
        public static async ValueTask Dispose(this object disposable, bool @async)
        {
            if (@async)
            {
                if (disposable is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                    return;
                }
            }

            ((IDisposable)disposable).Dispose();
        }
    }
}
