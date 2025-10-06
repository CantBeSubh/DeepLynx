using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface ITokenBusiness
{
    string CreateToken(string secretKey, string apiKey, double? expirationMinutes);
    public ApiKey GetApiKey(string apiKey);
    public TokenResponseDto CreateApiKey();
    public string HashApiKey(string apiKey);
    public bool VerifyApiKey(string providedKey, string storedHash);
}