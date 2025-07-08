using deeplynx.datalayer.Models;
using Microsoft.EntityFrameworkCore;
using Record = deeplynx.datalayer.Models.Record;

namespace deeplynx.tests;

public static class SeedData
{
    public static async Task SeedDatabase(DeeplynxContext context)
    {
        // Seed all entities in proper order to maintain referential integrity
        await SeedUsers(context);
        await SeedProjects(context);
        await SeedClasses(context);
        await SeedDataSources(context);
        await SeedTags(context);
        await SeedRelationships(context);
        await SeedEdgeMappings(context);
        await SeedRecordMappings(context);
        await SeedRecords(context);
        await SeedEdges(context);
    }

    // Users
    public static async Task SeedUsers(DeeplynxContext context)
    {
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Name = "John Smith",
                Email = "john.smith@company.com",
                Password = "$2a$10$N9qo8uLOickgx2ZMRZoMye.hashedpassword1",
                ArchivedAt = null
            },
            new User
            {
                Id = 2,
                Name = "Sarah Johnson",
                Email = "sarah.johnson@company.com",
                Password = "$2a$10$N9qo8uLOickgx2ZMRZoMye.hashedpassword2",
                ArchivedAt = null
            },
            new User
            {
                Id = 3,
                Name = "Mike Davis",
                Email = "mike.davis@company.com",
                Password = "$2a$10$N9qo8uLOickgx2ZMRZoMye.hashedpassword3",
                ArchivedAt = null
            },
            new User
            {
                Id = 4,
                Name = "System Architect",
                Email = "system.architect@legacy.com",
                Password = "$2a$10$N9qo8uLOickgx2ZMRZoMye.hashedpassword4",
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1) // Archived legacy user
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }

    // Projects
    public static async Task SeedProjects(DeeplynxContext context)
    {
        var projects = new List<Project>
        {
            new Project
            {
                Id = 1,
                Name = "Customer Analytics Platform",
                Abbreviation = "CAP",
                Description = "Comprehensive customer data analysis and segmentation platform for marketing insights",
                CreatedBy = "john.smith@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
                ModifiedBy = "sarah.johnson@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-15),
                ArchivedAt = null
            },
            new Project
            {
                Id = 2,
                Name = "Supply Chain Optimization",
                Abbreviation = "SCO",
                Description = "Real-time supply chain monitoring and optimization system with predictive analytics",
                CreatedBy = "mike.davis@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-8),
                ModifiedBy = "mike.davis@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-3),
                ArchivedAt = null
            },
            new Project
            {
                Id = 3,
                Name = "Legacy System Migration",
                Abbreviation = "LSM",
                Description = "Migration of legacy data systems to modern cloud-based infrastructure",
                CreatedBy = "system.architect@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-18),
                ModifiedBy = "migration.lead@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-2),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            }
        };

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();
    }

    // Classes
    public static async Task SeedClasses(DeeplynxContext context)
    {
        var classes = new List<Class>
        {
            // Customer Analytics Platform Classes
            new Class
            {
                Id = 1,
                Name = "Customer",
                Description = "Customer entity with demographic and behavioral data",
                Uuid = "550e8400-e29b-41d4-a716-446655440101",
                ProjectId = 1,
                CreatedBy = "john.smith@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
                ModifiedBy = "sarah.johnson@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-20),
                ArchivedAt = null
            },
            new Class
            {
                Id = 2,
                Name = "Purchase",
                Description = "Customer purchase transactions and order history",
                Uuid = "550e8400-e29b-41d4-a716-446655440102",
                ProjectId = 1,
                CreatedBy = "john.smith@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                ModifiedBy = "data.analyst@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-12),
                ArchivedAt = null
            },
            new Class
            {
                Id = 3,
                Name = "Segment",
                Description = "Customer segmentation categories and rules",
                Uuid = "550e8400-e29b-41d4-a716-446655440103",
                ProjectId = 1,
                CreatedBy = "sarah.johnson@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                ModifiedBy = "sarah.johnson@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-8),
                ArchivedAt = null
            },
            new Class
            {
                Id = 4,
                Name = "Product",
                Description = "Product catalog and inventory information",
                Uuid = "550e8400-e29b-41d4-a716-446655440104",
                ProjectId = 1,
                CreatedBy = "product.manager@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-9),
                ModifiedBy = "inventory.system@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-5),
                ArchivedAt = null
            },

            // Supply Chain Optimization Classes
            new Class
            {
                Id = 5,
                Name = "Supplier",
                Description = "Supplier information and performance metrics",
                Uuid = "550e8400-e29b-41d4-a716-446655440201",
                ProjectId = 2,
                CreatedBy = "mike.davis@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-8),
                ModifiedBy = "supply.manager@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-5),
                ArchivedAt = null
            },
            new Class
            {
                Id = 6,
                Name = "Inventory",
                Description = "Product inventory levels and warehouse data",
                Uuid = "550e8400-e29b-41d4-a716-446655440202",
                ProjectId = 2,
                CreatedBy = "mike.davis@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7),
                ModifiedBy = "warehouse.admin@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-2),
                ArchivedAt = null
            },
            new Class
            {
                Id = 7,
                Name = "Warehouse",
                Description = "Warehouse facility information and capacity data",
                Uuid = "550e8400-e29b-41d4-a716-446655440203",
                ProjectId = 2,
                CreatedBy = "warehouse.manager@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                ModifiedBy = "facility.admin@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-4),
                ArchivedAt = null
            },
            new Class
            {
                Id = 8,
                Name = "Shipment",
                Description = "Shipping and logistics tracking information",
                Uuid = "550e8400-e29b-41d4-a716-446655440204",
                ProjectId = 2,
                CreatedBy = "logistics.coordinator@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                ModifiedBy = "mike.davis@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-4),
                ArchivedAt = null
            },

            // Legacy System Migration Classes (Archived)
            new Class
            {
                Id = 9,
                Name = "LegacyUser",
                Description = "User accounts from legacy system requiring migration",
                Uuid = "550e8400-e29b-41d4-a716-446655440301",
                ProjectId = 3,
                CreatedBy = "system.architect@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-18),
                ModifiedBy = "migration.lead@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-3),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            },
            new Class
            {
                Id = 10,
                Name = "LegacyData",
                Description = "Historical data records from legacy database",
                Uuid = "550e8400-e29b-41d4-a716-446655440302",
                ProjectId = 3,
                CreatedBy = "system.architect@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                ModifiedBy = "data.migration@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-2),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            }
        };

        await context.Classes.AddRangeAsync(classes);
        await context.SaveChangesAsync();
    }

    // Data Sources
    public static async Task SeedDataSources(DeeplynxContext context)
    {
        var dataSources = new List<DataSource>
        {
            // Customer Analytics Platform Data Sources
            new DataSource
            {
                
                Name = "Customer CRM Database",
                Description = "Primary customer relationship management database",
                Abbreviation = "CRM_DB",
                Type = "SQL Server",
                BaseUri = "Server=crm-prod.company.com;Database=CustomerData;",
                Config = @"{""driver"":""sqlserver"",""host"":""crm-prod.company.com"",""port"":1433,""database"":""CustomerData"",""ssl_enabled"":true}",
                ProjectId = 1,
                CreatedBy = "john.smith@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
                ModifiedBy = "db.admin@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-45),
                ArchivedAt = null
            },
            new DataSource
            {
                
                Name = "E-commerce Transaction API",
                Description = "Real-time API for accessing e-commerce transaction data",
                Abbreviation = "ECOM_API",
                Type = "REST API",
                BaseUri = "https://api.ecommerce.company.com/v2/",
                Config = @"{""api_version"":""v2"",""authentication"":""Bearer Token"",""rate_limit"":1000,""timeout"":30}",
                ProjectId = 1,
                CreatedBy = "sarah.johnson@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                ModifiedBy = "api.developer@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-20),
                ArchivedAt = null
            },

            // Supply Chain Optimization Data Sources
            new DataSource
            {
               
                Name = "Enterprise Resource Planning System",
                Description = "Comprehensive ERP system with supplier and inventory data",
                Abbreviation = "ERP_SYS",
                Type = "Oracle Database",
                BaseUri = "Server=erp-oracle.company.com;Database=SUPPLY_CHAIN;",
                Config = @"{""driver"":""oracle"",""host"":""erp-oracle.company.com"",""port"":1521,""service_name"":""SUPPLY_CHAIN""}",
                ProjectId = 2,
                CreatedBy = "mike.davis@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-8),
                ModifiedBy = "erp.admin@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-10),
                ArchivedAt = null
            },
            new DataSource
            {
                
                Name = "IoT Warehouse Sensors",
                Description = "Real-time sensor data from warehouse facilities",
                Abbreviation = "IOT_SENSORS",
                Type = "MQTT Broker",
                BaseUri = "mqtt://iot-broker.warehouse.company.com:1883",
                Config = @"{""protocol"":""MQTT"",""broker_host"":""iot-broker.warehouse.company.com"",""port"":1883,""qos"":2}",
                ProjectId = 2,
                CreatedBy = "iot.specialist@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                ModifiedBy = "warehouse.manager@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-5),
                ArchivedAt = null
            },

            // Legacy System Migration Data Sources (Archived)
            new DataSource
            {
                
                Name = "Legacy Mainframe Database",
                Description = "Historical mainframe system with legacy data",
                Abbreviation = "MAINFRAME_DB",
                Type = "DB2 Mainframe",
                BaseUri = "Server=mainframe.legacy.com;Database=LEGACY_PROD;",
                Config = @"{""driver"":""db2"",""host"":""mainframe.legacy.com"",""port"":50000,""database"":""LEGACY_PROD""}",
                ProjectId = 3,
                CreatedBy = "system.architect@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-18),
                ModifiedBy = "migration.lead@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-3),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            },
            new DataSource
            {
                
                Name = "Legacy File System Archives",
                Description = "Historical file-based data archives",
                Abbreviation = "FILE_ARCHIVE",
                Type = "File System",
                BaseUri = "file://legacy-archive.company.com/data/",
                Config = @"{""type"":""network_share"",""base_path"":""//legacy-archive.company.com/data/"",""compression"":""gzip""}",
                ProjectId = 3,
                CreatedBy = "system.architect@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                ModifiedBy = "data.migration@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            }
        };

        await context.DataSources.AddRangeAsync(dataSources);
        await context.SaveChangesAsync();
        //await context.Database.ExecuteSqlRawAsync($"select setval(pg_get_serial_sequence('data_sources', 'id'), (select max(id) from data_sources))");
    }

    // Tags
    public static async Task SeedTags(DeeplynxContext context)
    {
        var tags = new List<Tag>
        {
            // Note: Id is automatically assigned and incremented by the database. These tags will be Id 1-12 in order.
            
            // Customer Analytics Platform Tags
            new Tag { Name = "Analytics", ProjectId = 1, CreatedBy = "john.smith@company.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12), ModifiedBy = "sarah.johnson@company.com", ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-30), ArchivedAt = null },
            new Tag { Name = "Marketing", ProjectId = 1, CreatedBy = "sarah.johnson@company.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11), ModifiedBy = null, ModifiedAt = null, ArchivedAt = null },
            new Tag { Name = "Customer Data", ProjectId = 1, CreatedBy = "john.smith@company.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10), ModifiedBy = "data.analyst@company.com", ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-20), ArchivedAt = null },
            new Tag { Name = "Business Intelligence", ProjectId = 1, CreatedBy = "sarah.johnson@company.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-9), ModifiedBy = "sarah.johnson@company.com", ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-15), ArchivedAt = null },

            // Supply Chain Optimization Tags
            new Tag { Name = "Logistics", ProjectId = 2, CreatedBy = "mike.davis@company.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-8), ModifiedBy = "supply.manager@company.com", ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-10), ArchivedAt = null },
            new Tag { Name = "Optimization", ProjectId = 2, CreatedBy = "mike.davis@company.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7), ModifiedBy = null, ModifiedAt = null, ArchivedAt = null },
            new Tag { Name = "Real-time Monitoring", ProjectId = 2, CreatedBy = "supply.manager@company.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6), ModifiedBy = "iot.specialist@company.com", ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-5), ArchivedAt = null },
            new Tag { Name = "Predictive Analytics", ProjectId = 2, CreatedBy = "data.scientist@company.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-5), ModifiedBy = "mike.davis@company.com", ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-3), ArchivedAt = null },
            new Tag { Name = "Inventory Management", ProjectId = 2, CreatedBy = "warehouse.admin@company.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4), ModifiedBy = null, ModifiedAt = null, ArchivedAt = null },

            // Legacy System Migration Tags (Archived)
            new Tag { Name = "Migration", ProjectId = 3, CreatedBy = "system.architect@legacy.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-18), ModifiedBy = "migration.lead@legacy.com", ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-3), ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1) },
            new Tag { Name = "Legacy Systems", ProjectId = 3, CreatedBy = "system.architect@legacy.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17), ModifiedBy = "data.migration@legacy.com", ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4), ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1) },
            new Tag { Name = "Data Transformation", ProjectId = 3, CreatedBy = "data.migration@legacy.com", CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-16), ModifiedBy = "migration.lead@legacy.com", ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-2), ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1) }
        };

        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();
    }

    // Relationships
    public static async Task SeedRelationships(DeeplynxContext context)
    {
        var relationships = new List<Relationship>
        {
            // Customer Analytics Platform Relationships
            new Relationship
            {
                Id = 1,
                Name = "Customer Makes Purchase",
                Description = "Defines the relationship between customers and their purchase transactions",
                Uuid = "550e8400-e29b-41d4-a716-446655440001",
                OriginId = 1, // Customer class
                DestinationId = 2, // Purchase class
                ProjectId = 1,
                CreatedBy = "john.smith@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                ModifiedBy = "sarah.johnson@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-25),
                ArchivedAt = null
            },
            new Relationship
            {
                Id = 2,
                Name = "Customer Belongs To Segment",
                Description = "Associates customers with their market segmentation categories",
                Uuid = "550e8400-e29b-41d4-a716-446655440002",
                OriginId = 1, // Customer class
                DestinationId = 3, // Segment class
                ProjectId = 1,
                CreatedBy = "sarah.johnson@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                ModifiedBy = "marketing.analyst@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-18),
                ArchivedAt = null
            },
            new Relationship
            {
                Id = 3,
                Name = "Purchase Contains Products",
                Description = "Links purchase transactions to specific products",
                Uuid = "550e8400-e29b-41d4-a716-446655440003",
                OriginId = 2, // Purchase class
                DestinationId = 4, // Product class
                ProjectId = 1,
                CreatedBy = "data.analyst@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-9),
                ModifiedBy = "john.smith@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-15),
                ArchivedAt = null
            },

            // Supply Chain Optimization Relationships
            new Relationship
            {
                Id = 4,
                Name = "Supplier Provides Inventory",
                Description = "Establishes supply relationship between suppliers and inventory",
                Uuid = "550e8400-e29b-41d4-a716-446655440004",
                OriginId = 5, // Supplier class
                DestinationId = 6, // Inventory class
                ProjectId = 2,
                CreatedBy = "mike.davis@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7),
                ModifiedBy = "supply.manager@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-10),
                ArchivedAt = null
            },
            new Relationship
            {
                Id = 5,
                Name = "Inventory Stored In Warehouse",
                Description = "Maps inventory items to warehouse storage locations",
                Uuid = "550e8400-e29b-41d4-a716-446655440005",
                OriginId = 6, // Inventory class
                DestinationId = 7, // Warehouse class
                ProjectId = 2,
                CreatedBy = "warehouse.manager@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                ModifiedBy = "logistics.coordinator@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-8),
                ArchivedAt = null
            },
            new Relationship
            {
                Id = 6,
                Name = "Shipment Contains Inventory",
                Description = "Defines inventory items included in shipments",
                Uuid = "550e8400-e29b-41d4-a716-446655440006",
                OriginId = 8, // Shipment class
                DestinationId = 6, // Inventory class
                ProjectId = 2,
                CreatedBy = "logistics.coordinator@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-5),
                ModifiedBy = "mike.davis@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-5),
                ArchivedAt = null
            },

            // Legacy System Migration Relationships (Archived)
            new Relationship
            {
                Id = 7,
                Name = "Legacy User Owns Legacy Data",
                Description = "Historical relationship mapping legacy users to data",
                Uuid = "550e8400-e29b-41d4-a716-446655440007",
                OriginId = 9, // LegacyUser class
                DestinationId = 10, // LegacyData class
                ProjectId = 3,
                CreatedBy = "system.architect@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                ModifiedBy = "migration.lead@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            }
        };

        await context.Relationships.AddRangeAsync(relationships);
        await context.SaveChangesAsync();
    }

    // Edge Mappings
    public static async Task SeedEdgeMappings(DeeplynxContext context)
    {
        var edgeMappings = new List<EdgeMapping>
        {
            // Customer Analytics Platform Edge Mappings
            new EdgeMapping
            {
                Id = 1,
                OriginParams = @"{""class_name"":""Customer"",""primary_key"":""customer_id"",""extraction_query"":""SELECT customer_id, email, first_name, last_name FROM customers WHERE status = 'active'"",""data_source_table"":""customers""}",
                DestinationParams = @"{""class_name"":""Purchase"",""primary_key"":""purchase_id"",""foreign_key"":""customer_id"",""extraction_query"":""SELECT purchase_id, customer_id, purchase_date, total_amount FROM purchases"",""data_source_table"":""purchases""}",
                RelationshipId = 1,
                OriginId = 1,
                DestinationId = 2,
                ProjectId = 1,
                CreatedBy = "john.smith@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                ModifiedBy = "data.engineer@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-20),
                ArchivedAt = null
            },
            new EdgeMapping
            {
                Id = 2,
                OriginParams = @"{""class_name"":""Customer"",""primary_key"":""customer_id"",""segmentation_rules"":{""premium"":""total_lifetime_value > 5000"",""regular"":""total_lifetime_value BETWEEN 1000 AND 5000""}}",
                DestinationParams = @"{""class_name"":""Segment"",""primary_key"":""segment_id"",""assignment_logic"":""rule_based"",""dynamic_assignment"":true}",
                RelationshipId = 2,
                OriginId = 1,
                DestinationId = 3,
                ProjectId = 1,
                CreatedBy = "sarah.johnson@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                ModifiedBy = "marketing.analyst@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-15),
                ArchivedAt = null
            },

            // Supply Chain Optimization Edge Mappings
            new EdgeMapping
            {
                Id = 3,
                OriginParams = @"{""class_name"":""Supplier"",""primary_key"":""supplier_id"",""performance_metrics"":[""on_time_delivery_rate"",""quality_score"",""cost_competitiveness""]}",
                DestinationParams = @"{""class_name"":""Inventory"",""primary_key"":""inventory_id"",""foreign_key"":""supplier_id"",""supply_chain_tracking"":{""lead_time_calculation"":""DATEDIFF(day, order_date, delivery_date)""}}",
                RelationshipId = 4,
                OriginId = 5,
                DestinationId = 6,
                ProjectId = 2,
                CreatedBy = "mike.davis@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7),
                ModifiedBy = "supply.manager@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-10),
                ArchivedAt = null
            },

            // Legacy System Migration Edge Mappings (Archived)
            new EdgeMapping
            {
                Id = 4,
                OriginParams = @"{""class_name"":""LegacyUser"",""primary_key"":""legacy_user_id"",""transformation_rules"":{""character_encoding"":""EBCDIC_to_UTF8"",""date_format_conversion"":""YYYYMMDD_to_ISO8601""}}",
                DestinationParams = @"{""class_name"":""LegacyData"",""primary_key"":""legacy_data_id"",""foreign_key"":""owner_user_id"",""migration_priority"":{""critical_data_first"":true}}",
                RelationshipId = 7,
                OriginId = 9,
                DestinationId = 10,
                ProjectId = 3,
                CreatedBy = "system.architect@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                ModifiedBy = "migration.lead@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            }
        };

        await context.EdgeMappings.AddRangeAsync(edgeMappings);
        await context.SaveChangesAsync();
    }

    // Record Mappings
    public static async Task SeedRecordMappings(DeeplynxContext context)
    {
        var recordMappings = new List<RecordMapping>
        {
            // Customer Analytics Platform Record Mappings
            new RecordMapping
            {
                Id = 1,
                RecordParams = @"{""record_extraction"":{""source_table"":""customers"",""primary_key_field"":""customer_id"",""extraction_query"":""SELECT customer_id, email, first_name, last_name, status FROM customers WHERE status = 'active'""},""field_mappings"":{""customer_id"":""customer_id"",""email"":""email"",""first_name"":""first_name"",""last_name"":""last_name""}}",
                ClassId = 1,
                ProjectId = 1,
                TagId = 3,
                CreatedBy = "john.smith@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                ModifiedBy = "data.engineer@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-20),
                ArchivedAt = null
            },
            new RecordMapping
            {
                Id = 2,
                RecordParams = @"{""record_extraction"":{""source_table"":""purchases"",""primary_key_field"":""purchase_id"",""extraction_query"":""SELECT purchase_id, customer_id, order_number, purchase_date, total_amount FROM purchases""},""aggregation_rules"":{""customer_metrics"":{""total_purchases"":""COUNT(*) GROUP BY customer_id"",""avg_order_value"":""AVG(total_amount) GROUP BY customer_id""}}}",
                ClassId = 2,
                ProjectId = 1,
                TagId = 1,
                CreatedBy = "sarah.johnson@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                ModifiedBy = "data.analyst@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-15),
                ArchivedAt = null
            },

            // Supply Chain Optimization Record Mappings
            new RecordMapping
            {
                Id = 3,
                RecordParams = @"{""record_extraction"":{""source_table"":""suppliers"",""primary_key_field"":""supplier_id"",""extraction_query"":""SELECT supplier_id, company_name, performance_rating FROM suppliers WHERE status = 'active'""},""performance_calculations"":{""overall_score"":""(performance_rating + quality_score + on_time_delivery_rate * 5) / 3""}}",
                ClassId = 5,
                ProjectId = 2,
                TagId = 5,
                CreatedBy = "mike.davis@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7),
                ModifiedBy = "supply.manager@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-10),
                ArchivedAt = null
            },
            new RecordMapping
            {
                Id = 4,
                RecordParams = @"{""record_extraction"":{""source_table"":""inventory"",""primary_key_field"":""inventory_id"",""extraction_query"":""SELECT inventory_id, product_code, current_quantity, reorder_level FROM inventory""},""inventory_calculations"":{""stock_status"":""CASE WHEN current_quantity <= reorder_level THEN 'reorder_needed' ELSE 'normal' END""}}",
                ClassId = 6,
                ProjectId = 2,
                TagId = 9,
                CreatedBy = "inventory.manager@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                ModifiedBy = "warehouse.system@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-5),
                ArchivedAt = null
            },

            // Legacy System Migration Record Mappings (Archived)
            new RecordMapping
            {
                Id = 5,
                RecordParams = @"{""record_extraction"":{""source_table"":""LEGACY.USERS"",""primary_key_field"":""USR_ID"",""extraction_query"":""SELECT USR_ID, USR_NAME, USR_DEPT FROM LEGACY.USERS WHERE STATUS = 'A'""},""transformation_rules"":{""character_encoding"":""EBCDIC_to_UTF8"",""field_name_mapping"":{""USR_ID"":""legacy_user_id"",""USR_NAME"":""user_name""}}}",
                ClassId = 9,
                ProjectId = 3,
                TagId = 10,
                CreatedBy = "system.architect@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                ModifiedBy = "migration.lead@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            }
        };

        await context.RecordMappings.AddRangeAsync(recordMappings);
        await context.SaveChangesAsync();
    }
    
    // Records
    public static async Task SeedRecords(DeeplynxContext context)
    {
        var records = new List<Record>
        {
            new Record
            {
                Id = 1,
                Uri = "crm://customers/CUST_001",
                Properties = @"{""customer_id"":""CUST_001"",""email"":""john.doe@email.com"",""first_name"":""John"",""last_name"":""Doe"",""phone"":""+1-555-0123"",""registration_date"":""2023-03-15T10:30:00Z"",""status"":""active"",""lifetime_value"":2850.75,""total_orders"":12}",
                OriginalId = "CUST_001",
                Name = "John Doe - Customer Profile",
                ClassId = 1,
                DataSourceId = 1,
                ProjectId = 1,
                MappingId = 1,
                CreatedBy = "john.smith@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                ModifiedBy = "crm.system@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-5),
                ArchivedAt = null
            },
            new Record
            {
                Id = 2,
                Uri = "ecommerce://purchases/PUR_001",
                Properties = @"{""purchase_id"":""PUR_001"",""customer_id"":""CUST_001"",""order_number"":""ORD-2024-001234"",""purchase_date"":""2024-01-15T10:30:00Z"",""total_amount"":299.99,""currency"":""USD"",""payment_method"":""credit_card"",""payment_status"":""completed""}",
                OriginalId = "PUR_001",
                Name = "Purchase Order ORD-2024-001234",
                ClassId = 2,
                DataSourceId = 2,
                ProjectId = 1,
                MappingId = 2,
                CreatedBy = "ecommerce.api@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                ModifiedBy = "order.processor@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-8),
                ArchivedAt = null
            },
            new Record
            {
                Id = 3,
                Uri = "marketing://segments/SEG_PREMIUM",
                Properties = @"{""segment_id"":""SEG_PREMIUM"",""segment_name"":""Premium Customers"",""description"":""High-value customers with frequent purchases"",""customer_count"":1247,""percentage_of_base"":8.5,""marketing_budget_allocation"":0.35}",
                OriginalId = "SEG_PREMIUM",
                Name = "Premium Customer Segment",
                ClassId = 3,
                DataSourceId = 2,
                ProjectId = 1,
                CreatedBy = "sarah.johnson@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-9),
                ModifiedBy = "marketing.analyst@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-3),
                ArchivedAt = null
            },

            // Supply Chain Optimization Records
            new Record
            {
                Id = 4,
                Uri = "erp://suppliers/SUP_ABC123",
                Properties = @"{""supplier_id"":""SUP_ABC123"",""company_name"":""Advanced Components Corp"",""contact_person"":""Sarah Mitchell"",""email"":""sarah.mitchell@advancedcomponents.com"",""phone"":""+1-800-555-0199"",""performance_rating"":4.2,""on_time_delivery_rate"":0.95}",
                OriginalId = "SUP_ABC123",
                Name = "Advanced Components Corp - Supplier",
                ClassId = 5,
                DataSourceId = 3,
                ProjectId = 2,
                MappingId = 3,
                CreatedBy = "mike.davis@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7),
                ModifiedBy = "supply.manager@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-6),
                ArchivedAt = null
            },
            new Record
            {
                Id = 5,
                Uri = "inventory://items/INV_XYZ789",
                Properties = @"{""inventory_id"":""INV_XYZ789"",""product_code"":""WIDGET_A_001"",""supplier_id"":""SUP_ABC123"",""current_quantity"":485,""reserved_quantity"":50,""available_quantity"":435,""reorder_level"":100,""unit_cost"":12.50}",
                OriginalId = "INV_XYZ789",
                Name = "High-Grade Widget Assembly - Inventory",
                ClassId = 6,
                DataSourceId = 3,
                ProjectId = 2,
                CreatedBy = "inventory.manager@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                ModifiedBy = "warehouse.system@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-1),
                ArchivedAt = null
            },
            new Record
            {
                Id = 6,
                Uri = "warehouses://facilities/WH_EAST_001",
                Properties = @"{""warehouse_id"":""WH_EAST_001"",""name"":""East Coast Distribution Center"",""total_capacity_sqft"":125000,""current_utilization"":0.72,""available_space_sqft"":35000,""staff_count"":45,""manager"":""Robert Chen""}",
                OriginalId = "WH_EAST_001",
                Name = "East Coast Distribution Center",
                ClassId = 7,
                DataSourceId = 4,
                ProjectId = 2,
                CreatedBy = "warehouse.manager@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-5),
                ModifiedBy = "facility.admin@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-4),
                ArchivedAt = null
            },
            new Record
            {
                Id = 7,
                Uri = "logistics://shipments/SHIP_OUT_001",
                Properties = @"{""shipment_id"":""SHIP_OUT_001"",""tracking_number"":""TRK123456789"",""origin_warehouse"":""WH_EAST_001"",""carrier"":""FedEx"",""service_type"":""Ground"",""shipment_date"":""2024-06-01T14:30:00Z"",""estimated_delivery"":""2024-06-03T17:00:00Z"",""status"":""delivered""}",
                OriginalId = "SHIP_OUT_001",
                Name = "Outbound Shipment TRK123456789",
                ClassId = 8,
                DataSourceId = 4,
                ProjectId = 2,
                CreatedBy = "logistics.coordinator@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4),
                ModifiedBy = "delivery.tracker@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-7),
                ArchivedAt = null
            },

            // Legacy System Migration Records (Archived)
            new Record
            {
                Id = 8,
                Uri = "legacy://users/USR_LEG_001",
                Properties = @"{""legacy_user_id"":""USR_LEG_001"",""username"":""JDOE001"",""full_name"":""JOHN DOE"",""department"":""SALES"",""role"":""MANAGER"",""migration_status"":""completed"",""migration_date"":""2024-11-20T02:00:00Z""}",
                OriginalId = "USR_LEG_001",
                Name = "John Doe - Legacy User Account",
                ClassId = 9,
                DataSourceId = 5,
                ProjectId = 3,
                CreatedBy = "system.architect@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                ModifiedBy = "migration.lead@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            },
            new Record
            {
                Id = 9,
                Uri = "legacy://data/DATA_LEG_001",
                Properties = @"{""legacy_data_id"":""DATA_LEG_001"",""data_type"":""CUSTOMER_RECORDS"",""owner_user_id"":""USR_LEG_001"",""record_count"":15420,""data_size_mb"":2500.75,""migration_status"":""completed"",""validation_status"":""passed""}",
                OriginalId = "DATA_LEG_001",
                Name = "Legacy Customer Data Records",
                ClassId = 10,
                DataSourceId = 6,
                ProjectId = 3,
                MappingId = 5,
                CreatedBy = "data.migration@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-16),
                ModifiedBy = "migration.validator@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-3),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            }
        };
        await context.Records.AddRangeAsync(records);
        await context.SaveChangesAsync();
    }

    // Edges
    public static async Task SeedEdges(DeeplynxContext context)
    {
        var edges = new List<Edge>
        {
            // Customer Analytics Platform Edges
            new Edge
            {
                Id = 1,
                OriginId = 1, // Customer record
                DestinationId = 2, // Purchase record
                RelationshipId = 1,
                DataSourceId = 1,
                ProjectId = 1,
                CreatedBy = "john.smith@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                ModifiedBy = "data.analyst@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-30),
                ArchivedAt = null
            },
            new Edge
            {
                Id = 2,
                OriginId = 1, // Customer record
                DestinationId = 3, // Segment record
                RelationshipId = 2,
                DataSourceId = 2,
                ProjectId = 1,
                MappingId = 1,
                CreatedBy = "sarah.johnson@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                ModifiedBy = "marketing.analyst@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-25),
                ArchivedAt = null
            },

            // Supply Chain Optimization Edges
            new Edge
            {
                Id = 3,
                OriginId = 4, // Supplier record
                DestinationId = 5, // Inventory record
                RelationshipId = 4,
                DataSourceId = 3,
                ProjectId = 2,
                CreatedBy = "mike.davis@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7),
                ModifiedBy = "supply.manager@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-15),
                ArchivedAt = null
            },
            new Edge
            {
                Id = 4,
                OriginId = 5, // Inventory record
                DestinationId = 6, // Warehouse record
                RelationshipId = 5,
                DataSourceId = 4,
                ProjectId = 2,
                CreatedBy = "warehouse.manager@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                ModifiedBy = "logistics.coordinator@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-12),
                ArchivedAt = null
            },
            new Edge
            {
                Id = 5,
                OriginId = 7, // Shipment record
                DestinationId = 5, // Inventory record
                RelationshipId = 6,
                DataSourceId = 4,
                ProjectId = 2,
                MappingId = 3,
                CreatedBy = "logistics.coordinator@company.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-5),
                ModifiedBy = "mike.davis@company.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-10),
                ArchivedAt = null
            },

            // Legacy System Migration Edges (Archived)
            new Edge
            {
                Id = 6,
                OriginId = 8, // Legacy User record
                DestinationId = 9, // Legacy Data record
                RelationshipId = 7,
                DataSourceId = 5,
                ProjectId = 3,
                CreatedBy = "system.architect@legacy.com",
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                ModifiedBy = "migration.lead@legacy.com",
                ModifiedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4),
                ArchivedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-1)
            }
        };

        await context.Edges.AddRangeAsync(edges);
        await context.SaveChangesAsync();
    }

    
}