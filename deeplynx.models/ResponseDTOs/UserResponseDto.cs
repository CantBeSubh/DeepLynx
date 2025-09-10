namespace deeplynx.models;

public class UserResponseDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public bool IsSysAdmin { get; set; }
}