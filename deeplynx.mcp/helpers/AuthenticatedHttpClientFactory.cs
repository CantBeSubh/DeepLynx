using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace deeplynx.mcp.helpers;

/// <summary>
/// Factory for creating HttpClient instances with automatic JWT authentication.
/// Extracts API key/secret from incoming MCP request headers and obtains a valid token.
/// </summary>
public interface IAuthenticatedHttpClientFactory
{
    /// <summary>
    /// Creates an HttpClient with a valid Bearer token attached.
    /// Token is automatically obtained/refreshed using the API key and secret
    /// from the incoming request headers.
    /// </summary>
    Task<HttpClient> CreateClientAsync();
}

public class AuthenticatedHttpClientFactory : IAuthenticatedHttpClientFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenHelper _tokenHelper;
    private readonly string _baseUrl;

    public AuthenticatedHttpClientFactory(
        IHttpContextAccessor httpContextAccessor,
        ITokenHelper tokenHelper)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenHelper = tokenHelper;
        _baseUrl = EnvironmentHelper.GetRequiredEnvironmentVariable("NEXUS_API_URL").TrimEnd('/') + "/";
    }

    public async Task<HttpClient> CreateClientAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new InvalidOperationException("No HTTP context available");
        }

        // Debug: Log all headers
        Console.WriteLine("=== Request Headers ===");
        foreach (var header in context.Request.Headers)
        {
            Console.WriteLine($"{header.Key}: {header.Value}");
        }
        Console.WriteLine("======================");

        var apiKey = context.Request.Headers["X-Nexus-Api-Key"].FirstOrDefault();
        var apiSecret = context.Request.Headers["X-Nexus-Api-Secret"].FirstOrDefault();
        
        Console.WriteLine($"ApiKey found: {!string.IsNullOrEmpty(apiKey)}");
        Console.WriteLine($"ApiSecret found: {!string.IsNullOrEmpty(apiSecret)}");

        var client = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new UnauthorizedAccessException(
                "Missing required headers: X-Nexus-Api-Key and X-Nexus-Api-Secret");
        }

        // Get a valid token (cached or fresh)
        var token = await _tokenHelper.GetValidTokenAsync(apiKey, apiSecret);
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}