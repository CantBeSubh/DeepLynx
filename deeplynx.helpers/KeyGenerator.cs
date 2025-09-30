using System.Security.Cryptography;

namespace deeplynx.helpers;

public class KeyGenerator
{
    public static string GenerateKeyBase64(int byteLength = 64)
    {
        var bytes = new byte[byteLength];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        //creates a url safe string 
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('='); // Remove padding
    }
}