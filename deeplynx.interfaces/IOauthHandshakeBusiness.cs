using deeplynx.models;

namespace deeplynx.interfaces;

public interface IOauthHandshakeBusiness
{
    Task<string> GenerateAuthCode(string clientId, long userId, string redirectUri, string state);
    Task<string> ExchangeAuthCodeForToken(
        string code, string clientId, string clientSecret, string redirectUri, string state, double? expiration);
}