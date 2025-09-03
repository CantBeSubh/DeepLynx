using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using deeplynx.interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace deeplynx.business;

public class MemoryCacheBusiness : ICacheBusiness
{
    private readonly IMemoryCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ConcurrentDictionary<string, bool> _keys;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheBusiness"/> class.
    /// </summary>
    public MemoryCacheBusiness()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _keys = new ConcurrentDictionary<string, bool>();
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
    public Task<T> Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var value) && value is string jsonString)
            return Task.FromResult(JsonSerializer.Deserialize<T>(jsonString, _jsonOptions));

        return Task.FromResult(default(T));
    }

    /// <summary>
    /// Operation to Set cache data with key value pair
    /// </summary>
    /// <param name="key">The Key name of the data to be cached</param>
    /// <param name="value">The value of the data to be cached</param>
    /// <param name="ttl">Time To Live(ttl)- The duration of time the data will be cached</param>
    /// <returns>bool based on set success</returns>
    public Task<bool> Set(string key, object value, TimeSpan? ttl = null)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions();
        _keys[key] = true;

        if (ttl.HasValue) cacheEntryOptions.SetAbsoluteExpiration(ttl.Value);

        _cache.Set(key, JsonSerializer.Serialize(value, _jsonOptions), cacheEntryOptions);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Operation to Set cache data with key value pair
    /// </summary>
    /// <param name="key">The Key name of the data to be cached</param>
    /// <param name="value">The value of the data to be cached</param>
    /// <param name="ttl">Time To Live(ttl)- The duration of time the data will be cached</param>
    /// <returns>bool based on set success</returns>
    public Task<bool> Set(string key, object value, int? ttl = null)
    {
        // Memory Cache only takes timespan ttl's not int. Will convert to timespan if int is provided
        return Set(key, value, ttl.HasValue ? TimeSpan.FromSeconds(ttl.Value) : null);
    }

    /// <summary>
    /// Operation to Delete cache data by matching key
    /// </summary>
    /// <param name="key">The Key name of the data to be cached</param>
    /// <returns>bool based on delete success</returns>
    public Task<bool> Delete(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.FromResult(true);
    }
    
    /// <summary>
    /// Operation to flush all existing data
    /// </summary>
    /// <returns>bool based on flush success</returns>
    public Task<bool> Flush()
    {
        foreach (var key in _keys.Keys) _cache.Remove(key);

        _keys.Clear();
        return Task.FromResult(true);
    }
}