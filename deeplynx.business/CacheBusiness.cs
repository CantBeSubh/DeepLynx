using deeplynx.interfaces;

namespace deeplynx.business
{
    public class CacheBusiness
    {
        // Create singleton instance when the class is loaded
        private static readonly CacheBusiness _instance = new CacheBusiness();
        private ICacheBusiness Cache;
        
        /// <summary>
        /// Private constructor to prevent instantiation from outside class.
        /// </summary>
        private CacheBusiness()
        {
            Cache = CacheFactory.CreateCache();
        }
        
        /// <summary>
        /// Static property to provide access to the singleton instance.
        /// </summary>
        public static CacheBusiness Instance => _instance;

        /// <summary>
        /// Used only for testing to set the CacheService to a mocked version
        /// </summary>
        /// <param name="cacheService">The Cache object that is used</param>
        public void SetCacheService(ICacheBusiness cacheService)
        {
            Cache = cacheService;
        }
        
        /// <summary>
        /// Wrapper methods to expose Get cache operations
        /// </summary>
        /// <param name="key">The Key name of the cached data to be retrieved</param>
        /// <returns>Cached data object</returns>
        public Task<T> Get<T>(string key) => Cache.Get<T>(key);
        
        /// <summary>
        /// Wrapper methods to expose Set cache operations
        /// </summary>
        /// <param name="key">The Key name of the data to be cached</param>
        /// <param name="value">The value of the data to be cached</param>
        /// <param name="ttl">Time To Live(ttl)- The duration of time the data will be cached</param>
        /// <returns>bool based on set success</returns>
        public Task<bool> Set(string key, object value, TimeSpan? ttl = null) => Cache.Set(key, value, ttl);
        
        /// <summary>
        /// Wrapper methods to expose Set cache operations
        /// </summary>
        /// <param name="key">The Key name of the data to be cached</param>
        /// <param name="value">The value of the data to be cached</param>
        /// <param name="ttl">Time To Live(ttl)- The duration of time the data will be cached</param>
        /// <returns>bool based on set success</returns>
        public Task<bool> Set(string key, object value, int? ttl = null) => Cache.Set(key, value, ttl);
        
        /// <summary>
        /// Wrapper methods to expose Delete cache operations
        /// </summary>
        /// <param name="key">The Key name of the data to be cached</param>
        /// <returns>bool based on delete success</returns>
        public Task<bool> Delete(string key) => Cache.Delete(key);
        
        /// <summary>
        /// Wrapper methods to expose Flush cache operations
        /// </summary>
        /// <returns>bool based on flush success</returns>
        public Task<bool> Flush() => Cache.Flush();
        
        /// <summary>
        /// Wrapper methods to expose FlushByPattern cache operations
        /// </summary>
        /// <param name="pattern">The pattern by which matching cached data will be found</param>
        /// <returns>bool based on flush-by-pattern success</returns>
        public Task<bool> FlushByPattern(string pattern) => Cache.FlushByPattern(pattern);
    }
}