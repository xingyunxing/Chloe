using System.Data;

namespace Chloe
{
    public static class DbSessionExtension
    {
        public static void BeginTransaction(this IDbSession dbSession, IsolationLevel? il)
        {
            if (il == null)
            {
                dbSession.BeginTransaction();
                return;
            }

            dbSession.BeginTransaction(il.Value);
        }
    }
}
