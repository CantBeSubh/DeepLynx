using deeplynx.helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers;

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

    [HttpPost("CreateToken", Name = "api_create_token")]
    public IActionResult CreateToken([FromBody] CreateTokenDto tokenDto)
    {
        
        return Ok(_tokenBusiness.CreateToken(apiKey, apiKey));
    }

    [HttpPost("CreateApiKey", Name = "api_create_api_key")]
    public IActionResult CreateApiKey()
    {
        string apiKey = KeyGenerator.GenerateKeyBase64();
        string secret = KeyGenerator.GenerateKeyBase64();
        
    }

}