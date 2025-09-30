namespace deeplynx.helpers.Context;

public static class UserContextStorage
{
    private static AsyncLocal<string> _username = new();

    public static string Username
    {
        get => _username.Value;
        set => _username.Value = value;
    }
}