using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.interfaces;

public interface ITokenBusiness
{
    public Task<string> CreateToken(string secretKey, string apiKey, double? expirationMinutes);
    public Task<TokenResponseDto> CreateApiKey(long userId, string? clientId = null);
    public Task<ApiKey> GetApiKey(string apiKey);
    public Task<bool> DeleteApiKey(long userId, string key);
    Task<List<string>> GetAllUserKeys(long userId);
    public string HashApiSecret(string rawSecret);
    public bool VerifyApiSecret(string providedKey, string storedHash);
    public Task<bool> RevokeToken(string jti);
    public Task<bool> IsTokenRevoked(string jti);
    public Task<int> RevokeAllUserTokens(long userId);
}