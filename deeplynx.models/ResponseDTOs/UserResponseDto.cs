namespace deeplynx.models;

public class UserResponseDto
{
    public string FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; }
    public DateTime? ArchivedAt { get; set; }
}