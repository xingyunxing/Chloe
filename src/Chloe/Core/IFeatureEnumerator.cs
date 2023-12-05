using Chloe.Reflection;
using Chloe.Threading.Tasks;
using System.Collections;

namespace Chloe
{
    internal interface IFeatureEnumerator<out T> : IAsyncEnumerator<T>, IEnumerator<T>
    {
    }

    abstract class FeatureEnumerator<T> : IFeatureEnumerator<T>
    {
        IFeatureEnumerator<T> _innerEnumerator;
        T _current;

        protected FeatureEnumerator()
        {
        }

        protected IFeatureEnumerator<T> InnerEnumerator { get { return this._innerEnumerator; } }

        public T Current => this._current;

        object IEnumerator.Current => this._current;

        public void Dispose()
        {
            this.Dispose(false).GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            await this.Dispose(true);
        }

        protected virtual async ValueTask Dispose(bool @async)
        {
            if (this._innerEnumerator == null)
                return;

            await this._innerEnumerator.Dispose(@async);
        }

        public bool MoveNext()
        {
            return this.MoveNext(false).GetResult();
        }

        public BoolResultTask MoveNextAsync()
        {
            return this.MoveNext(true);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        protected virtual Task<IFeatureEnumerator<T>> CreateEnumerator(bool @async)
        {
            throw new NotImplementedException();
        }
        protected virtual async BoolResultTask MoveNext(bool @async)
        {
            if (this._innerEnumerator == null)
            {
                this._innerEnumerator = await this.CreateEnumerator(@async);
            }

            bool hasNext = await this._innerEnumerator.MoveNext(@async);

            if (!hasNext)
            {
                this._current = default;
                return false;
            }

            this._current = this._innerEnumerator.GetCurrent();

            return true;
        }
    }

    class FeatureEnumeratorAdapter<T> : IFeatureEnumerator<T>
    {
        object _enumerator;
        T _current;

        public FeatureEnumeratorAdapter(object enumerator)
        {
            if (!(enumerator is IAsyncEnumerator<T>) && !(enumerator is IEnumerator<T>))
            {
                throw new ArgumentException();
            }

            this._enumerator = enumerator;
        }

        public T Current => this._current;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            if (this._enumerator is IDisposable disposable)
            {
                disposable.Dispose();
                return;
            }

            this.DisposeAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            if (this._enumerator is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync();
                return;
            }

            this.Dispose();
        }

        public bool MoveNext()
        {
            if (this._enumerator is IEnumerator<T> enumerator)
            {
                bool hasNext = enumerator.MoveNext();
                if (hasNext)
                    this._current = enumerator.Current;
                else
                    this._current = default;

                return hasNext;
            }

            return this.MoveNextAsync().GetAwaiter().GetResult();
        }

        public async BoolResultTask MoveNextAsync()
        {
            if (this._enumerator is IAsyncEnumerator<T> enumerator)
            {
                bool hasNext = await enumerator.MoveNextAsync();
                if (hasNext)
                    this._current = enumerator.Current;
                else
                    this._current = default;

                return hasNext;
            }

            return this.MoveNext();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
    class DynamicFeatureEnumerator : IFeatureEnumerator<object>
    {
        object _enumerator;
        object _current;

        bool _isAsyncEnumerator;
        MethodInvoker _moveNextAsyncInvoker;
        MemberGetter _getCurrentGetter;

        public DynamicFeatureEnumerator(object enumerator)
        {
            var asyncEnumeratorInterface = enumerator.GetType().GetInterface(typeof(IAsyncEnumerator<>).Name);
            if (asyncEnumeratorInterface != null)
            {
                this._isAsyncEnumerator = true;

                var moveNextMethod = asyncEnumeratorInterface.GetMethod(nameof(IAsyncEnumerator<int>.MoveNextAsync));
                this._moveNextAsyncInvoker = MethodInvokerContainer.Get(moveNextMethod);

                var currentProperty = asyncEnumeratorInterface.GetProperty(nameof(IAsyncEnumerator<int>.Current));
                this._getCurrentGetter = MemberGetterContainer.Get(currentProperty);
            }
            else
            {
                if (!(enumerator is IEnumerator))
                {
                    throw new ArgumentException();
                }
            }

            this._enumerator = enumerator;
        }

        public object Current => this._current;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
            this.Dispose(false).GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            await this.Dispose(true);
        }

        async ValueTask Dispose(bool @async)
        {
            if (@async)
            {
                if (this._enumerator is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
                else if (this._enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            else
            {
                if (this._enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                else if (this._enumerator is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
            }
        }

        public bool MoveNext()
        {
            if (this._enumerator is IEnumerator enumerator)
            {
                bool hasNext = enumerator.MoveNext();
                if (hasNext)
                    this._current = enumerator.Current;
                else
                    this._current = default;

                return hasNext;
            }

            return this.MoveNextAsync().GetResult();
        }

        public async BoolResultTask MoveNextAsync()
        {
            if (this._isAsyncEnumerator)
            {
                BoolResultTask task = (BoolResultTask)this._moveNextAsyncInvoker(this._enumerator);

                bool hasNext = await task;
                if (hasNext)
                    this._current = this._getCurrentGetter(this._enumerator);
                else
                    this._current = default;

                return hasNext;
            }

            return this.MoveNext();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    class NullFeatureEnumerator<T> : IFeatureEnumerator<T>
    {
        public static readonly NullFeatureEnumerator<T> Instance = new NullFeatureEnumerator<T>();

        public NullFeatureEnumerator()
        {
        }

        public T Current => default;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {

        }

        public async ValueTask DisposeAsync()
        {

        }

        public bool MoveNext()
        {
            return false;
        }

        public async BoolResultTask MoveNextAsync()
        {
            return false;
        }

        public void Reset()
        {

        }
    }
    class ScalarFeatureEnumerator<T> : IFeatureEnumerator<T>
    {
        T _result;
        bool _hasRead;

        public ScalarFeatureEnumerator(T result)
        {
            this._result = result;
        }

        public T Current => this._result;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {

        }

        public async ValueTask DisposeAsync()
        {

        }

        public bool MoveNext()
        {
            if (this._hasRead)
            {
                return false;
            }

            this._hasRead = true;
            return true;
        }

        public async BoolResultTask MoveNextAsync()
        {
            return this.MoveNext();
        }

        public void Reset()
        {

        }
    }

    class PreloadableFeatureEnumerator<T> : IFeatureEnumerator<T>
    {
        IFeatureEnumerator<T> _enumerator;
        bool _hasElement;
        bool _firstRead = true;
        bool _initialized;

        public PreloadableFeatureEnumerator(IFeatureEnumerator<T> enumerator)
        {
            this._enumerator = enumerator;
        }

        public T Current => (this._enumerator as IEnumerator<T>).Current;

        object IEnumerator.Current => this.Current;

        /// <summary>
        /// 调用 MoveNext 之前必须先调用此方法
        /// </summary>
        /// <returns></returns>
        public async ValueTask Initialize()
        {
            if (this._initialized)
                return;

            this._hasElement = await this._enumerator.MoveNextAsync();
            this._initialized = true;
        }

        public void Dispose()
        {
            this._enumerator.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await this._enumerator.DisposeAsync();
        }

        public bool MoveNext()
        {
            return this.MoveNext(false).GetResult();
        }

        public async BoolResultTask MoveNextAsync()
        {
            return await this.MoveNext(true);
        }
        async BoolResultTask MoveNext(bool @async)
        {
            if (this._firstRead)
            {
                this._firstRead = false;
                return this._hasElement;
            }

            return await this._enumerator.MoveNext(@async);
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    internal static class FeatureEnumeratorExtensions
    {
        public static T GetCurrent<T>(this IFeatureEnumerator<T> enumerator)
        {
            if (enumerator == null)
                return default;

            return (enumerator as IEnumerator<T>).Current;
        }

        public static async BoolResultTask MoveNext<T>(this IFeatureEnumerator<T> enumerator, bool @async)
        {
            var next = @async ? await enumerator.MoveNextAsync() : enumerator.MoveNext();
            return next;
        }
    }
}
