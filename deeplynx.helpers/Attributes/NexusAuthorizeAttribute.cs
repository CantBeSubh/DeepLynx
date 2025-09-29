using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;

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
            var token = ExtractToken(context.HttpContext.Request);
            if (string.IsNullOrEmpty(token))
            {
                SetUnauthorizedResult(context, "No token provided");
                return;
            }

            var principal = ValidateToken(token).GetAwaiter().GetResult();
            context.HttpContext.User = principal;

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

    private async Task<ClaimsPrincipal> ValidateToken(string token)
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

        // (Optional) double-check alg in the header
        if (validatedToken is JwtSecurityToken jwt &&
            !string.Equals(jwt.Header.Alg, SecurityAlgorithms.RsaSha256, StringComparison.Ordinal))
        {
            throw new SecurityTokenInvalidAlgorithmException("Unexpected token algorithm.");
        }

        return principal;
    }

    private void SetUnauthorizedResult(AuthorizationFilterContext context, string message)
    {
        context.Result = new JsonResult(new { error = message, status = 401 }) { StatusCode = 401 };
    }

    private void SetForbiddenResult(AuthorizationFilterContext context, string message)
    {
        context.Result = new JsonResult(new { error = message, status = 403 }) { StatusCode = 403 };
    }
}