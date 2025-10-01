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
[Route("api/auth")]
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
    /// Creates a new token for api access for api users
    /// </summary>
    /// <param name="tokenDto"></param>
    /// <returns>Jwt token</returns>
    [HttpPost("CreateToken", Name = "api_create_token")]
    public IActionResult CreateToken([FromBody] CreateTokenDto tokenDto)
    {
        return Ok(_tokenBusiness.CreateToken(tokenDto.ApiSecret, tokenDto.ApiKey));
    }

    /// <summary>
    /// Creates an api secret and an api key for users accessing the api via an api key
    /// </summary>
    /// <returns>Api secret and api key</returns>
    [HttpPost("CreateApiKey", Name = "api_create_api_key")]
    public IActionResult CreateApiKey()
    {
        var tokenDto = _tokenBusiness.CreateApiKey();
        return Ok(tokenDto);
    }
}