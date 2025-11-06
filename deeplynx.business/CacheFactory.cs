using StackExchange.Redis;
using deeplynx.interfaces;
using deeplynx.helpers;

namespace deeplynx.business
{
    public class CacheFactory
    {
        /// <summary>
        /// Used to determine what cache service to use by the Config.CACHE_PROVIDER_TYPE variable
        /// </summary>
        /// <returns>The Cache Business Object</returns>
        /// <exception cref="Exception">Returned if CACHE_PROVIDER_TYPE = redis but no REDIS_CONNECTION_STRING is provided</exception>
        public static ICacheBusiness CreateCache(Config config)
        {
            switch (config.CACHE_PROVIDER_TYPE)
            {
                case "memory":
                    return new MemoryCacheBusiness(config);
                case "redis":
                    if (!string.IsNullOrEmpty(config.REDIS_CONNECTION_STRING))
                    {
                        var options = ConfigurationOptions.Parse(config.REDIS_CONNECTION_STRING);
                        options.AllowAdmin = true;
                        var connectionMultiplexer = ConnectionMultiplexer.Connect(options);
                        return new RedisCacheBusiness(connectionMultiplexer, config);
                    }
                    else
                    {
                        throw new Exception("Redis connection string not found in environment variables.");
                    }
                default:
                    return new MemoryCacheBusiness(config);
            }
        }
    }
}