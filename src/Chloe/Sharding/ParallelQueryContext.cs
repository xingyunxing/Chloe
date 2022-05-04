using System.Collections;

namespace Chloe.Sharding
{
    internal interface IParallelQueryContext : IDisposable
    {
        bool Canceled { get; }

        ISharedDbContextProviderPool GetDbContextProviderPool(IPhysicDataSource dataSource);
        void Cancel();
        void AfterExecuteCommand(object result);
    }

    internal class ParallelQueryContext : IParallelQueryContext
    {
        int _canceled;
        IShardingContext _shardingContext;
        List<ISharedDbContextProviderPool> _pools = new List<ISharedDbContextProviderPool>();

        public ParallelQueryContext(IShardingContext shardingContext)
        {
            this._shardingContext = shardingContext;
        }

        public ISharedDbContextProviderPool GetDbContextProviderPool(IPhysicDataSource dataSource)
        {
            var pool = this._shardingContext.GetDbContextProviderPool(dataSource);
            this._pools.Add(pool);
            return pool;
        }
        public bool Canceled { get { return this._canceled != 0; } }

        public void Cancel()
        {
            System.Threading.Interlocked.Increment(ref this._canceled);
        }

        public virtual void AfterExecuteCommand(object result)
        {

        }

        public void Dispose()
        {
            for (int i = 0; i < this._pools.Count; i++)
            {
                this._pools[i].Dispose();
            }
        }

        public static void LogQueryCanceled(bool canCancel)
        {
#if DEBUG
            if (canCancel)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("-----------------------------------query canceled-----------------------------------");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
#endif
        }
    }

    internal class UniqueDataParallelQueryContext : ParallelQueryContext
    {
        public UniqueDataParallelQueryContext(IShardingContext shardingContext) : base(shardingContext)
        {
        }

        public override void AfterExecuteCommand(object result)
        {
            if (result is IList list)
            {
                if (list.Count > 0)
                {
                    this.Cancel();
                }
            }
        }
    }

    internal class AnyQueryParallelQueryContext : ParallelQueryContext
    {
        public AnyQueryParallelQueryContext(IShardingContext shardingContext) : base(shardingContext)
        {
        }

        public override void AfterExecuteCommand(object result)
        {
            bool hasData = (bool)result;
            if (hasData)
            {
                this.Cancel();
            }
        }
    }
}
