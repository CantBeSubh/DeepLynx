using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.interfaces;

public interface ITokenBusiness
{
    Task<string> CreateToken(string apiKey, string apiSecret, double? expirationMinutes);
    Task<TokenResponseDto> CreateApiKey(long userId, string? clientId = null);
    Task<ApiKey> GetApiKey(string apiKey);
    Task<bool> DeleteApiKey(long userId, string key);
    Task<List<string>> GetAllUserKeys(long userId);
    string HashApiSecret(string rawSecret);
    bool VerifyApiSecret(string providedKey, string storedHash);
    Task<bool> RevokeToken(string jti);
    Task<bool> IsTokenRevoked(string jti);
    Task<int> RevokeAllUserTokens(long userId);
}