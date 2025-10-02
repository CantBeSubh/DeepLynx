namespace deeplynx.helpers.Context;

public static class UserContextStorage
{
    private static AsyncLocal<string> _email = new();
    private static AsyncLocal<long> _userId = new();

    public static string Email
    {
        get => _email.Value;
        set => _email.Value = value;
    }

    public static long UserId
    {
        get => _userId.Value;
        set => _userId.Value = value;
    }
}