using System.Text.Json;
using StackExchange.Redis;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;
using deeplynx.interfaces;

namespace deeplynx.business;

public class RedisCacheBusiness : ICacheBusiness
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheBusiness"/> class.
        /// </summary>
        public RedisCacheBusiness(ConnectionMultiplexer connectionMultiplexer)
        {
            _redis = connectionMultiplexer;
            _db = _redis.GetDatabase();

            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }

        /// <summary>
        /// Retrieves cached data matching the provided key
        /// </summary>
        /// <param name="key">The key of cached data</param>
        /// <returns>The matching Cached data </returns>
        public async Task<T> Get<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
        }

        /// <summary>
        /// Operation to Set cache data with key value pair
        /// </summary>
        /// <param name="key">The Key name of the data to be cached</param>
        /// <param name="value">The value of the data to be cached</param>
        /// <param name="ttl">Time To Live(ttl)- The duration of time the data will be cached</param>
        /// <returns>bool based on set success</returns>
        public async Task<bool> Set(string key, object value, TimeSpan? ttl = null)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            bool result;
            if (ttl.HasValue)
            {
                result = await _db.StringSetAsync(key, json, ttl);
            }
            else
            {
                result = await _db.StringSetAsync(key, json);
            }

            return result;
        }

        /// <summary>
        /// Operation to Set cache data with key value pair
        /// </summary>
        /// <param name="key">The Key name of the data to be cached</param>
        /// <param name="value">The value of the data to be cached</param>
        /// <param name="ttl">Time To Live(ttl)- The duration of time the data will be cached</param>
        /// <returns>bool based on set success</returns>
        public async Task<bool> Set(string key, object value, int? ttl = null)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            bool result;
            if (ttl.HasValue)
            {
                result = await _db.StringSetAsync(key, json, TimeSpan.FromSeconds(ttl.Value));
            }
            else
            {
                result = await _db.StringSetAsync(key, json);
            }

            return result;
        }

        /// <summary>
        /// Operation to Delete cache data by matching key
        /// </summary>
        /// <param name="key">The Key name of the data to be cached</param>
        /// <returns>bool based on delete success</returns>
        public async Task<bool> Delete(string key)
        {
            return await _db.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Operation to flush all existing data
        /// </summary>
        /// <returns>bool based on flush success</returns>
        public async Task<bool> Flush()
        {
            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                await server.FlushDatabaseAsync();
            }

            return true;
        }

        /// <summary>
        /// Operation to flush all existing data that matches the provided pattern
        /// </summary>
        /// <returns>bool based on flush success</returns>
        public async Task<bool> FlushByPattern(string pattern)
        {
            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(pattern: pattern + "*");
                foreach (var key in keys)
                {
                    await _db.KeyDeleteAsync(key);
                }
            }

            return true;
        }
    }