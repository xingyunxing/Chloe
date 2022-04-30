using System.Data;

namespace Chloe
{
    /// <summary>
    /// 调用 ITransientTransaction.Dispose() 时，如果事务未提交，则会自动回滚
    /// </summary>
    public interface ITransientTransaction : IDisposable
    {
        IDbContext DbContext { get; }
        void Commit();
        void Rollback();
    }

    class TransientTransaction : ITransientTransaction
    {
        bool _disposed = false;
        bool _completed = false;

        DbContextFacade _dbContext;

        public TransientTransaction(DbContextFacade dbContext) : this(dbContext, null)
        {

        }

        public TransientTransaction(DbContextFacade dbContext, IsolationLevel? il)
        {
            this._dbContext = dbContext;
            this._dbContext.Butler.BeginTransaction(il);
        }
        public TransientTransaction(IDbContext dbContext) : this(dbContext, null)
        {

        }

        public TransientTransaction(IDbContext dbContext, IsolationLevel? il)
        {
            this.DbContext = dbContext;
            this.DbContext.Session.BeginTransaction(il);
        }
        public void Commit()
        {
            if (this._completed)
                return;

            if (this._dbContext.Butler.IsInTransaction)
                this._dbContext.Butler.CommitTransaction();

            this._completed = true;
        }

        public IDbContext DbContext { get; private set; }
        public IDbContextFacade DbContextFacade { get; }

        public void Rollback()
        {
            if (this._completed)
                return;

            if (this._dbContext.Butler.IsInTransaction)
                this._dbContext.Butler.RollbackTransaction();

            this._completed = true;
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            this.Rollback();
            this._disposed = true;
        }
    }
}
