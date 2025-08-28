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

        public RedisCacheBusiness(ConnectionMultiplexer connectionMultiplexer)
        {
            _redis = connectionMultiplexer;
            _db = _redis.GetDatabase();

            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }

        public async Task<T> Get<T>(string key)
        {
            var value = await _db.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
        }

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

        public async Task<bool> Delete(string key)
        {
            return await _db.KeyDeleteAsync(key);
        }

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