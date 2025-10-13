namespace deeplynx.models.Configuration;

public class DefaultRolePermissions
{
    public static class Admin
    {
        // project admin gets everything except for write on organization
        public static readonly Dictionary<string, string[]> AllowedPermissions = new()
        {
            { "project", new[] { "read", "write" } },
            { "object_storage", new[] { "read", "write" } },
            { "data_source", new[] { "read", "write" } },
            { "record", new[] { "read", "write" } },
            { "edge", new[] { "read", "write" } },
            { "file", new[] { "read", "write" } },
            { "tag", new[] { "read", "write" } },
            { "class", new[] { "read", "write" } },
            { "relationship", new[] { "read", "write" } },
            { "user", new[] { "read", "write" } },
            { "group", new[] { "read", "write" } },
            { "organization", new[] { "read" } },
            { "role", new[] { "read", "write" } },
            { "permission", new[] { "read", "write" } },
            { "sensitivity_label", new[] { "read", "write" } },
        };
    }

    public static class User
    {
        // user has more limited permissions than admin
        public static readonly Dictionary<string, string[]> AllowedPermissions = new()
        {
            { "project", new[] { "read" } },
            { "object_storage", new[] { "read" } },
            { "data_source", new[] { "read", "write" } },
            { "record", new[] { "read", "write" } },
            { "edge", new[] { "read", "write" } },
            { "file", new[] { "read", "write" } },
            { "tag", new[] { "read", "write" } },
            { "class", new[] { "read", "write" } },
            { "relationship", new[] { "read", "write" } },
            { "user", new[] { "read" } },
            { "group", new[] { "read" } },
            { "organization", new[] { "read" } },
            { "role", new[] { "read" } },
            { "permission", new[] { "read" } },
            { "sensitivity_label", new[] { "read" } },
        };
    }
}