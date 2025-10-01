namespace deeplynx.models;

public class UpdateUserRequestDto
{
    public string? Name { get; set; }
    // since we search by email, do not allow update of email
    public string? Username { get; set; }
    public string? SsoId { get; set; }
    public bool? IsArchived { get; set; } = false;
    
    public long? ProjectId { get; set; }
    public bool? IsActive { get; set; } = false;
}