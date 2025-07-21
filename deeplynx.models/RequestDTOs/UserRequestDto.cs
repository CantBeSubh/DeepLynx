namespace deeplynx.models;

public class UserRequestDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime? ArchivedAt { get; set; }
    
    public long? ProjectId { get; set; }
}