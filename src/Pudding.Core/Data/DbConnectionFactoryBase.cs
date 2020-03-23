using System.Collections.Concurrent;
using System.Data;

namespace Pudding.Core.Data
{
    public abstract class DbConnectionFactoryBase : IDbConnectionFactory
    {
        protected readonly ConcurrentDictionary<string, string> _connectionStrings;
        public DbConnectionFactoryBase(ConcurrentDictionary<string, string> connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }
        public abstract IDbConnection GetDbInstance(string name);
    }
}