using Microsoft.EntityFrameworkCore;
using deeplynx.models;
using deeplynx.interfaces;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace deeplynx.business;

public class OauthApplicationBusiness : IOauthApplicationBusiness
{
    private readonly DeeplynxContext _context;
    private readonly ILogger<OauthApplicationBusiness> _logger;
    private readonly IEventBusiness _eventBusiness;

    public OauthApplicationBusiness(
        DeeplynxContext context,
        ILogger<OauthApplicationBusiness> logger,
        IEventBusiness eventBusiness
    )
    {
        _context = context;
        _eventBusiness = eventBusiness;
        _logger = logger;
    }

    public async Task<IEnumerable<OauthApplicationResponseDto>> GetAllOauthApplications(bool hideArchived = true)
    {
        var applicationQuery = _context.OauthApplications.AsQueryable();

        if (hideArchived)
        {
            applicationQuery = applicationQuery.Where(x => !x.IsArchived);
        }
        
        var applications = await applicationQuery.ToListAsync();

        return applications
            .Select(a => new OauthApplicationResponseDto()
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                AppOwnerEmail = a.AppOwnerEmail,
                CallbackUrl = a.CallbackUrl,
                BaseUrl = a.BaseUrl,
                ClientId = a.ClientId,
                IsArchived = a.IsArchived,
                LastUpdatedAt = a.LastUpdatedAt,
                LastUpdatedBy = a.LastUpdatedBy
            });
    }

    public async Task<OauthApplicationResponseDto> GetOauthApplication(
        long applicationId, bool hideArchived = true)
    {
        var application = await _context.OauthApplications
            .Where(a => a.Id == applicationId)
            .FirstOrDefaultAsync();

        if (application == null)
        {
            throw new KeyNotFoundException($"Oauth application with id {applicationId} not found");
        }

        if (hideArchived && application.IsArchived)
        {
            throw new KeyNotFoundException($"Oauth application with id {applicationId} is archived");
        }

        return new OauthApplicationResponseDto
        {
            Id = application.Id,
            Name = application.Name,
            Description = application.Description,
            AppOwnerEmail = application.AppOwnerEmail,
            CallbackUrl = application.CallbackUrl,
            BaseUrl = application.BaseUrl,
            ClientId = application.ClientId,
            IsArchived = application.IsArchived,
            LastUpdatedAt = application.LastUpdatedAt,
            LastUpdatedBy = application.LastUpdatedBy
        };
    }

    public async Task<OauthApplicationSecureResponseDto> CreateOauthApplication(
        CreateOauthApplicationRequestDto requestDto, long userId)
    {
        ValidationHelper.ValidateModel(requestDto);

        // generate client ID and secret
        var clientId = GenerateClientId();
        var clientSecret = GenerateClientSecret();
        var clientSecretHash = HashSecret(clientSecret);

        var application = new OauthApplication
        {
            Name = requestDto.Name,
            Description = requestDto.Description,
            ClientId = clientId,
            ClientSecretHash = clientSecretHash,
            CallbackUrl = requestDto.CallbackUrl,
            BaseUrl = requestDto.BaseUrl,
            AppOwnerEmail = requestDto.AppOwnerEmail,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            LastUpdatedBy = userId
        };
        
        _context.OauthApplications.Add(application);
        await _context.SaveChangesAsync();
        
        // log create OAuth application event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            Operation = "create",
            EntityType = "oauth_application",
            EntityId = application.Id,
            EntityName = application.Name,
            Properties = JsonSerializer.Serialize(new { application.Name, application.ClientId })
        });

        return new OauthApplicationSecureResponseDto
        {
            Name = application.Name,
            ClientId = application.ClientId,
            ClientSecretRaw = clientSecret,
        };
    }

    public async Task<OauthApplicationResponseDto> UpdateOauthApplication(
        long applicationId, UpdateOauthApplicationRequestDto requestDto, long userId)
    {
        var application = await _context.OauthApplications.FindAsync(applicationId);

        if (application == null || application.IsArchived)
        {
            throw new KeyNotFoundException($"Oauth application with id {applicationId} not found");
        }
        
        application.Name = requestDto.Name ?? application.Name;
        application.Description = requestDto.Description ?? application.Description;
        application.CallbackUrl = requestDto.CallbackUrl ?? application.CallbackUrl;
        application.BaseUrl = requestDto.BaseUrl ?? application.BaseUrl;
        application.AppOwnerEmail = requestDto.AppOwnerEmail ?? application.AppOwnerEmail;
        application.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        application.LastUpdatedBy = userId;
        
        _context.OauthApplications.Update(application);
        await _context.SaveChangesAsync();
        
        // log update Oauth application event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            Operation = "update",
            EntityType = "oauth_application",
            EntityId = application.Id,
            EntityName = application.Name,
            Properties = JsonSerializer.Serialize(new { application.Name, application.ClientId })
        });

        return new OauthApplicationResponseDto
        {
            Id = application.Id,
            Name = application.Name,
            Description = application.Description,
            AppOwnerEmail = application.AppOwnerEmail,
            CallbackUrl = application.CallbackUrl,
            BaseUrl = application.BaseUrl,
            ClientId = application.ClientId,
            IsArchived = application.IsArchived,
            LastUpdatedAt = application.LastUpdatedAt,
            LastUpdatedBy = application.LastUpdatedBy
        };
    }

    public async Task<bool> ArchiveOauthApplication(long applicationId, long userId)
    {
        var application = await _context.OauthApplications.FindAsync(applicationId);
        
        if (application == null || application.IsArchived)
            throw new KeyNotFoundException($"Oauth application with id {applicationId} not found");

        application.IsArchived = true;
        application.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        application.LastUpdatedBy = userId;
        _context.OauthApplications.Update(application);
        await _context.SaveChangesAsync();
        
        // log archive Oauth application event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            Operation = "archive",
            EntityType = "oauth_application",
            EntityId = application.Id,
            EntityName = application.Name,
            Properties = JsonSerializer.Serialize(new { application.Name, application.ClientId })
        });

        return true;
    }
    
    public async Task<bool> UnarchiveOauthApplication(long applicationId, long userId)
    {
        var application = await _context.OauthApplications.FindAsync(applicationId);
        
        if (application == null || !application.IsArchived)
            throw new KeyNotFoundException($"Oauth application with id {applicationId} not found or is not archived");

        application.IsArchived = false;
        application.LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        application.LastUpdatedBy = userId;
        _context.OauthApplications.Update(application);
        await _context.SaveChangesAsync();
        
        // log unarchive Oauth application event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            Operation = "unarchive",
            EntityType = "oauth_application",
            EntityId = application.Id,
            EntityName = application.Name,
            Properties = JsonSerializer.Serialize(new { application.Name, application.ClientId })
        });

        return true;
    }
    
    public async Task<bool> DeleteOauthApplication(long applicationId, long userId)
    {
        var application = await _context.OauthApplications.FindAsync(applicationId);
        
        if (application == null || application.IsArchived)
            throw new KeyNotFoundException($"Oauth application with id {applicationId} not found");
        
        // grab event-relevant details before deletion
        var appId = application.Id;
        var appName = application.Name;
        var appClientId = application.ClientId;

        _context.OauthApplications.Remove(application);
        await _context.SaveChangesAsync();
        
        // log delete Oauth application event
        await _eventBusiness.CreateEvent(new CreateEventRequestDto
        {
            Operation = "delete",
            EntityType = "oauth_application",
            EntityId = appId,
            EntityName = appName,
            Properties = JsonSerializer.Serialize(new { appName, appClientId })
        });

        return true;
    }

    private string GenerateClientId()
    {
        var bytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        var base64 = Convert.ToBase64String(bytes);
        return base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private string GenerateClientSecret()
    {
        var bytes = new byte[64];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        var base64 = Convert.ToBase64String(bytes);
        return base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private string HashSecret(string secret)
    {
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            var salt = new byte[32];
            rng.GetBytes(salt);

            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
               secret,
               salt,
               100000,
               System.Security.Cryptography.HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(32);
                var saltBase64 = Convert.ToBase64String(salt);
                var hashBase64 = Convert.ToBase64String(hash);
                return $"{saltBase64}:{hashBase64}";
            }
        }
    }
}