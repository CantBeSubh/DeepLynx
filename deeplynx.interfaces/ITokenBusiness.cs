namespace deeplynx.interfaces;

public interface ITokenBusiness
{
    string CreateToken(string secretKey, string apiKey);
    public string CreateApiKey();
}