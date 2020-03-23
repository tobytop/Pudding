using System.Data;

namespace Pudding.Core.Data
{
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// 获取数据库连接实例
        /// </summary>
        /// <param name="name">数据库连接字符串</param>
        /// <returns></returns>
        IDbConnection GetDbInstance(string name);

    }
}