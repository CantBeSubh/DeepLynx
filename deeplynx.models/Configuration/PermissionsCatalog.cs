namespace deeplynx.models.Configuration;
    public class PermissionStructure
    {
        public string Resource { get; set; }
        public string Action { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool isDefault { get; set; }
    }

    public static class DefaultPermissions
    {
        public static readonly List<PermissionStructure> AllDefaultPermissions = new()
        {
            new() { Resource = "project",           Action = "write", Name = "Write Projects",            Description = "Permission to write/modify project data",            isDefault = true },
            new() { Resource = "project",           Action = "read",  Name = "Read Projects",             Description = "Permission to read project data",                     isDefault = true },

            new() { Resource = "object_storage",    Action = "write", Name = "Write Object Storages",     Description = "Permission to write/modify object storage",           isDefault = true },
            new() { Resource = "object_storage",    Action = "read",  Name = "Read Object Storages",      Description = "Permission to read object storage",                    isDefault = true },

            new() { Resource = "data_source",       Action = "write", Name = "Write Data Sources",        Description = "Permission to write/modify data sources",             isDefault = true },
            new() { Resource = "data_source",       Action = "read",  Name = "Read Data Sources",         Description = "Permission to read data sources",                      isDefault = true },

            new() { Resource = "record",            Action = "write", Name = "Write Records",             Description = "Permission to write/modify records",                  isDefault = true },
            new() { Resource = "record",            Action = "read",  Name = "Read Records",              Description = "Permission to read records",                           isDefault = true },

            new() { Resource = "edge",              Action = "write", Name = "Write Edges",               Description = "Permission to write/modify edges",                    isDefault = true },
            new() { Resource = "edge",              Action = "read",  Name = "Read Edges",                Description = "Permission to read edges",                             isDefault = true },

            new() { Resource = "file",              Action = "write", Name = "Write Files",               Description = "Permission to write/modify files",                    isDefault = true },
            new() { Resource = "file",              Action = "read",  Name = "Read Files",                Description = "Permission to read files",                             isDefault = true },

            new() { Resource = "tag",               Action = "write", Name = "Write Tags",                Description = "Permission to write/modify tags",                     isDefault = true },
            new() { Resource = "tag",               Action = "read",  Name = "Read Tags",                 Description = "Permission to read tags",                              isDefault = true },

            new() { Resource = "class",             Action = "write", Name = "Write Classes",             Description = "Permission to write/modify classes",                  isDefault = true },
            new() { Resource = "class",             Action = "read",  Name = "Read Classes",              Description = "Permission to read classes",                           isDefault = true },

            new() { Resource = "relationship",      Action = "write", Name = "Write Relationships",       Description = "Permission to write/modify relationships",             isDefault = true },
            new() { Resource = "relationship",      Action = "read",  Name = "Read Relationships",        Description = "Permission to read relationships",                      isDefault = true },

            new() { Resource = "user",              Action = "write", Name = "Write Users",               Description = "Permission to write/modify users",                    isDefault = true },
            new() { Resource = "user",              Action = "read",  Name = "Read Users",                Description = "Permission to read users",                             isDefault = true },

            new() { Resource = "group",             Action = "write", Name = "Write Groups",              Description = "Permission to write/modify groups",                   isDefault = true },
            new() { Resource = "group",             Action = "read",  Name = "Read Groups",               Description = "Permission to read groups",                            isDefault = true },

            new() { Resource = "organization",      Action = "write", Name = "Write Organizations",       Description = "Permission to write/modify organizations",             isDefault = true },
            new() { Resource = "organization",      Action = "read",  Name = "Read Organizations",        Description = "Permission to read organizations",                      isDefault = true },

            new() { Resource = "role",              Action = "write", Name = "Write Roles",               Description = "Permission to write/modify roles",                    isDefault = true },
            new() { Resource = "role",              Action = "read",  Name = "Read Roles",                Description = "Permission to read roles",                             isDefault = true },

            new() { Resource = "permission",        Action = "write", Name = "Write Permissions",         Description = "Permission to write/modify permissions",               isDefault = true },
            new() { Resource = "permission",        Action = "read",  Name = "Read Permissions",          Description = "Permission to read permissions",                        isDefault = true },

            new() { Resource = "sensitivity_label", Action = "write", Name = "Write Sensitivity Labels",  Description = "Permission to write/modify sensitivity labels",         isDefault = true },
            new() { Resource = "sensitivity_label", Action = "read",  Name = "Read Sensitivity Labels",   Description = "Permission to read sensitivity labels",                 isDefault = true },
        };
    }