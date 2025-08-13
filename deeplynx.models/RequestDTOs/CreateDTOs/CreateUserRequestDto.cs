namespace deeplynx.models;

public class CreateUserRequestDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime? ArchivedAt { get; set; }
    
    public long? ProjectId { get; set; }
}