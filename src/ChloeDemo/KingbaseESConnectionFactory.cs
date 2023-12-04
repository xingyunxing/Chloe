using Chloe.Infrastructure;
using Kdbndp;
using System.Data;

namespace ChloeDemo
{
    public class KingbaseESConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connString = null;

        public KingbaseESConnectionFactory(string connString)
        {
            _connString = connString;
        }

        public IDbConnection CreateConnection()
        {
            return new KdbndpConnection(this._connString);
        }
    }
}