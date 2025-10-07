using deeplynx.helpers;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers;

// <summary>
/// Controller creating tokens for api only users.
/// </summary>
/// <remarks>
/// This controller provides endpoints to create tokens and api keys.
/// </remarks>
[ApiController]
[NexusAuthorize]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IEventBusiness _eventBusiness;
    private readonly ITokenBusiness _tokenBusiness;
    public AuthController(IEventBusiness eventBusiness, ITokenBusiness tokenBusiness)
    {
        _eventBusiness = eventBusiness;
        _tokenBusiness = tokenBusiness;
    }

    /// <summary>
    /// Create JWT Token
    /// </summary>
    /// <param name="tokenDto"></param>
    /// <returns>Jwt token</returns>
    [HttpPost("CreateToken", Name = "api_create_token")]
    public IActionResult CreateToken([FromBody] CreateTokenDto tokenDto)
    {
        return Ok(_tokenBusiness.CreateToken(tokenDto.ApiSecret, tokenDto.ApiKey, tokenDto.ExpirationMinutes));
    }

    /// <summary>
    /// Create API Secret
    /// </summary>
    /// <returns>Api secret and api key</returns>
    [HttpPost("CreateApiKey", Name = "api_create_api_key")]
    public IActionResult CreateApiKey()
    {
        var tokenDto = _tokenBusiness.CreateApiKey();
        return Ok(tokenDto);
    }
}