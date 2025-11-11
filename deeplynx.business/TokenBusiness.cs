using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Context;
using deeplynx.interfaces;
using deeplynx.models;
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
        if (!VerifyApiSecret(apiSecret, apiKeyRecord.Secret))
            throw new UnauthorizedAccessException("Invalid API credentials");

        // 4. Verify the application exists if ApplicationId is present
        if (apiKeyRecord.ApplicationId.HasValue)
        {
            var oauthApp = _context.OauthApplications
                .FirstOrDefault(app => app.Id == apiKeyRecord.ApplicationId.Value && !app.IsArchived);
        
            if (oauthApp == null)
            {
                throw new InvalidOperationException(
                    $"OAuth application with ID '{apiKeyRecord.ApplicationId.Value}' not found or has been archived. Cannot create token.");
            }
        }

        // 5. Use the JWT signing secret
        var jwtSigningSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

        if (string.IsNullOrEmpty(jwtSigningSecret))
            throw new InvalidOperationException("JWT signing secret not configured");

        var secretCheck = jwtSigningSecret.Length < 32
            ? jwtSigningSecret.PadRight(32, '0')
            : jwtSigningSecret;

        var key = Encoding.UTF8.GetBytes(secretCheck);
        double expirationMinutes = expiration > 0 ? (double)expiration : 60;
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        // Generate a unique token identifier (jti) to store in DB
        var jti = Guid.NewGuid().ToString();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email.ToLower()),
            new Claim(JwtRegisteredClaimNames.Name, user.Email.ToLower()),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new Claim("apiKey", apiKey)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Store the token hash in the database for revocation tracking
        var tokenHash = HashToken(jti);
        _context.OauthTokens.Add(new OauthToken
        {
            TokenHash = tokenHash,
            UserId = user.Id,
            ApplicationId = apiKeyRecord.ApplicationId,
            ExpiresAt = DateTime.SpecifyKind(expiresAt, DateTimeKind.Unspecified),
            Revoked = false,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
        });
        _context.SaveChanges();

        return tokenString;
    }

    public async Task<bool> RevokeToken(string jti)
    {
        var tokenHash = HashToken(jti);
        var token = await _context.OauthTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (token == null)
            throw new KeyNotFoundException("Token not found");

        if (token.Revoked)
            return false; // Already revoked

        token.Revoked = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsTokenRevoked(string jti)
    {
        var tokenHash = HashToken(jti);
        var token = await _context.OauthTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (token == null)
        {
            return false;
        }

        return token.Revoked;
    }

    public async Task<int> RevokeAllUserTokens(long userId)
    {
        var tokens = await _context.OauthTokens
            .Where(t => t.UserId == userId && !t.Revoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Revoked = true;
        }

        await _context.SaveChangesAsync();
        return tokens.Count;
    }

    public async Task<ApiKey> GetApiKey(string key)
    {
            var apiKey = await _context.ApiKeys.FirstOrDefaultAsync(k => k.Key == key);
            if (apiKey == null)
            {
                throw new KeyNotFoundException($"Api Keypair with key {apiKey} not found");
            }
            
            return apiKey;
    }

    public TokenResponseDto CreateApiKey(long userId, string? clientId = null)
    {
        // Generate random key and secret
        string apiKey = KeyGenerator.GenerateKeyBase64();
        string apiSecret = KeyGenerator.GenerateKeyBase64();

        // Hash the SECRET
        string hashedSecret = HashApiKey(apiSecret);

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with id {userId} not found");
        }

        // Look up application by ClientId if provided
        long? applicationId = null;
        if (!string.IsNullOrEmpty(clientId))
        {
            var oauthApp = _context.OauthApplications
                .FirstOrDefault(app => app.ClientId == clientId && !app.IsArchived);
        
            if (oauthApp == null)
            {
                throw new KeyNotFoundException(
                    $"OAuth application with ClientId '{clientId}' not found or has been archived.");
            }
        
            applicationId = oauthApp.Id;
        }

        // Store the plaintext key + hashed secret
        _context.ApiKeys.Add(new ApiKey
        {
            Key = apiKey,
            UserId = user.Id,
            Secret = hashedSecret,
            ApplicationId = applicationId
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

    public bool VerifyApiSecret(string providedKey, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(providedKey, storedHash);
    }

    // Hash the JTI (token identifier) for storage
    private string HashToken(string jti)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(jti));
        return Convert.ToBase64String(hashBytes);
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