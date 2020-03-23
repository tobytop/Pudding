using MySql.Data.MySqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;

namespace Pudding.Core.Data
{
    public class MsSqlDbConnectionFactory : DbConnectionFactoryBase
    {
        public MsSqlDbConnectionFactory(ConcurrentDictionary<string, string> connectionStrings) : base(connectionStrings)
        { }
        public override IDbConnection GetDbInstance(string name)
        {
            return new SqlConnection(_connectionStrings[name]);
        }
    }
    public class MySqlDbConnectionFactory : DbConnectionFactoryBase
    {
        public MySqlDbConnectionFactory(ConcurrentDictionary<string, string> connectionStrings) : base(connectionStrings)
        { }
        public override IDbConnection GetDbInstance(string name)
        {
            return new MySqlConnection(_connectionStrings[name]);
        }
    }

    public enum DbType
    {
        None,
        MsSql,
        MySql
    }
}
