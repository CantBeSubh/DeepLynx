namespace deeplynx.mcp.helpers;

public class EnvironmentHelper
{
    public static string GetRequiredEnvironmentVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);
        
        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException(
                $"Required environment variable '{variableName}' is not set. " +
                $"Please ensure it is defined in your .env file or environment variables.");
        }
        
        return value;
    }
}