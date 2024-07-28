using Chloe.Data;
using System.Data;
using System.Threading.Tasks;

namespace Chloe.SQLite
{
    public class ChloeSQLiteDataReader : DataReaderDecorator, IDataReader, IDataRecord, IDisposable
    {
        ChloeSQLiteCommand _cmd;
        bool _hasReleaseLock = false;
        public ChloeSQLiteDataReader(IDataReader reader, ChloeSQLiteCommand cmd) : base(reader)
        {
            this._cmd = cmd;
        }

        ~ChloeSQLiteDataReader()
        {
            this.Dispose();
        }

        void ReleaseLock()
        {
            if (this._hasReleaseLock == false)
            {
                this._cmd.ConcurrentConnection.RWLock.EndRead();
                this._hasReleaseLock = true;
            }
        }

        public override BoolResultTask ReadAsync()
        {
            //由于 ChloeSQLiteConcurrentConnection 使用了 ReaderWriterLockSlim 读写锁，所以所有异步方法实现改成调用同步方法，防止异步调用时线程切换后无法释放锁！
#if NETFX
            return Task.FromResult(base.Read());
#else
            return new ValueTask<bool>(base.Read());
#endif
        }

        public override void Close()
        {
            this.PersistedReader.Close();
            this.ReleaseLock();
        }

        public override void Dispose()
        {
            try
            {
                if (this.PersistedReader != null)
                    this.PersistedReader.Dispose();
            }
            finally
            {
                this.ReleaseLock();
            }

            GC.SuppressFinalize(this);
        }
    }
}
