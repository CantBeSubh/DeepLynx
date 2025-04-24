namespace YourProjectNamespace.Helpers
{
    using System.Text.Json;

    public static class JsonHelper
    {
        public static string? ExtractStringOrJson(object? input)
        {
            return input switch
            {
                null => null,
                JsonElement element => element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    _ => element.ToString()
                },
                _ => input.ToString()
            };
        }
    }
}