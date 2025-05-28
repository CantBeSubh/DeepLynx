using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using deeplynx.models;

namespace deeplynx.api.Controllers
{
    [ApiController]
    [Route("/")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginBusiness _loginBusiness;

        public LoginController(ILoginBusiness loginBusiness)
        {
            _loginBusiness = loginBusiness;
        }
        /// <summary>
        /// Login
        /// </summary>
        /// <returns></returns>
        [HttpGet("login")]
        public async Task<IActionResult> Login()
        {
            try
            {
                _loginBusiness.Login();
                return Ok();
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while logging in.: {exc}";
                NLog.LogManager.GetCurrentClassLogger().Error(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
            
        }
    }
}