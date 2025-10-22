using deeplynx.helpers;
using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Authorization;
using deeplynx.helpers.Context;

namespace deeplynx.api.Controllers;

// <summary>
/// Controller creating tokens for api only users.
/// </summary>
/// <remarks>
/// This controller provides endpoints to create tokens and api keys.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/token")]
public class TokenController : ControllerBase
{
    private readonly IEventBusiness _eventBusiness;
    private readonly ITokenBusiness _tokenBusiness;
    private readonly ILogger<RecordController> _logger;
    public TokenController(IEventBusiness eventBusiness, ITokenBusiness tokenBusiness,ILogger<RecordController> logger)
    {
        _eventBusiness = eventBusiness;
        _tokenBusiness = tokenBusiness;
        _logger = logger;
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

    /// <summary>
    /// Delete API Secret
    /// </summary>
    /// <param name="key">key to be deleted</param>
    /// <returns>Api secret and api key</returns>
    [HttpDelete("DeleteApiKey/{key}", Name = "api_delete_api_key")]
    async public Task<IActionResult> DeleteApiKey(string key)
    {
        var userId = UserContextStorage.UserId;
        try
        {
            await _tokenBusiness.DeleteApiKey(userId, key);
            return Ok(new { message = $"Deleted key {key}" });
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while deleting key {key}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    /// <summary>
    /// Get all api keys associated with a user
    /// </summary>
    /// <returns>api keys associated with the user</returns>
    [HttpGet("GetAllUserKeys", Name = "api_get_all_user_keys")]
    public async Task<ActionResult<List<string>>> GetAllUserKeys()
    {
        var userId = UserContextStorage.UserId;
        try
        {
            var keys = await _tokenBusiness.GetAllUserKeys(userId);
            return Ok(keys);
        }
        catch (Exception exc)
        {
            var message = $"An error occurred while retrieving keys for user {userId}: {exc}";
            _logger.LogError(message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}