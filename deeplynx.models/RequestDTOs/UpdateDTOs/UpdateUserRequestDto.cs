namespace deeplynx.models;

public class UpdateUserRequestDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool? IsArchived { get; set; } = false;
    
    public long? ProjectId { get; set; }
}