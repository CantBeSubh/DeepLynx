namespace deeplynx.models;

public class OauthCodeData
{
    public string Code { get; set; }
    public long ApplicationId { get; set; }
    public string RedirectUri { get; set; }
    public long UserId { get; set; }
    public string State { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
}