using System.Text.Json;
using System.Collections.Concurrent;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Memory;
using DotNetEnv;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace deeplynx.business
{
    public class CacheBusiness
    {
        // Create singleton instance when the class is loaded
        private static readonly CacheBusiness _instance = new CacheBusiness();
        private ICacheService Cache;

        // Private constructor to prevent instantiation from outside
        private CacheBusiness()
        {
            Env.Load("../.env");
            var cacheProviderType = Environment.GetEnvironmentVariable("CACHE_PROVIDER_TYPE");
            
            switch (cacheProviderType)
            {
                case "memory":
                    Cache = new MemoryCacheImpl();
                    break;
                case "redis":
                    var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
                    if (!string.IsNullOrEmpty(redisConnectionString))
                    {
                        ConfigurationOptions options = ConfigurationOptions.Parse(redisConnectionString);
                        options.AllowAdmin = true;
                        var connectionMultiplexer = ConnectionMultiplexer.Connect(options);
                        Cache = new RedisCacheImpl(connectionMultiplexer);
                    }
                    else
                    {
                        throw new Exception("Redis connection string not found in environment variables.");
                    }
                    break;
                default:
                    Cache = new MemoryCacheImpl();
                    break;
            }
        }

        // Static property to provide access to the singleton instance
        public static CacheBusiness Instance => _instance;
        
        // Used to set the CacheService to a mocked version in tests
        public void SetCacheService(ICacheService cacheService)
        {
            Cache = cacheService;
        }

        // Wrapper methods to expose cache operations
        public Task<T> Get<T>(string key) => Cache.Get<T>(key);
        public Task<bool> Set(string key, object value, TimeSpan? ttl = null) => Cache.Set(key, value, ttl);
        public Task<bool> Set(string key, object value, int? ttl = null) => Cache.Set(key, value, ttl);
        public Task<bool> Delete(string key) => Cache.Delete(key);
        public Task<bool> Flush() => Cache.Flush();
        public Task<bool> FlushByPattern(string pattern) => Cache.FlushByPattern(pattern);
    }

    public interface ICacheService
    {
        Task<T> Get<T>(string key);
        Task<bool> Set(string key, object value, TimeSpan? ttl = null);
        Task<bool> Set(string key, object value, int? ttl = null);
        Task<bool> Delete(string key);
        Task<bool> Flush();
        Task<bool> FlushByPattern(string pattern);
    }

    public class MemoryCacheImpl : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, bool> _keys;
        private readonly JsonSerializerOptions _jsonOptions;
        
        public MemoryCacheImpl()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _keys = new ConcurrentDictionary<string, bool>();
            _jsonOptions = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
        }

        public Task<T> Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var value) && value is string jsonString)
            {
                return Task.FromResult(JsonSerializer.Deserialize<T>(jsonString, _jsonOptions));
            }

            return Task.FromResult(default(T));
        }

        public Task<bool> Set(string key, object value, TimeSpan? ttl = null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions();
            _keys[key] = true;

            if (ttl.HasValue)
            {
                cacheEntryOptions.SetAbsoluteExpiration(ttl.Value);
            }

            _cache.Set(key, JsonSerializer.Serialize(value, _jsonOptions), cacheEntryOptions);
            return Task.FromResult(true);
        }
        
        // Memory Cache only takes timespan ttl's not int. Will convert to timespan if int is provided
        public Task<bool> Set(string key, object value, int? ttl = null)
        {
            return Set(key, value, ttl.HasValue ? TimeSpan.FromSeconds(ttl.Value) : (TimeSpan?)null);
        }

        public Task<bool> Delete(string key)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
            return Task.FromResult(true);
        }

        public Task<bool> Flush()
        {
            foreach (var key in _keys.Keys)
            {
                _cache.Remove(key);
            }

            _keys.Clear();
            return Task.FromResult(true);
        }

        public Task<bool> FlushByPattern(string pattern)
        {
            // In the case of memory cache, this method is the same as flush
            return Flush();
        }
    }

    public class RedisCacheImpl : ICacheService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheImpl(ConnectionMultiplexer connectionMultiplexer)
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
}
