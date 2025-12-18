using System.Net.Http.Json;
using System.Text.Json.Serialization;
using deeplynx.mcp.helpers;

namespace deeplynx.mcp.helpers;

public interface ITokenHelper
{
    Task<string> GetValidTokenAsync(string apiKey, string apiSecret);
}

public class TokenHelper : ITokenHelper
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, CachedToken> _tokenCache = new();
    private static readonly int tokenLifespan = 120;
    private static readonly TimeSpan ExpirationBuffer = TimeSpan.FromMinutes(5);

    public TokenHelper()
    {
        var baseUrl = EnvironmentHelper.GetRequiredEnvironmentVariable("NEXUS_API_URL");
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task<string> GetValidTokenAsync(string apiKey, string apiSecret)
    {
        // check if we have a cached token that is still valid
        if (_tokenCache.TryGetValue(apiKey, out var cached) && !IsTokenExpired(cached))
        {
            return cached.Token;
        }

        var newToken = await CreateTokenAsync(apiKey, apiSecret);
        _tokenCache[apiKey] = new CachedToken
        {
            Token = newToken,
            ExpiresAt = DateTime.UtcNow + TimeSpan.FromMinutes(tokenLifespan)
        };

        return newToken;
    }

    private async Task<string> CreateTokenAsync(string apiKey, string apiSecret)
    {
        var requestBody = new CreateTokenRequest
        {
            ApiKey = apiKey,
            ApiSecret = apiSecret,
            ExpirationMinutes = tokenLifespan
        };

        var response = await _httpClient.PostAsJsonAsync("oauth/tokens", requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Failed to create token: {response.StatusCode} - {errorContent}");
        }

        var token = await response.Content.ReadAsStringAsync();
        return token.Trim('"');
    }

    private bool IsTokenExpired(CachedToken cached)
    {
        // consider token expired if it's within the 5 minute buffer of actual expiration
        return DateTime.UtcNow >= cached.ExpiresAt - ExpirationBuffer;
    }

    private class CachedToken
    {
        public string Token {get; set;} = null!;
        public DateTime ExpiresAt {get; set;}
    }

    private class CreateTokenRequest
    {
        [JsonPropertyName("apiKey")]
        public string ApiKey { get; set; } = null!;
        
        [JsonPropertyName("apiSecret")]
        public string ApiSecret { get; set; } = null!;
        
        [JsonPropertyName("expirationMinutes")]
        public int? ExpirationMinutes { get; set; }
    }
}