using deeplynx.interfaces;

namespace deeplynx.business
{
    public class CacheBusiness
    {
        // Create singleton instance when the class is loaded
        private static readonly CacheBusiness _instance = new CacheBusiness();
        private ICacheBusiness Cache;

        // Private constructor to prevent instantiation from outside
        private CacheBusiness()
        {
            Cache = CacheFactory.CreateCache();
        }

        // Static property to provide access to the singleton instance
        public static CacheBusiness Instance => _instance;

        // Used only for testing to set the CacheService to a mocked version
        public void SetCacheService(ICacheBusiness cacheService)
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
}