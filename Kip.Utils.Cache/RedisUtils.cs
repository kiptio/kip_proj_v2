using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Kip.Utils.Cache
{
    public static class RedisUtils
    {
        // 推荐在命名redis的key的时候最好的加上前缀，并且使用 ：来分割前缀
        private static readonly string _prexKey = "test:";

        private static readonly string _conn = "127.0.0.1:6379";
        private static readonly int _dbNum = 5;

        private static ConnectionMultiplexer _redis;

        static RedisUtils()
        {
            _redis = ConnectionMultiplexer.Connect(_conn);

            // 注册事件
            _redis.ErrorMessage += (sender, e) => {
                Console.WriteLine("ErrorMessage: " + e.Message);
            };
        }
        private static IDatabase _redisDb
        {
            get { return _redis.GetDatabase(_dbNum); }
        }

        private static string AddKeyPrex(string key)
        {
            return _prexKey + key;
        }

        public static T Excute<T>(Func<IDatabase, T> fn)
        {
            return fn(_redisDb);
        }

        public static string StringGet(string key)
        {
            key = AddKeyPrex(key);
            return _redisDb.StringGet(key);
        }

        public static bool StringSet(string key, string value)
        {
            key = AddKeyPrex(key);
            return _redisDb.StringSet(key, value);
        }

        public static IEnumerable<string> HashValues(string key)
        {
            key = AddKeyPrex(key);
            return _redisDb.HashValues(key).Select(o => o.ToString());
        }

        public static bool HashSet(string key, string field, string value)
        {
            key = AddKeyPrex(key);
            return _redisDb.HashSet(key, field, value);
        }
    }
}
