namespace deeplynx.models;

public class CreateTokenDto
{
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public int ExpirationMinutes { get; set; }
}