using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using deeplynx.datalayer.Models;
using Microsoft.Extensions.Logging;

namespace deeplynx.helpers.Hubs;

public class EventNotificationHub : Hub
{
    private readonly DeeplynxContext _context;
    private readonly ILogger<EventNotificationHub> _logger;

    public EventNotificationHub(DeeplynxContext context, ILogger<EventNotificationHub> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        // Access user from SignalR's Context
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            // Get email from the JWT claims
            var email = Context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            
            if (!string.IsNullOrEmpty(email))
            {
                // Look up user from database
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                
                if (user != null)
                {
                    // Add this connection to a group named after the userId
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{user.Id}");
                    _logger.LogInformation(
                        "User {Email} (ID: {UserId}) connected with ConnectionId: {ConnectionId}", 
                        email, user.Id, Context.ConnectionId
                    );
                }
                else
                {
                    _logger.LogWarning("User {Email} not found in database", email);
                }
            }
            else
            {
                _logger.LogWarning("Email claim not found in token");
            }
        }
        else
        {
            _logger.LogWarning("User not authenticated in SignalR connection");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User?.Identity?.IsAuthenticated == true)
        {
            var email = Context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                
                if (user != null)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{user.Id}");
                    _logger.LogInformation(
                        "User {Email} (ID: {UserId}) disconnected. Reason: {Reason}", 
                        email, user.Id, exception?.Message ?? "Normal disconnect"
                    );
                }
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}