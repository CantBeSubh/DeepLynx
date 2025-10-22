using deeplynx.datalayer.Models;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.interfaces;

public interface ITokenBusiness
{
    string CreateToken(string secretKey, string apiKey, double? expirationMinutes);
    public ApiKey GetApiKey(string apiKey);
    public TokenResponseDto CreateApiKey();
    public Task<bool> DeleteApiKey(long userId, string key);
    Task<List<string>> GetAllUserKeys(long userId);
    public string HashApiKey(string apiKey);
    public bool VerifyApiKey(string providedKey, string storedHash);
}