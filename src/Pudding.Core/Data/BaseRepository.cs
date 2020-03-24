using Autofac.Features.Indexed;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Transactions;

namespace Pudding.Core.Data
{
    public abstract partial class BaseRepository : IDisposable
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DbType DbType { get; private set; }
        /// <summary>
        /// 连接名称
        /// </summary>
        public string ConnectionName { get; private set; }
        /// <summary>
        /// 获取数据库连接
        /// </summary>
        public IDbConnection Conn { protected get; set; }

        public IIndex<DbType, IDbConnectionFactory> Manager { protected get; set; }
        public BaseRepository(DbType dbType = DbType.MySql, string connectionName = Config.DEFAULT_DB_CONNECTION)
        {
            DbType = dbType;
            ConnectionName = connectionName;
        }

        /// <summary>
        /// 插入数据库通用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        protected long Insert<T>(T t)
        {
            PropertyInfo[] ps = t.GetType().GetProperties();
            List<string> @colms = new List<string>();
            List<string> @params = new List<string>();

            string tableName = GetTableName(t);
            bool isIdentity = false;
            foreach (PropertyInfo p in ps)
            {
                if (p.GetCustomAttributes(false).Count(o => o.GetType() == typeof(IgnoreAttribute)) > 0)
                {
                    continue;
                }

                KeyAttribute property = (KeyAttribute)p.GetCustomAttributes(false).FirstOrDefault(o => o.GetType() == typeof(KeyAttribute));
                if (property != null || !property.Identity)
                {
                    isIdentity = true;
                }

                switch (Type.GetTypeCode(p.PropertyType))
                {
                    case TypeCode.DateTime:
                        if (Convert.ToDateTime(p.GetValue(t, null)) > DateTime.MinValue)
                        {
                            @colms.Add(string.Format("{0}", p.Name));
                            @params.Add(string.Format("@{0}", p.Name));
                        };
                        break;
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        if (!p.GetValue(t, null).ToString().Equals("0"))
                        {
                            @colms.Add(string.Format("{0}", p.Name));
                            @params.Add(string.Format("@{0}", p.Name));
                        }
                        break;
                    default:
                        if ((property == null && p.GetValue(t, null) != null) || (property != null && !property.Identity))
                        {
                            @colms.Add(string.Format("{0}", p.Name));
                            @params.Add(string.Format("@{0}", p.Name));
                        }
                        break;
                }
            }
            string sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, string.Join(", ", @colms), string.Join(", ", @params));
            if (isIdentity)
            {
                switch (DbType)
                {
                    case DbType.MsSql:
                        sql += " SELECT CAST(SCOPE_IDENTITY() as int)";
                        break;
                    case DbType.MySql:
                        sql += ";select last_insert_id()";
                        break;
                }
            }

            return Conn.Query<long>(sql, t).First();
        }

        /// <summary>
        /// 更新通用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected int Update<T>(T entity)
        {
            PropertyInfo[] ps = entity.GetType().GetProperties();
            List<string> @params = new List<string>();

            string where = string.Empty;

            string tableName = GetTableName(entity);
            foreach (PropertyInfo p in ps)
            {
                if (p.GetCustomAttributes(false).Count(o => o.GetType() == typeof(IgnoreAttribute)) > 0)
                {
                    continue;
                }

                KeyAttribute property = (KeyAttribute)p.GetCustomAttributes(false).FirstOrDefault(o => o.GetType() == typeof(KeyAttribute));
                if (property != null)
                {
                    where = string.Format("{0}=@{0}", p.Name);
                    continue;
                }

                switch (Type.GetTypeCode(p.PropertyType))
                {
                    case TypeCode.DateTime:
                        if (Convert.ToDateTime(p.GetValue(entity, null)) > DateTime.MinValue)
                        {
                            @params.Add(string.Format("{0}=@{0}", p.Name));
                        };
                        break;
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                        if (!p.GetValue(entity, null).ToString().Equals("0"))
                        {
                            @params.Add(string.Format("{0}=@{0}", p.Name));
                        }
                        break;
                    default:
                        if (p.GetValue(entity, null) != null)
                        {
                            @params.Add(string.Format("{0}=@{0}", p.Name));
                        }
                        break;
                }
            }
            string sql = string.Format("update {0} set {1} where {2}", tableName, string.Join(", ", @params), where);
            return Conn.Execute(sql, entity);
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private static string GetTableName<T>(T entity)
        {
            string tableName = string.Empty;
            object[] objAttrs = entity.GetType().GetCustomAttributes(typeof(TableAttribute), true);
            if (objAttrs.Length > 0)
            {
                if (objAttrs[0] is TableAttribute attr)
                {
                    tableName = attr.Name;
                }
            }
            return tableName;
        }

        /// <summary>
        /// 事务语句统一执行
        /// </summary>
        /// <param name="ac"></param>
        /// <returns></returns>
        protected bool TransactionExecute(Action ac)
        {
            try
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    ac.Invoke();
                    ts.Complete();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 事务语句统一执行（有返回）
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="fun"></param>
        /// <returns></returns>
        protected T TransactionExecute<T>(Func<T> fun)
        {
            T result = default(T);
            try
            {
                using (TransactionScope ts = new TransactionScope())
                {
                    result = fun.Invoke();
                    ts.Complete();
                }
                return result;
            }
            catch
            {
                return result;
            }
        }

        public void Dispose()
        {
            if (Conn != null)
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
                Conn.Dispose();
            }
        }
    }
}