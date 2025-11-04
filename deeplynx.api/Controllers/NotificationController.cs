using Microsoft.AspNetCore.Mvc;
using deeplynx.interfaces;
using Microsoft.AspNetCore.Authorization;
using deeplynx.helpers;

namespace deeplynx.api.Controllers
{

    /// <summary>
    /// Controller for managing notifications.
    /// </summary>
    [ApiController]
    [Route("notification")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationBusiness _notificationBusiness;
        private readonly ILogger<ClassController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class
        /// </summary>
        /// <param name="notificationBusiness">The business logic interface for handling class operations.</param>
        /// <param name="logger">Error/Info logging interface for database log table.</param>
        public NotificationController(INotificationBusiness notificationBusiness, ILogger<ClassController> logger)
        {
            _notificationBusiness = notificationBusiness;
            _logger = logger;
        }
        /// <summary>
        /// Send email
        /// </summary>
        [HttpPost("SendEmail", Name = "api_send_email")]
        [AuthInProject("read", "notification")]
        [AuthInProject("write", "notification")]
        public async Task<IActionResult> SendEmail([FromQuery] string email, string? name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = email;
                }
                var message = await _notificationBusiness.SendEmail(email, name);
                return Ok(message);
            }
            catch (Exception exc)
            {
                var message = $"An unexpected error occurred while sending email: {exc}";
                _logger.LogError(message);
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}