using Chloe.Data;
using System.Data;
using System.Threading.Tasks;

namespace Chloe.SQLite
{
    public class ChloeSQLiteCommand : DbCommandDecorator, IDbCommand, IDisposable
    {
        ChloeSQLiteConcurrentConnection _connection;
        ChloeSQLiteTransaction _transaction;

        public ChloeSQLiteCommand(ChloeSQLiteConcurrentConnection connection) : base(connection.PersistedConnection.CreateCommand())
        {
            this._connection = connection;
        }

        public ChloeSQLiteConcurrentConnection ConcurrentConnection { get { return this._connection; } }

        public IDbCommand PersistedDbCommand { get { return this.PersistedCommand; } }

        public override IDbConnection Connection
        {
            get
            {
                return this._connection;
            }
            set
            {
                ChloeSQLiteConcurrentConnection conn = (ChloeSQLiteConcurrentConnection)value;
                this._connection = conn;
                this.PersistedCommand.Connection = conn.PersistedConnection;
            }
        }

        public override IDbTransaction Transaction
        {
            get
            {
                return this._transaction;
            }
            set
            {
                ChloeSQLiteTransaction tran = (ChloeSQLiteTransaction)value;
                this._transaction = tran;
                this.PersistedCommand.Transaction = this._transaction.PersistedTransaction;
            }
        }

        public override int ExecuteNonQuery()
        {
            this._connection.RWLock.BeginWrite();
            try
            {
                return this.PersistedCommand.ExecuteNonQuery();
            }
            finally
            {
                this._connection.RWLock.EndWrite();
            }
        }
        public override Task<int> ExecuteNonQueryAsync()
        {
            //由于 ChloeSQLiteConcurrentConnection 使用了 ReaderWriterLockSlim 读写锁，所以所有异步方法实现改成调用同步方法，防止异步调用时线程切换后无法释放锁！
            return Task.FromResult(this.ExecuteNonQuery());
        }

        public override IDataReader ExecuteReader()
        {
            this._connection.RWLock.BeginRead();
            try
            {
                return new ChloeSQLiteDataReader(this.PersistedCommand.ExecuteReader(), this);
            }
            catch
            {
                this._connection.RWLock.EndRead();
                throw;
            }
        }
        public override IDataReader ExecuteReader(CommandBehavior behavior)
        {
            this._connection.RWLock.BeginRead();
            try
            {
                /* 不出异常的话要锁的释放留给 ChloeSQLiteDataReader 去执行 */
                return new ChloeSQLiteDataReader(this.PersistedCommand.ExecuteReader(behavior), this);
            }
            catch
            {
                /* 出异常的话要释放锁 */
                this._connection.RWLock.EndRead();
                throw;
            }
        }

        public override Task<IDataReader> ExecuteReaderAsync()
        {
            //由于 ChloeSQLiteConcurrentConnection 使用了 ReaderWriterLockSlim 读写锁，所以所有异步方法实现改成调用同步方法，防止异步调用时线程切换后无法释放锁！
            return Task.FromResult(this.ExecuteReader());
        }
        public override Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior)
        {
            //由于 ChloeSQLiteConcurrentConnection 使用了 ReaderWriterLockSlim 读写锁，所以所有异步方法实现改成调用同步方法，防止异步调用时线程切换后无法释放锁！
            return Task.FromResult(this.ExecuteReader(behavior));
        }


        public override object ExecuteScalar()
        {
            this._connection.RWLock.BeginRead();
            try
            {
                return this.PersistedCommand.ExecuteScalar();
            }
            finally
            {
                this._connection.RWLock.EndRead();
            }
        }
        public override Task<object> ExecuteScalarAsync()
        {
            //由于 ChloeSQLiteConcurrentConnection 使用了 ReaderWriterLockSlim 读写锁，所以所有异步方法实现改成调用同步方法，防止异步调用时线程切换后无法释放锁！
            return Task.FromResult(this.ExecuteScalar());
        }
    }
}
