namespace deeplynx.interfaces;

public interface ICacheBusiness
{
    Task<T> Get<T>(string key);
    Task<bool> Set(string key, object value, TimeSpan? ttl = null);
    Task<bool> Set(string key, object value, int? ttl = null);
    Task<bool> Delete(string key);
    Task<bool> Flush();
}
