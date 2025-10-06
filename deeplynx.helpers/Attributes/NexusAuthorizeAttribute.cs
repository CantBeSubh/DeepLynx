using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using deeplynx.datalayer.Models;
using deeplynx.helpers.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace deeplynx.helpers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class NexusAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    // Optional constructor to pass required scopes/roles later
    private readonly string[]? _requiredRoles;
    private readonly string[]? _requiredClaims;

    // Cache the configuration manager statically so we don't refetch every request.
    private static IConfigurationManager<OpenIdConnectConfiguration>? _configManager;
    private static readonly object _lock = new();

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (IsLocalDevelopmentBypassEnabled(context))
        {
            SetMockUserPrincipal(context.HttpContext);
            return;
        }

        try
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            var handler = new JwtSecurityTokenHandler();
            var token = ExtractToken(context.HttpContext.Request);
            if (string.IsNullOrEmpty(token))
            {
                SetUnauthorizedResult(context, "No token provided");
                return;
            }
            var jwtToken = handler.ReadJwtToken(token);
            var algorithm = jwtToken.Header.Alg;

            if (algorithm.StartsWith("RS"))
            {
                principal = ValidateRsaToken(token).GetAwaiter().GetResult();
            }
            else if (algorithm.StartsWith("HS"))
            {
                principal = ValidateShaToken(token, context.HttpContext).GetAwaiter().GetResult();
            }

            context.HttpContext.User = principal;

            // ensures the user exists in DB before UserContextMiddleware attempts to fetch User ID
            EnsureUserExists(context.HttpContext, principal).GetAwaiter().GetResult();

            // TODO: custom permission/role checks if you need them
        }
        catch (SecurityTokenExpiredException)
        {
            SetUnauthorizedResult(context, "Token has expired");
        }
        catch (Exception ex)
        {
            SetUnauthorizedResult(context, $"Authorization failed: {ex.Message}");
        }
    }

    private bool IsLocalDevelopmentBypassEnabled(AuthorizationFilterContext context)
    {
        var disableAuth = Environment.GetEnvironmentVariable("DISABLE_BACKEND_AUTHENTICATION")?.ToLower() == "true";
        
        return disableAuth;
    }

    private void SetMockUserPrincipal(HttpContext httpContext)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "LocalDeveloper"),
            new Claim(ClaimTypes.NameIdentifier, "local-dev-user"),
            new Claim("sub", "local-dev-user"),
            new Claim("email", "developer@localhost"),
            new Claim(ClaimTypes.Role, "Developer"),
        };

        var identity = new ClaimsIdentity(claims, "LocalDevelopment");
        var principal = new ClaimsPrincipal(identity);
        httpContext.User = principal;
    }

    private string? ExtractToken(HttpRequest request)
    {
        var authHeader = request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return authHeader["Bearer ".Length..];

        var tokenQuery = request.Query["token"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tokenQuery))
            return tokenQuery;

        // Ensure this matches whatever cookie you actually set
        var tokenCookie = request.Cookies["access_token"]; // or "jwt-token" if that's your real name
        return string.IsNullOrEmpty(tokenCookie) ? null : tokenCookie;
    }

    private async Task<ClaimsPrincipal> ValidateRsaToken(string token)
    {
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");   // e.g. "https://YOUR_OKTA_DOMAIN/oauth2/YOUR_AUTH_SERVER_ID"
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"); // e.g. "api://deeplynx"

        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("JWT_ISSUER not configured");
        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("JWT_AUDIENCE not configured");

        // Build (or reuse) an OpenID Connect configuration manager for this issuer
        var metadataAddress = $"{issuer.TrimEnd('/')}/.well-known/openid-configuration";
        if (_configManager == null)
        {
            lock (_lock)
            {
                _configManager ??= new ConfigurationManager<OpenIdConnectConfiguration>(
                    metadataAddress,
                    new OpenIdConnectConfigurationRetriever());
            }
        }

        var oidcConfig = await _configManager.GetConfigurationAsync(CancellationToken.None);

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidateIssuer = true,

            ValidAudience = audience,
            ValidateAudience = true,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),

            // RS256 signature validation using Okta's JWKS
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = oidcConfig.SigningKeys,

            // Extra safety: require a signed token and the expected algorithm
            RequireSignedTokens = true,
            ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 } // "RS256"
        };

        var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

        return principal;
    }

    private async Task<ClaimsPrincipal> ValidateShaToken(string token, HttpContext httpContext)
    {
        var handler = new JwtSecurityTokenHandler();

        var jwtToken = handler.ReadJwtToken(token);
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                       ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value
                       ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                       ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value
                       ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "username")?.Value;

        // Extract API key from claims
        var apiKey = jwtToken.Claims.FirstOrDefault(c => c.Type == "apiKey")?.Value;

        var secret = await GetSecretForUser(username, apiKey, httpContext);

        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException($"ApiKey is not valid - Username: {username}, ApiKey: {apiKey}");

        // Use the actual secret from database
        var secretKey = secret;

        // Ensure secret is long enough for HMAC-SHA256 (pad if necessary)
        if (secretKey.Length < 32)
        {
            secretKey = secretKey.PadRight(32, '0'); // Pad with zeros to meet minimum length
        }

        // Manually validate the JWT signature
        if (!ValidateJwtSignature(token, secretKey))
        {
            throw new SecurityTokenValidationException("JWT signature validation failed");
        }

        // Create claims principal from the validated token
        var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "JWT");
        var principal = new ClaimsPrincipal(claimsIdentity);

        return principal;


    }

    private bool ValidateJwtSignature(string token, string secret)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return false;

            var header = parts[0];
            var payload = parts[1];
            var signature = parts[2];

            // Create the signature from header and payload
            var message = $"{header}.{payload}";
            var keyBytes = Encoding.UTF8.GetBytes(secret);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                var computedSignature = Base64UrlEncode(computedHash);

                return computedSignature == signature;
            }
        }
        catch
        {
            return false;
        }
    }

    private string Base64UrlEncode(byte[] input)
    {
        var output = Convert.ToBase64String(input);
        output = output.Split('=')[0]; // Remove any trailing '='s
        output = output.Replace('+', '-'); // 62nd char of encoding
        output = output.Replace('/', '_'); // 63rd char of encoding
        return output;
    }

    private async Task<string?> GetSecretForUser(string username, string apiKeyValue, HttpContext httpContext)
    {
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(apiKeyValue))
        {
            var serviceScopeFactory = httpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DeeplynxContext>();
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == username.ToLower());
                if (user != null)
                {
                    var apiKey = await dbContext.ApiKeys.FirstOrDefaultAsync(a => a.Key == apiKeyValue && a.UserId == user.Id);
                    return apiKey?.Secret;
                }
            }
        }
        return null;
    }

    private void SetUnauthorizedResult(AuthorizationFilterContext context, string message)
    {
        context.Result = new JsonResult(new { error = message, status = 401 }) { StatusCode = 401 };
    }

    private void SetForbiddenResult(AuthorizationFilterContext context, string message)
    {
        context.Result = new JsonResult(new { error = message, status = 403 }) { StatusCode = 403 };
    }


    /// <summary>
    /// Insert user if not exists in DB; Update user if SSO ID not exists in DB
    /// </summary>
    /// <param name="httpContext">Request context</param>
    /// <param name="principal">SSO information</param>
    private async Task EnsureUserExists(HttpContext httpContext, ClaimsPrincipal principal)
    {
        try
        {
            // extract email from claims
            var email = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                // fall back to other common claim types
                email = principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal.FindFirst("email")?.Value
                    ?? principal.FindFirst("sub")?.Value;
            }

            if (string.IsNullOrEmpty(email))
            {
                // No email found - can't provision user
                return;
            }

            // Extract additional profile info for insertion into DB
            var ssoId = principal.FindFirst("sub")?.Value             // okta user ID
                ?? principal.FindFirst("cid")?.Value 
                ?? principal.FindFirst("uid")?.Value;  
            var username = principal.FindFirst("preferred_username")?.Value // use email if username not found
                ?? email;  
            var name = principal.FindFirst(ClaimTypes.Name)?.Value          // use username if name not found
                ?? principal.FindFirst("name")?.Value
                ?? username;                                                            

            // get DB context
            var serviceScopeFactory = httpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DeeplynxContext>();

                // TODO: see if this check can be performed as a cache function to avoid extraneous DB round trip
                // check if user already exists
                var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (existingUser != null)
                {
                    // user exists- check if SSO ID needs to be populated (and if it can be with claims info)
                    if (string.IsNullOrEmpty(existingUser.SsoId) && !string.IsNullOrEmpty(ssoId))
                    {
                        existingUser.SsoId = ssoId;
                        existingUser.Username = username;
                        existingUser.Name = name;
                        existingUser.IsActive = true;
                        existingUser.IsArchived = false;
                        await dbContext.SaveChangesAsync();

                        var logger = httpContext.RequestServices.GetService<ILogger<NexusAuthorizeAttribute>>();
                        logger?.LogInformation($"Updated SSO ID for existing user {email}");
                    }
                }
                else
                {
                    // user does not exist- create them in the DB
                    var newUser = new User
                    {
                        Name = name,
                        Email = email,
                        Username = username,
                        SsoId = ssoId,
                        IsActive = true,
                        IsArchived = false
                    };

                    dbContext.Users.Add(newUser);
                    await dbContext.SaveChangesAsync();

                    var logger = httpContext.RequestServices.GetService<ILogger<NexusAuthorizeAttribute>>();
                    logger?.LogInformation($"User with email {email} created successfully");
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request; UserContextMiddleware will handle missing user if relevant
            var logger = httpContext.RequestServices.GetService<ILogger<NexusAuthorizeAttribute>>();
            logger?.LogError(ex, "Error during user provisioning");
        }
    }
}