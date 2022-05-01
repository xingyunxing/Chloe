using Chloe.Core;
using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Extensions;
using Chloe.Infrastructure;
using Chloe.InternalExtensions;
using System.Data;
using System.Linq.Expressions;

namespace Chloe
{
    public abstract partial class DbContextProvider : IDisposable
    {
        protected DbCommandInfo Translate(DbExpression e)
        {
            IDbExpressionTranslator translator = this.DatabaseProvider.CreateDbExpressionTranslator();
            DbCommandInfo dbCommandInfo = translator.Translate(e);
            return dbCommandInfo;
        }
        protected Task<int> ExecuteNonQuery(DbExpression e, bool @async)
        {
            DbCommandInfo dbCommandInfo = this.Translate(e);
            return this.ExecuteNonQuery(dbCommandInfo, @async);
        }
        protected Task<int> ExecuteNonQuery(DbCommandInfo dbCommandInfo, bool @async)
        {
            return this.Session.ExecuteNonQuery(dbCommandInfo.CommandText, dbCommandInfo.GetParameters(), @async);
        }
        protected Task<object> ExecuteScalar(DbCommandInfo dbCommandInfo, bool @async)
        {
            return this.Session.ExecuteScalar(dbCommandInfo.CommandText, dbCommandInfo.GetParameters(), @async);
        }
        protected Task<IDataReader> ExecuteReader(DbExpression e, bool @async)
        {
            DbCommandInfo dbCommandInfo = this.Translate(e);
            return this.ExecuteReader(dbCommandInfo, @async);
        }
        protected Task<IDataReader> ExecuteReader(DbCommandInfo dbCommandInfo, bool @async)
        {
            return this.Session.ExecuteReader(dbCommandInfo.CommandText, dbCommandInfo.GetParameters(), @async);
        }
    }
}
