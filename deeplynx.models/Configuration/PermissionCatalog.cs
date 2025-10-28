namespace deeplynx.models.Configuration;

public class PermissionCatalog
{
    public static class DefaultPermissions
    {
        // List of all resources and associated perms
        public static readonly Dictionary<string, string[]> AllDefaultPermissions = new()
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
            { "organization", new[] { "read", "write" } },
            { "role", new[] { "read", "write" } },
            { "permission", new[] { "read", "write" } },
            { "sensitivity_label", new[] { "read", "write" } },
        };
    }
}