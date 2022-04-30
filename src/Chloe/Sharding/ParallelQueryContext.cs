using System.Collections;

namespace Chloe.Sharding
{
    internal interface IParallelQueryContext : IDisposable
    {
        /// <summary>
        /// 返回结果是否可停止查询
        /// </summary>
        /// <returns></returns>
        bool BeforeExecuteCommand();
        void AfterExecuteCommand(object result);
    }

    internal class ParallelQueryContext : IParallelQueryContext
    {

        public static readonly Func<ParallelQueryContext> ParallelQueryContextFactory = () =>
        {
            return new ParallelQueryContext();
        };

        List<IDisposable> _managedResourceList = new List<IDisposable>();

        public ParallelQueryContext()
        {

        }

        public void AddManagedResource(IDisposable disposable)
        {
            this._managedResourceList.Add(disposable);
        }

        public virtual bool BeforeExecuteCommand()
        {
            return false;
        }
        public virtual void AfterExecuteCommand(object result)
        {

        }

        public void Dispose()
        {
            for (int i = 0; i < this._managedResourceList.Count; i++)
            {
                this._managedResourceList[i].Dispose();
            }
        }

        public static void LogQueryCanceled(bool canCancel)
        {
#if DEBUG
            if (canCancel)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("query canceled.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
#endif
        }
    }

    internal class UniqueDataParallelQueryContext : ParallelQueryContext
    {
        public static readonly Func<ParallelQueryContext> UniqueDataParallelQueryContextFactory = () =>
        {
            return new UniqueDataParallelQueryContext();
        };

        int _countHasQuery;

        public UniqueDataParallelQueryContext()
        {
        }

        public override bool BeforeExecuteCommand()
        {
            bool canCancel = this._countHasQuery >= 1;

            ParallelQueryContext.LogQueryCanceled(canCancel);

            return canCancel;
        }

        public override void AfterExecuteCommand(object result)
        {
            if (result is IList list)
            {
                System.Threading.Interlocked.Add(ref this._countHasQuery, list.Count);
            }
        }
    }

    internal class AnyQueryParallelQueryContext : ParallelQueryContext
    {
        public static readonly Func<ParallelQueryContext> AnyQueryParallelQueryContextFactory = () =>
        {
            return new AnyQueryParallelQueryContext();
        };

        bool _hasData;

        public AnyQueryParallelQueryContext()
        {
        }

        public override bool BeforeExecuteCommand()
        {
            bool canCancel = this._hasData;

            ParallelQueryContext.LogQueryCanceled(canCancel);

            return canCancel;
        }

        public override void AfterExecuteCommand(object result)
        {
            bool hasData = (bool)result;
            if (hasData)
            {
                this._hasData = hasData;
            }
        }
    }
}
