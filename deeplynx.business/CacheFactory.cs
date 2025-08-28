using StackExchange.Redis;
using DotNetEnv;
using deeplynx.interfaces;

namespace deeplynx.business
{
    public class CacheFactory
    {
        public static ICacheBusiness CreateCache()
        {
            Env.Load("../.env");
            var cacheProviderType = Environment.GetEnvironmentVariable("CACHE_PROVIDER_TYPE");

            switch (cacheProviderType)
            {
                case "memory":
                    return new MemoryCacheBusiness();
                case "redis":
                    var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
                    if (!string.IsNullOrEmpty(redisConnectionString))
                    {
                        var options = ConfigurationOptions.Parse(redisConnectionString);
                        options.AllowAdmin = true;
                        var connectionMultiplexer = ConnectionMultiplexer.Connect(options);
                        return new RedisCacheBusiness(connectionMultiplexer);
                    }
                    else
                    {
                        throw new Exception("Redis connection string not found in environment variables.");
                    }
                default:
                    return new MemoryCacheBusiness();
            }
        }
    }
}