using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Pudding.Core.Cache {
    public class RedisConnectionHelp {
        //系统自定义Key前缀
        public static readonly string SysCustomKey = "";

        //"127.0.0.1:6379,allowadmin=true
        //private static readonly string RedisConnectionString = ConfigurationManager.ConnectionStrings["redis"].ConnectionString;
        private static readonly object Locker = new object ();
        //private static ConnectionMultiplexer _instance;
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> ConnectionCache = new ConcurrentDictionary<string, ConnectionMultiplexer> ();

        /// <summary>
        /// 缓存获取
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer GetConnectionMultiplexer (string connectionString) {

            if (!ConnectionCache.ContainsKey (connectionString)) {
                lock (Locker) {
                    ConnectionCache[connectionString] = GetManager (connectionString);
                }
            }
            return ConnectionCache[connectionString];
        }

        private static ConnectionMultiplexer GetManager (string connectionString) {
            //connectionString = connectionString ?? RedisConnectionString;
            ConnectionMultiplexer connect = ConnectionMultiplexer.Connect (connectionString);

            //注册如下事件
            connect.ConnectionFailed += MuxerConnectionFailed;
            connect.ConnectionRestored += MuxerConnectionRestored;
            connect.ErrorMessage += MuxerErrorMessage;
            connect.ConfigurationChanged += MuxerConfigurationChanged;
            connect.HashSlotMoved += MuxerHashSlotMoved;
            connect.InternalError += MuxerInternalError;

            return connect;
        }

        #region 事件

        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConfigurationChanged (object sender, EndPointEventArgs e) {
            Console.WriteLine ("Configuration changed: " + e.EndPoint);
        }

        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerErrorMessage (object sender, RedisErrorEventArgs e) {
            Console.WriteLine ("ErrorMessage: " + e.Message);
        }

        /// <summary>
        /// 重新建立连接之前的错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionRestored (object sender, ConnectionFailedEventArgs e) {
            Console.WriteLine ("ConnectionRestored: " + e.EndPoint);
        }

        /// <summary>
        /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionFailed (object sender, ConnectionFailedEventArgs e) {
            Console.WriteLine ("重新连接：Endpoint failed: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : (", " + e.Exception.Message)));
        }

        /// <summary>
        /// 更改集群
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerHashSlotMoved (object sender, HashSlotMovedEventArgs e) {
            Console.WriteLine ("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
        }

        /// <summary>
        /// redis类库错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerInternalError (object sender, InternalErrorEventArgs e) {
            Console.WriteLine ("InternalError:Message" + e.Exception.Message);
        }

        #endregion 事件
    }
    public class RedisRepository : IDisposable {
        private readonly ConnectionMultiplexer _conn;
        public string CustomKey;

        #region 构造函数

        public RedisRepository (string readWriteHosts) {
            _conn = RedisConnectionHelp.GetConnectionMultiplexer (readWriteHosts);
        }
        #endregion

        #region 辅助方法

        private string AddSysCustomKey (string oldKey) {
            string prefixKey = CustomKey ?? RedisConnectionHelp.SysCustomKey;
            return prefixKey + oldKey;
        }

        private T Do<T> (Func<IDatabase, T> func) {
            IDatabase database = _conn.GetDatabase (); //DbNum);
            return func (database);
        }

        private string ConvertJson<T> (T value) {
            string result = value is string ? value.ToString () : JsonConvert.SerializeObject (value);
            return result;
        }

        private T ConvertObj<T> (RedisValue value) {
            if (typeof (string) == typeof (T)) {
                return (T) Convert.ChangeType (value, typeof (string));
            }

            return JsonConvert.DeserializeObject<T> (value);
        }

        private List<T> ConvetList<T> (RedisValue[] values) {
            List<T> result = new List<T> ();
            foreach (RedisValue item in values) {
                T model = ConvertObj<T> (item);
                result.Add (model);
            }
            return result;
        }

        private RedisKey[] ConvertRedisKeys (List<string> redisKeys) {
            return redisKeys.Select (redisKey => (RedisKey) redisKey).ToArray ();
        }
        #endregion

        #region string

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public bool StringSet (string key, string value, TimeSpan? expiry = default (TimeSpan?)) {
            key = AddSysCustomKey (key);
            return Do (db => db.StringSet (key, value, expiry));
        }

        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="keyValues">键值对</param>
        /// <returns></returns>
        public bool StringSet (List<KeyValuePair<RedisKey, RedisValue>> keyValues) {
            List<KeyValuePair<RedisKey, RedisValue>> newkeyValues =
                keyValues.Select (p => new KeyValuePair<RedisKey, RedisValue> (AddSysCustomKey (p.Key), p.Value)).ToList ();
            return Do (db => db.StringSet (newkeyValues.ToArray ()));
        }

        /// <summary>
        /// 保存一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool StringSet<T> (string key, T obj, TimeSpan? expiry = default (TimeSpan?)) {
            key = AddSysCustomKey (key);
            string json = ConvertJson (obj);
            return Do (db => db.StringSet (key, json, expiry));
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <returns></returns>
        public string StringGet (string key) {
            key = AddSysCustomKey (key);
            return Do (db => db.StringGet (key));
        }

        /// <summary>
        /// 获取多个Key
        /// </summary>
        /// <param name="listKey">Redis Key集合</param>
        /// <returns></returns>
        public RedisValue[] StringGet (List<string> listKey) {
            List<string> newKeys = listKey.Select (AddSysCustomKey).ToList ();
            return Do (db => db.StringGet (ConvertRedisKeys (newKeys)));
        }

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T StringGet<T> (string key) {
            key = AddSysCustomKey (key);
            return Do (db => ConvertObj<T> (db.StringGet (key)));
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double StringIncrement (string key, double val = 1) {
            key = AddSysCustomKey (key);
            return Do (db => db.StringIncrement (key, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double StringDecrement (string key, double val = 1) {
            key = AddSysCustomKey (key);
            return Do (db => db.StringDecrement (key, val));
        }

        #endregion 同步方法

        #region 同步方法 List

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRemove<T> (string key, T value) {
            key = AddSysCustomKey (key);
            Do (db => db.ListRemove (key, ConvertJson (value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> ListRange<T> (string key) {
            key = AddSysCustomKey (key);
            return Do (redis => {
                RedisValue[] values = redis.ListRange (key);
                return ConvetList<T> (values);
            });
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRightPush<T> (string key, T value) {
            key = AddSysCustomKey (key);
            Do (db => db.ListRightPush (key, ConvertJson (value)));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListRightPop<T> (string key) {
            key = AddSysCustomKey (key);
            return Do (db => {
                RedisValue value = db.ListRightPop (key);
                return ConvertObj<T> (value);
            });
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListLeftPush<T> (string key, T value) {
            key = AddSysCustomKey (key);
            Do (db => db.ListLeftPush (key, ConvertJson (value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListLeftPop<T> (string key) {
            key = AddSysCustomKey (key);
            return Do (db => {
                RedisValue value = db.ListLeftPop (key);
                return ConvertObj<T> (value);
            });
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long ListLength (string key) {
            key = AddSysCustomKey (key);
            return Do (redis => redis.ListLength (key));
        }

        #endregion 同步方法

        #region 异步方法 List
        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListRemoveAsync<T> (string key, T value) {
            key = AddSysCustomKey (key);
            return await Do (db => db.ListRemoveAsync (key, ConvertJson (value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> ListRangeAsync<T> (string key) {
            key = AddSysCustomKey (key);
            RedisValue[] values = await Do (redis => redis.ListRangeAsync (key));
            return ConvetList<T> (values);
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListRightPushAsync<T> (string key, T value) {
            key = AddSysCustomKey (key);
            return await Do (db => db.ListRightPushAsync (key, ConvertJson (value)));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> ListRightPopAsync<T> (string key) {
            key = AddSysCustomKey (key);
            RedisValue value = await Do (db => db.ListRightPopAsync (key));
            return ConvertObj<T> (value);
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListLeftPushAsync<T> (string key, T value) {
            key = AddSysCustomKey (key);
            return await Do (db => db.ListLeftPushAsync (key, ConvertJson (value)));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> ListLeftPopAsync<T> (string key) {
            key = AddSysCustomKey (key);
            RedisValue value = await Do (db => db.ListLeftPopAsync (key));
            return ConvertObj<T> (value);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> ListLengthAsync (string key) {
            key = AddSysCustomKey (key);
            return await Do (redis => redis.ListLengthAsync (key));
        }
        #endregion

        #region Hash

        #region 同步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashExists (string key, string dataKey) {
            key = AddSysCustomKey (key);
            return Do (db => db.HashExists (key, dataKey));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool HashSet<T> (string key, string dataKey, T t) {
            key = AddSysCustomKey (key);
            return Do (db => {
                string json = ConvertJson (t);
                return db.HashSet (key, dataKey, json);
            });
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashDelete (string key, string dataKey) {
            key = AddSysCustomKey (key);
            return Do (db => db.HashDelete (key, dataKey));
        }

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        public long HashDelete (string key, List<RedisValue> dataKeys) {
            key = AddSysCustomKey (key);
            //List<RedisValue> dataKeys1 = new List<RedisValue>() {"1","2"};
            return Do (db => db.HashDelete (key, dataKeys.ToArray ()));
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public T HashGet<T> (string key, string dataKey) {
            key = AddSysCustomKey (key);
            return Do (db => {
                string value = db.HashGet (key, dataKey);
                return ConvertObj<T> (value);
            });
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public double HashIncrement (string key, string dataKey, double val = 1) {
            key = AddSysCustomKey (key);
            return Do (db => db.HashIncrement (key, dataKey, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public double HashDecrement (string key, string dataKey, double val = 1) {
            key = AddSysCustomKey (key);
            return Do (db => db.HashDecrement (key, dataKey, val));
        }

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> HashKeys<T> (string key) {
            key = AddSysCustomKey (key);
            return Do (db => {
                RedisValue[] values = db.HashKeys (key);
                return ConvetList<T> (values);
            });
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<bool> HashExistsAsync (string key, string dataKey) {
            key = AddSysCustomKey (key);
            return await Do (db => db.HashExistsAsync (key, dataKey));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<bool> HashSetAsync<T> (string key, string dataKey, T t) {
            key = AddSysCustomKey (key);
            return await Do (db => {
                string json = ConvertJson (t);
                return db.HashSetAsync (key, dataKey, json);
            });
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<bool> HashDeleteAsync (string key, string dataKey) {
            key = AddSysCustomKey (key);
            return await Do (db => db.HashDeleteAsync (key, dataKey));
        }

        /// <summary>
        /// 移除hash中的多个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKeys"></param>
        /// <returns></returns>
        public async Task<long> HashDeleteAsync (string key, List<RedisValue> dataKeys) {
            key = AddSysCustomKey (key);
            //List<RedisValue> dataKeys1 = new List<RedisValue>() {"1","2"};
            return await Do (db => db.HashDeleteAsync (key, dataKeys.ToArray ()));
        }

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<T> HashGeAsync<T> (string key, string dataKey) {
            key = AddSysCustomKey (key);
            string value = await Do (db => db.HashGetAsync (key, dataKey));
            return ConvertObj<T> (value);
        }

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public async Task<double> HashIncrementAsync (string key, string dataKey, double val = 1) {
            key = AddSysCustomKey (key);
            return await Do (db => db.HashIncrementAsync (key, dataKey, val));
        }

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        public async Task<double> HashDecrementAsync (string key, string dataKey, double val = 1) {
            key = AddSysCustomKey (key);
            return await Do (db => db.HashDecrementAsync (key, dataKey, val));
        }

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> HashKeysAsync<T> (string key) {
            key = AddSysCustomKey (key);
            RedisValue[] values = await Do (db => db.HashKeysAsync (key));
            return ConvetList<T> (values);
        }

        #endregion 异步方法

        #endregion Hash

        #region SortedSet 有序集合

        #region 同步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        public bool SortedSetAdd<T> (string key, T value, double score) {
            key = AddSysCustomKey (key);
            return Do (redis => redis.SortedSetAdd (key, ConvertJson<T> (value), score));
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool SortedSetRemove<T> (string key, T value) {
            key = AddSysCustomKey (key);
            return Do (redis => redis.SortedSetRemove (key, ConvertJson (value)));
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SortedSetRangeByRank<T> (string key) {
            key = AddSysCustomKey (key);
            return Do (redis => {
                RedisValue[] values = redis.SortedSetRangeByRank (key);
                return ConvetList<T> (values);
            });
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SortedSetLength (string key) {
            key = AddSysCustomKey (key);
            return Do (redis => redis.SortedSetLength (key));
        }

        #endregion 同步方法

        #region 异步方法

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        public async Task<bool> SortedSetAddAsync<T> (string key, T value, double score) {
            key = AddSysCustomKey (key);
            return await Do (redis => redis.SortedSetAddAsync (key, ConvertJson<T> (value), score));
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<bool> SortedSetRemoveAsync<T> (string key, T value) {
            key = AddSysCustomKey (key);
            return await Do (redis => redis.SortedSetRemoveAsync (key, ConvertJson (value)));
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> SortedSetRangeByRankAsync<T> (string key) {
            key = AddSysCustomKey (key);
            RedisValue[] values = await Do (redis => redis.SortedSetRangeByRankAsync (key));
            return ConvetList<T> (values);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> SortedSetLengthAsync (string key) {
            key = AddSysCustomKey (key);
            return await Do (redis => redis.SortedSetLengthAsync (key));
        }

        #endregion 异步方法

        #endregion SortedSet 有序集合

        #region key

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public bool KeyDelete (string key) {
            key = AddSysCustomKey (key);
            return Do (db => db.KeyDelete (key));
        }

        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public long KeyDelete (List<string> keys) {
            List<string> newKeys = keys.Select (AddSysCustomKey).ToList ();
            return Do (db => db.KeyDelete (ConvertRedisKeys (newKeys)));
        }

        /// <summary>
        /// 判断key是否存储
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns></returns>
        public bool KeyExists (string key) {
            key = AddSysCustomKey (key);
            return Do (db => db.KeyExists (key));
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">就的redis key</param>
        /// <param name="newKey">新的redis key</param>
        /// <returns></returns>
        public bool KeyRename (string key, string newKey) {
            key = AddSysCustomKey (key);
            return Do (db => db.KeyRename (key, newKey));
        }

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool KeyExpire (string key, TimeSpan? expiry = default (TimeSpan?)) {
            key = AddSysCustomKey (key);
            return Do (db => db.KeyExpire (key, expiry));
        }

        #endregion key

        public void Dispose () {
            if (_conn != null) {
                _conn.Close ();
                _conn.Dispose ();
            }
        }
    }
}