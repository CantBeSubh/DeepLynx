namespace deeplynx.models;

public class CreateUserRequestDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public bool? IsSysAdmin { get; set; } = false;
    
    // TODO: remove project ID from create user request
    public long? ProjectId { get; set; }
}