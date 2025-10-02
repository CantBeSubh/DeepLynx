using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using deeplynx.datalayer.Models;
using Microsoft.Extensions.Logging;

namespace deeplynx.hubs;

public class EventNotificationHub : Hub
{
    private readonly DeeplynxContext _context;
    private readonly ILogger<EventNotificationHub> _logger;
    private const string HARDCODED_EMAIL = "keaton.flake@inl.gov"; // TODO: Remove when JWT is implemented

    public EventNotificationHub(DeeplynxContext context, ILogger<EventNotificationHub> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        // Look up user from database using hardcoded email
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == HARDCODED_EMAIL);
        
        if (user != null)
        {
            // Add this connection to a group named after the userId
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{user.Id}");
            _logger.LogInformation(
                "User {Email} (ID: {UserId}) connected with ConnectionId: {ConnectionId}", 
                HARDCODED_EMAIL, user.Id, Context.ConnectionId
            );
        }
        else
        {
            _logger.LogWarning("Hardcoded user {Email} not found in database", HARDCODED_EMAIL);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == HARDCODED_EMAIL);
        
        if (user != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{user.Id}");
            _logger.LogInformation(
                "User {Email} (ID: {UserId}) disconnected. Reason: {Reason}", 
                HARDCODED_EMAIL, user.Id, exception?.Message ?? "Normal disconnect"
            );
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}