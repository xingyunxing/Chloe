using Chloe.Dameng;
using Chloe.Infrastructure;
using Dm;
using System.Data;

namespace ChloeDemo
{
    public class DamengConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connString = null;

        public DamengConnectionFactory(string connString)
        {
            _connString = connString;
        }

        public IDbConnection CreateConnection()
        {
            return new DmConnection(this._connString);
        }
    }
}