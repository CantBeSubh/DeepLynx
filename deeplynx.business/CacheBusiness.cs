using deeplynx.datalayer.Models;
using deeplynx.interfaces;
using deeplynx.helpers;

namespace deeplynx.business
{
    public class CacheBusiness : ICacheBusiness
    {
        private ICacheBusiness Cache;
        private Config _config;
    
        public CacheBusiness(Config config)
        {
            _config = config;
            Cache = CacheFactory.CreateCache(_config);
        }
    
        public string CacheType => Cache.CacheType;

        /// <summary>
        /// Used only for testing to set the CacheService to a mocked version
        /// </summary>
        /// <param name="cacheService">The Cache object that is used</param>
        public void SetCacheService(ICacheBusiness cacheService)
        {
            Cache = cacheService;
        }

        /// <summary>
        /// Reset the cache instance by recreating it from the factory.
        /// Useful for testing when environment variables change.
        /// </summary>
        public void ResetCacheInstance()
        {
            Cache = CacheFactory.CreateCache(_config);
        }

        /// <summary>
        /// Wrapper method to expose Get cache operations
        /// </summary>
        /// <param name="key">The Key name of the cached data to be retrieved</param>
        /// <returns>Cached data object</returns>
        public Task<T> GetAsync<T>(string key) => Cache.GetAsync<T>(key);

        /// <summary>
        /// Wrapper method to expose Set cache operations
        /// </summary>
        /// <param name="key">The Key name of the data to be cached</param>
        /// <param name="value">The value of the data to be cached</param>
        /// <param name="ttl">Time To Live(ttl)- The duration of time the data will be cached</param>
        /// <returns>bool based on set success</returns>
        public Task<bool> SetAsync(string key, object value, TimeSpan? ttl = null) => Cache.SetAsync(key, value, ttl);

        /// <summary>
        /// Wrapper method to expose Set cache operations
        /// </summary>
        /// <param name="key">The Key name of the data to be cached</param>
        /// <param name="value">The value of the data to be cached</param>
        /// <param name="ttl">Time To Live(ttl)- The duration of time the data will be cached</param>
        /// <returns>bool based on set success</returns>
        public Task<bool> SetAsync(string key, object value, int? ttl = null) => Cache.SetAsync(key, value, ttl);

        /// <summary>
        /// Wrapper method to expose Delete cache operations
        /// </summary>
        /// <param name="key">The Key name of the data to be cached</param>
        /// <returns>bool based on delete success</returns>
        public Task<bool> DeleteAsync(string key) => Cache.DeleteAsync(key);

        /// <summary>
        /// Wrapper method to expose Flush cache operations
        /// </summary>
        /// <returns>bool based on flush success</returns>
        public Task<bool> FlushAsync() => Cache.FlushAsync();
    }
}