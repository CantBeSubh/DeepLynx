using deeplynx.helpers.Context;
using deeplynx.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace deeplynx.controllers;

[ApiController]
[Authorize]
[Route("oauth")]
[Tags("OauthHandshake")]
public class OauthHandshakeController : ControllerBase
{
    private readonly IOauthHandshakeBusiness _oauthBusiness;
    private readonly ILogger<OauthHandshakeController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OauthHandshakeController"/> class
    /// </summary>
    /// <param name="oauthBusiness">The business logic interface for handling record operations.</param>
    /// <param name="logger">Error/Info logging interface for database log table.</param>
    public OauthHandshakeController(
        IOauthHandshakeBusiness oauthBusiness,
        ILogger<OauthHandshakeController> logger
    )
    {
        _oauthBusiness = oauthBusiness;
        _logger = logger;
    }

    /// <summary>
    /// Oauth 2.0 Authorization Endpoint
    /// </summary>
    /// <param name="clientId">The known client ID of the requesting application</param>
    /// <param name="redirectUri">The callback url of the requesting application</param>
    /// <param name="state">CSRF protection token</param>
    /// <returns>Redirects to callback URL with authorization code</returns>
    /// <remarks>
    /// This endpoint requires authentication. The Next.js proxy ensures the user
    /// is authenticated before forwarding the request here.
    /// </remarks>
    [HttpGet("authorize", Name = "api_oauth_authorize")]
    public async Task<IActionResult> Authorize(
        [FromQuery] string clientId,
        [FromQuery] string redirectUri,
        [FromQuery] string state)
    {
        try
        {
            var userId = UserContextStorage.UserId;
            _logger.LogInformation($"Generating auth code for application {clientId} on behalf of user {userId}");

            var authCode = await _oauthBusiness.GenerateAuthCode(clientId, userId, redirectUri, state);
            var callbackUrl = BuildCallbackUrl(redirectUri, authCode, state);
            _logger.LogInformation($"Auth code generated successfully for application {clientId}, user {userId}");

            return Redirect(callbackUrl);
        }
        catch (Exception ex)
        {
            var message = "An unexpected error occurred in the Oauth authorization flow";
            _logger.LogError(ex, message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = message });
        }
    }

    /// <summary>
    /// Oauth 2.0 Token Endpoint
    /// </summary>
    /// <param name="code">The authorization code received from the authorize endpoint</param>
    /// <param name="clientId">The Oauth application's client ID</param>
    /// <param name="clientSecret">The Oauth application's client secret</param>
    /// <param name="redirectUri">The same redirect URI used in the authorize request</param>
    /// <param name="state">The same CSRF state used in the authorize request</param>
    /// <param name="expiration">(Optional) token expiration time in minutes (default: 480)</param>
    /// <returns>JSON response with access token or error</returns>
    /// <remarks>
    /// This endpoint does not require user authentication. It uses client credentials
    /// (client_id and client_secret) to authenticate the OAuth application.
    /// </remarks>
    [HttpPost("exchange", Name = "api_oauth_exchange")]
    [AllowAnonymous] // token exchange uses client credentials vs user authentication to verify identity
    public async Task<IActionResult> Exchange(
        [FromQuery] string code,
        [FromQuery] string clientId,
        [FromQuery] string clientSecret,
        [FromQuery] string redirectUri,
        [FromQuery] string state,
        [FromQuery] double? expiration)
    {
        try
        {
            _logger.LogInformation($"Exchanging auth code for token for application {clientId}");
            var token = await _oauthBusiness.ExchangeAuthCodeForToken(code, clientId, clientSecret, redirectUri, state, expiration);
            _logger.LogInformation($"Token generated successfully for application {clientId}");

            // Return token response in OAuth 2.0 standard format
            return Ok(new
            {
                access_token = token,
                token_type = "Bearer",
                expires_in = (expiration ?? 480) * 60, // Convert minutes to seconds
                state
            });
        }
        catch (Exception ex)
        {
            var message = "Unexpected error occurred during OAuth token exchange";
            _logger.LogError(ex, message);
            return StatusCode(500, new { error = "server_error", error_description = message});
        }
    }
    
    private string BuildCallbackUrl(string baseUrl, string code, string state)
    {
        var uriBuilder = new UriBuilder(baseUrl);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["code"] = code;
        query["state"] = state;

        uriBuilder.Query = query.ToString();
        return uriBuilder.ToString();
    }
}