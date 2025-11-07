using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace deeplynx.business;

public class TokenBusiness : ITokenBusiness
{
    private readonly DeeplynxContext _context;

    public TokenBusiness(DeeplynxContext context)
    {
        _context = context;
    }

    public string CreateToken(string apiSecret, string apiKey, double? expiration)
    {
        // 1. Look up the API key record
        var apiKeyRecord = _context.ApiKeys
            .FirstOrDefault(x => x.Key == apiKey);

        if (apiKeyRecord == null)
            throw new KeyNotFoundException("API key not found");

        // 2. Get the User Email for the Token
        var user = _context.Users
            .FirstOrDefault(x => x.Id == apiKeyRecord.UserId);

        if (user == null)
            throw new InvalidOperationException("Associated user not found");

        // 3. Verify the PROVIDED plaintext secret against the STORED hash
        if (!VerifyApiKey(apiSecret, apiKeyRecord.Secret))
            throw new UnauthorizedAccessException("Invalid API credentials");

        // 4. Use the JWT signing secret
        var jwtSigningSecret = Config.Instance.JWT_SECRET_KEY;

        if (string.IsNullOrEmpty(jwtSigningSecret))
            throw new InvalidOperationException("JWT signing secret not configured");

        var secretCheck = jwtSigningSecret.Length < 32
            ? jwtSigningSecret.PadRight(32, '0')
            : jwtSigningSecret;

        var key = Encoding.UTF8.GetBytes(secretCheck);
        double expirationMinutes = expiration > 0 ? (double)expiration : 60;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email.ToLower()),
            new Claim(JwtRegisteredClaimNames.Name, user.Email.ToLower()),
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

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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
        // Generate random key and secret
        string apiKey = KeyGenerator.GenerateKeyBase64();
        string apiSecret = KeyGenerator.GenerateKeyBase64();

        // Hash the SECRET
        string hashedSecret = HashApiKey(apiSecret);

        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == UserContextStorage.Email.ToLower());

        if (user == null)
        {
            throw new KeyNotFoundException($"User with email {UserContextStorage.Email} not found");
        }

        // Store the plaintext key + hashed secret
        _context.ApiKeys.Add(new ApiKey
        {
            Key = apiKey,
            UserId = user.Id,
            Secret = hashedSecret
        });
        _context.SaveChanges();

        // Return: plaintext key + plaintext secret to user
        return new TokenResponseDto
        {
            apiKey = apiKey,
            apiSecret = apiSecret
        };
    }

    public async Task<bool> DeleteApiKey(long userId, string key)
    {
        var keyToRemove = await _context.ApiKeys.SingleOrDefaultAsync(k => k.Key == key && k.UserId == userId);

        if (keyToRemove == null)
            throw new KeyNotFoundException($"Key {key} not found.");

        _context.ApiKeys.Remove(keyToRemove);
        await _context.SaveChangesAsync();

        return true;
    }

    public string HashApiKey(string apiKey)
    {
        return BCrypt.Net.BCrypt.HashPassword(apiKey, workFactor: 12);
    }

    public bool VerifyApiKey(string providedKey, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(providedKey, storedHash);
    }

    async Task<List<string>> ITokenBusiness.GetAllUserKeys(long userId)
    {
        // Query for existing records (excluding archived)
        var userApiKeys = await _context.ApiKeys
            .Where(r => r.UserId == userId)
            .ToListAsync();
        // Send just the key, not the secret
        var keys = userApiKeys.Select(c => c.Key).ToList();
        return keys;
    }
}