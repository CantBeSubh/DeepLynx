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

    public MemoryCacheBusiness()
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
            return Task.FromResult(JsonSerializer.Deserialize<T>(jsonString, _jsonOptions));

        return Task.FromResult(default(T));
    }

    public Task<bool> Set(string key, object value, TimeSpan? ttl = null)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions();
        _keys[key] = true;

        if (ttl.HasValue) cacheEntryOptions.SetAbsoluteExpiration(ttl.Value);

        _cache.Set(key, JsonSerializer.Serialize(value, _jsonOptions), cacheEntryOptions);
        return Task.FromResult(true);
    }

    // Memory Cache only takes timespan ttl's not int. Will convert to timespan if int is provided
    public Task<bool> Set(string key, object value, int? ttl = null)
    {
        return Set(key, value, ttl.HasValue ? TimeSpan.FromSeconds(ttl.Value) : null);
    }

    public Task<bool> Delete(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        return Task.FromResult(true);
    }

    public Task<bool> Flush()
    {
        foreach (var key in _keys.Keys) _cache.Remove(key);

        _keys.Clear();
        return Task.FromResult(true);
    }

    public Task<bool> FlushByPattern(string pattern)
    {
        // In the case of memory cache, this method is the same as flush
        return Flush();
    }
}