using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.IdentityModel.Tokens;

namespace deeplynx.business;

public class TokenBusiness : ITokenBusiness
{
    private readonly DeeplynxContext _context;

    public TokenBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public string CreateToken(string secretKey, string apiKey, double? expiration)
    {
        if (VerifyApiKey(apiKey, secretKey))
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _context.ApiKeys.FirstOrDefault(x => x.Key == apiKey)?.Secret;
            if (string.IsNullOrEmpty(secret))
            {
                throw new KeyNotFoundException($"Api key not found");
            }

            // Use the actual secret from database with padding if needed
            var secretCheck = secret.Length < 32 ? secret.PadRight(32, '0') : secret;
            var key = Encoding.UTF8.GetBytes(secretCheck);
            double expirationMinutes = expiration > 0 ? (double)expiration: 60;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, UserContextStorage.Email.ToLower()),
                new Claim(JwtRegisteredClaimNames.Name, UserContextStorage.Email.ToLower()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64), 
                new Claim("apiKey", apiKey)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        return string.Empty;
    }

    public ApiKey GetApiKey(string apiKey)
    {
        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == UserContextStorage.Email);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with email {UserContextStorage.Email} not found");
            }
            var userApiKey = _context.ApiKeys.FirstOrDefault(k => k.Key == apiKey);
            return userApiKey;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public TokenResponseDto CreateApiKey()
    {
        string apiKey = KeyGenerator.GenerateKeyBase64();
        string secret = KeyGenerator.GenerateKeyBase64();
        string hashedKey = HashApiKey(apiKey);
        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == UserContextStorage.Email.ToLower());
        
        if (user == null)
        {
            throw new KeyNotFoundException($"User with email {UserContextStorage.Email} not found");
        }

        _context.ApiKeys.Add(new ApiKey
        {
            Key = apiKey,
            UserId = user.Id,
            Secret = secret
        });
        _context.SaveChanges();
        
        return new TokenResponseDto
        {
            apiKey = apiKey,
            apiSecret = hashedKey
        };
    }

    public string HashApiKey(string apiKey)
    {
        return BCrypt.Net.BCrypt.HashPassword(apiKey, workFactor: 12);
    }

    public bool VerifyApiKey(string providedKey, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(providedKey, storedHash);
    }
}