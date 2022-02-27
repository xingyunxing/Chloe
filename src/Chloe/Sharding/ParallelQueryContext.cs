using System.Collections;

namespace Chloe.Sharding
{
    internal interface IParallelQueryContext : IDisposable
    {
        //IShareDbContextPool DbContextPool { get; }
        bool BeforeExecuteCommand();
        void AfterExecuteCommand(object result);
    }

    internal class ParallelQueryContext : IParallelQueryContext
    {
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
    }

    internal class UniqueDataParallelQueryContext : ParallelQueryContext
    {
        int _countHasQuery;

        public override bool BeforeExecuteCommand()
        {
            if (this._countHasQuery >= 1)
                return true;

            return false;
        }

        public override void AfterExecuteCommand(object result)
        {
            if (result is IList list)
            {
                System.Threading.Interlocked.Add(ref this._countHasQuery, list.Count);
            }
        }
    }
}
