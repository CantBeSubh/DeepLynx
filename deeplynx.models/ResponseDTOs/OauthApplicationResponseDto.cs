using System.ComponentModel.DataAnnotations.Schema;

namespace deeplynx.models;

public class OauthApplicationResponseDto
{
    public long Id { get; set; }
    
    public string ClientId { get; set; }
    
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    public string CallbackUrl { get; set; }
    
    public string? BaseUrl { get; set; }
    
    public string? AppOwnerEmail { get; set; }
    
    public bool IsArchived { get; set; }
    
    public DateTime LastUpdatedAt { get; set; }

    public long? LastUpdatedBy { get; set; }
}