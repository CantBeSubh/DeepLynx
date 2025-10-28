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
                Name = "John Smith",
                Email = "john.smith@company.com",
                Password = "$2a$10$N9qo8uLOickgx2ZMRZoMye.hashedpassword1",
                IsArchived = false
            },
            new User
            {
                Name = "Sarah Johnson",
                Email = "sarah.johnson@company.com",
                Password = "$2a$10$N9qo8uLOickgx2ZMRZoMye.hashedpassword2",
                IsArchived = false
            },
            new User
            {
                Name = "Mike Davis",
                Email = "mike.davis@company.com",
                Password = "$2a$10$N9qo8uLOickgx2ZMRZoMye.hashedpassword3", 
                IsArchived = false
            },
            new User
            {
                Name = "System Architect",
                Email = "system.architect@legacy.com",
                Password = "$2a$10$N9qo8uLOickgx2ZMRZoMye.hashedpassword4",
                IsArchived = true
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
                Name = "Customer Analytics Platform",
                Abbreviation = "CAP",
                Description = "Comprehensive customer data analysis and segmentation platform for marketing insights",
                LastUpdatedBy = "john.smith@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
                IsArchived = false
            },
            new Project
            {
                Name = "Supply Chain Optimization",
                Abbreviation = "SCO",
                Description = "Real-time supply chain monitoring and optimization system with predictive analytics",
                LastUpdatedBy = "mike.davis@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-8),
                 IsArchived = false
            },
            new Project
            {
                Name = "Legacy System Migration",
                Abbreviation = "LSM",
                Description = "Migration of legacy data systems to modern cloud-based infrastructure",
                LastUpdatedBy = "system.architect@legacy.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-18),
                 IsArchived = true
            }
        };

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();
    }

    // Classes
    public static async Task SeedClasses(DeeplynxContext context)
    {
        var js_user = await context.Users.FirstAsync(u => u.Email == "john.smith@company.com");
        var md_user = await context.Users.FirstAsync(u => u.Email == "mike.davis@company.com");
        var sa_user = await context.Users.FirstAsync(u => u.Email == "system.architect@legacy.com");
        var sj_user = await context.Users.FirstAsync(u => u.Email == "sarah.johnson@company.com");
       
        var classes = new List<Class>
        {
            // Customer Analytics Platform Classes
            new Class
            {
                Name = "Customer",
                Description = "Customer entity with demographic and behavioral data",
                Uuid = "550e8400-e29b-41d4-a716-446655440101",
                ProjectId = 1,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddDays(-15), // Most recent was ModifiedAt
                LastUpdatedBy = js_user.Id, // Most recent modifier
                IsArchived = false
            },
            new Class
            {
                Name = "Purchase",
                Description = "Customer purchase transactions and order history",
                Uuid = "550e8400-e29b-41d4-a716-446655440102",
                ProjectId = 1,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                LastUpdatedBy =sj_user.Id,
                IsArchived = false
            },
            new Class
            {
                Name = "Segment",
                Description = "Customer segmentation categories and rules",
                Uuid = "550e8400-e29b-41d4-a716-446655440103",
                ProjectId = 1,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                LastUpdatedBy = sa_user.Id,
                IsArchived = false
            },
            new Class
            {
                Name = "Product",
                Description = "Product catalog and inventory information",
                Uuid = "550e8400-e29b-41d4-a716-446655440104",
                ProjectId = 1,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                LastUpdatedBy = sa_user.Id,
                IsArchived = false
            },

            // Supply Chain Optimization Classes
            new Class
            {
                Name = "Supplier",
                Description = "Supplier information and performance metrics",
                Uuid = "550e8400-e29b-41d4-a716-446655440201",
                ProjectId = 2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                LastUpdatedBy = md_user.Id,
                IsArchived = false
            },
            new Class
            {
                Name = "Inventory",
                Description = "Product inventory levels and warehouse data",
                Uuid = "550e8400-e29b-41d4-a716-446655440202",
                ProjectId = 2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                LastUpdatedBy =md_user.Id,
                IsArchived = false
               
            },
            new Class
            {
                Name = "Warehouse",
                Description = "Warehouse facility information and capacity data",
                Uuid = "550e8400-e29b-41d4-a716-446655440203",
                ProjectId = 2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                LastUpdatedBy = sj_user.Id,
                IsArchived = false
               
            },
            new Class
            {
                Name = "Shipment",
                Description = "Shipping and logistics tracking information",
                Uuid = "550e8400-e29b-41d4-a716-446655440204",
                ProjectId = 2,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                LastUpdatedBy = md_user.Id,
                IsArchived = false
            },

            // Legacy System Migration Classes (Archived)
            new Class
            {
                Name = "LegacyUser",
                Description = "User accounts from legacy system requiring migration",
                Uuid = "550e8400-e29b-41d4-a716-446655440301",
                ProjectId = 3,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                LastUpdatedBy =sa_user.Id,
                IsArchived = false
            },
            new Class
            {
                Name = "LegacyData",
                Description = "Historical data records from legacy database",
                Uuid = "550e8400-e29b-41d4-a716-446655440302",
                ProjectId = 3,
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                LastUpdatedBy = sa_user.Id,
                IsArchived = false
              
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
                LastUpdatedBy = "john.smith@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),
                IsArchived = false
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
                LastUpdatedBy= "sarah.johnson@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                IsArchived = false
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
                LastUpdatedBy = "mike.davis@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-8),
                IsArchived = false
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
                LastUpdatedBy = "iot.specialist@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                IsArchived = false
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
                LastUpdatedBy= "system.architect@legacy.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-18),
                IsArchived = false
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
                LastUpdatedBy = "system.architect@legacy.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                IsArchived = false
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
            new Tag { Name = "Analytics", ProjectId = 1, LastUpdatedBy = "john.smith@company.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-12),  IsArchived = false },
            new Tag { Name = "Marketing", ProjectId = 1, LastUpdatedBy = "sarah.johnson@company.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11), IsArchived = false },
            new Tag { Name = "Customer Data", ProjectId = 1, LastUpdatedBy = "john.smith@company.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10), IsArchived = false },
            new Tag { Name = "Business Intelligence", ProjectId = 1, LastUpdatedBy = "sarah.johnson@company.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-9), IsArchived = false },

            // Supply Chain Optimization Tags
            new Tag { Name = "Logistics", ProjectId = 2, LastUpdatedBy = "mike.davis@company.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-8), IsArchived = false },
            new Tag { Name = "Optimization", ProjectId = 2, LastUpdatedBy = "mike.davis@company.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7),  IsArchived = false },
            new Tag { Name = "Real-time Monitoring", ProjectId = 2, LastUpdatedBy = "supply.manager@company.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6), IsArchived = false },
            new Tag { Name = "Predictive Analytics", ProjectId = 2, LastUpdatedBy = "data.scientist@company.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-5), IsArchived = false },
            new Tag { Name = "Inventory Management", ProjectId = 2, LastUpdatedBy = "warehouse.admin@company.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4),  IsArchived = false },

            // Legacy System Migration Tags (Archived)
            new Tag { Name = "Migration", ProjectId = 3, LastUpdatedBy = "system.architect@legacy.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-18), IsArchived = true },
            new Tag { Name = "Legacy Systems", ProjectId = 3, LastUpdatedBy = "system.architect@legacy.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17), IsArchived = true },
            new Tag { Name = "Data Transformation", ProjectId = 3, LastUpdatedBy = "data.migration@legacy.com", LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-16), IsArchived = true }
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
                Name = "Customer Makes Purchase",
                Description = "Defines the relationship between customers and their purchase transactions",
                Uuid = "550e8400-e29b-41d4-a716-446655440001",
                OriginId = 1, // Customer class
                DestinationId = 2, // Purchase class
                ProjectId = 1,
                LastUpdatedBy = "john.smith@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                 IsArchived = false
            },
            new Relationship
            {
                Name = "Customer Belongs To Segment",
                Description = "Associates customers with their market segmentation categories",
                Uuid = "550e8400-e29b-41d4-a716-446655440002",
                OriginId = 1, // Customer class
                DestinationId = 3, // Segment class
                ProjectId = 1,
                LastUpdatedBy = "sarah.johnson@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                 IsArchived = false
            },
            new Relationship
            {
                Name = "Purchase Contains Products",
                Description = "Links purchase transactions to specific products",
                Uuid = "550e8400-e29b-41d4-a716-446655440003",
                OriginId = 2, // Purchase class
                DestinationId = 4, // Product class
                ProjectId = 1,
                LastUpdatedBy = "data.analyst@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-9),
                 IsArchived = false
            },

            // Supply Chain Optimization Relationships
            new Relationship
            {
                Name = "Supplier Provides Inventory",
                Description = "Establishes supply relationship between suppliers and inventory",
                Uuid = "550e8400-e29b-41d4-a716-446655440004",
                OriginId = 5, // Supplier class
                DestinationId = 6, // Inventory class
                ProjectId = 2,
                LastUpdatedBy = "mike.davis@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7),
                 IsArchived = false
            },
            new Relationship
            {
                Name = "Inventory Stored In Warehouse",
                Description = "Maps inventory items to warehouse storage locations",
                Uuid = "550e8400-e29b-41d4-a716-446655440005",
                OriginId = 6, // Inventory class
                DestinationId = 7, // Warehouse class
                ProjectId = 2,
                LastUpdatedBy = "warehouse.manager@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                 IsArchived = false
            },
            new Relationship
            {
                Name = "Shipment Contains Inventory",
                Description = "Defines inventory items included in shipments",
                Uuid = "550e8400-e29b-41d4-a716-446655440006",
                OriginId = 8, // Shipment class
                DestinationId = 6, // Inventory class
                ProjectId = 2,
                LastUpdatedBy = "logistics.coordinator@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-5),
                 IsArchived = false
            },

            // Legacy System Migration Relationships (Archived)
            new Relationship
            {
                Name = "Legacy User Owns Legacy Data",
                Description = "Historical relationship mapping legacy users to data",
                Uuid = "550e8400-e29b-41d4-a716-446655440007",
                OriginId = 9, // LegacyUser class
                DestinationId = 10, // LegacyData class
                ProjectId = 3,
                LastUpdatedBy = "system.architect@legacy.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                 IsArchived = true
            }
        };

        await context.Relationships.AddRangeAsync(relationships);
        await context.SaveChangesAsync();
    }
    
    // Records
    public static async Task SeedRecords(DeeplynxContext context)
    {
        var records = new List<Record>
        {
            new Record
            {
                Uri = "crm://customers/CUST_001",
                Properties = @"{""customer_id"":""CUST_001"",""email"":""john.doe@email.com"",""first_name"":""John"",""last_name"":""Doe"",""phone"":""+1-555-0123"",""registration_date"":""2023-03-15T10:30:00Z"",""status"":""active"",""lifetime_value"":2850.75,""total_orders"":12}",
                OriginalId = "CUST_001",
                Name = "John Doe - Customer Profile",
                ClassId = 1,
                DataSourceId = 1,
                ProjectId = 1,
                LastUpdatedBy = "john.smith@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                 IsArchived = false
            },
            new Record
            {
                Uri = "ecommerce://purchases/PUR_001",
                Properties = @"{""purchase_id"":""PUR_001"",""customer_id"":""CUST_001"",""order_number"":""ORD-2024-001234"",""purchase_date"":""2024-01-15T10:30:00Z"",""total_amount"":299.99,""currency"":""USD"",""payment_method"":""credit_card"",""payment_status"":""completed""}",
                OriginalId = "PUR_001",
                Name = "Purchase Order ORD-2024-001234",
                ClassId = 2,
                DataSourceId = 2,
                ProjectId = 1,
                LastUpdatedBy = "ecommerce.api@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                 IsArchived = false
            },
            new Record
            {
                Uri = "marketing://segments/SEG_PREMIUM",
                Properties = @"{""segment_id"":""SEG_PREMIUM"",""segment_name"":""Premium Customers"",""description"":""High-value customers with frequent purchases"",""customer_count"":1247,""percentage_of_base"":8.5,""marketing_budget_allocation"":0.35}",
                OriginalId = "SEG_PREMIUM",
                Name = "Premium Customer Segment",
                ClassId = 3,
                DataSourceId = 2,
                ProjectId = 1,
                LastUpdatedBy = "sarah.johnson@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-9),
                 IsArchived = false
            },

            // Supply Chain Optimization Records
            new Record
            {
                Uri = "erp://suppliers/SUP_ABC123",
                Properties = @"{""supplier_id"":""SUP_ABC123"",""company_name"":""Advanced Components Corp"",""contact_person"":""Sarah Mitchell"",""email"":""sarah.mitchell@advancedcomponents.com"",""phone"":""+1-800-555-0199"",""performance_rating"":4.2,""on_time_delivery_rate"":0.95}",
                OriginalId = "SUP_ABC123",
                Name = "Advanced Components Corp - Supplier",
                ClassId = 5,
                DataSourceId = 3,
                ProjectId = 2,
                LastUpdatedBy = "mike.davis@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7),
                 IsArchived = false
            },
            new Record
            {
                Uri = "inventory://items/INV_XYZ789",
                Properties = @"{""inventory_id"":""INV_XYZ789"",""product_code"":""WIDGET_A_001"",""supplier_id"":""SUP_ABC123"",""current_quantity"":485,""reserved_quantity"":50,""available_quantity"":435,""reorder_level"":100,""unit_cost"":12.50}",
                OriginalId = "INV_XYZ789",
                Name = "High-Grade Widget Assembly - Inventory",
                ClassId = 6,
                DataSourceId = 3,
                ProjectId = 2,
                LastUpdatedBy = "inventory.manager@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                 IsArchived = false
            },
            new Record
            {
                Uri = "warehouses://facilities/WH_EAST_001",
                Properties = @"{""warehouse_id"":""WH_EAST_001"",""name"":""East Coast Distribution Center"",""total_capacity_sqft"":125000,""current_utilization"":0.72,""available_space_sqft"":35000,""staff_count"":45,""manager"":""Robert Chen""}",
                OriginalId = "WH_EAST_001",
                Name = "East Coast Distribution Center",
                ClassId = 7,
                DataSourceId = 4,
                ProjectId = 2,
                LastUpdatedBy = "warehouse.manager@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-5),
                 IsArchived = false
            },
            new Record
            {
                Uri = "logistics://shipments/SHIP_OUT_001",
                Properties = @"{""shipment_id"":""SHIP_OUT_001"",""tracking_number"":""TRK123456789"",""origin_warehouse"":""WH_EAST_001"",""carrier"":""FedEx"",""service_type"":""Ground"",""shipment_date"":""2024-06-01T14:30:00Z"",""estimated_delivery"":""2024-06-03T17:00:00Z"",""status"":""delivered""}",
                OriginalId = "SHIP_OUT_001",
                Name = "Outbound Shipment TRK123456789",
                ClassId = 8,
                DataSourceId = 4,
                ProjectId = 2,
                LastUpdatedBy = "logistics.coordinator@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-4),
                 IsArchived = false
            },

            // Legacy System Migration Records (Archived)
            new Record
            {
                Uri = "legacy://users/USR_LEG_001",
                Properties = @"{""legacy_user_id"":""USR_LEG_001"",""username"":""JDOE001"",""full_name"":""JOHN DOE"",""department"":""SALES"",""role"":""MANAGER"",""migration_status"":""completed"",""migration_date"":""2024-11-20T02:00:00Z""}",
                OriginalId = "USR_LEG_001",
                Name = "John Doe - Legacy User Account",
                ClassId = 9,
                DataSourceId = 5,
                ProjectId = 3,
                LastUpdatedBy = "system.architect@legacy.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                 IsArchived = true
            },
            new Record
            {
                Uri = "legacy://data/DATA_LEG_001",
                Properties = @"{""legacy_data_id"":""DATA_LEG_001"",""data_type"":""CUSTOMER_RECORDS"",""owner_user_id"":""USR_LEG_001"",""record_count"":15420,""data_size_mb"":2500.75,""migration_status"":""completed"",""validation_status"":""passed""}",
                OriginalId = "DATA_LEG_001",
                Name = "Legacy Customer Data Records",
                ClassId = 10,
                DataSourceId = 6,
                ProjectId = 3,
                LastUpdatedBy = "data.migration@legacy.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-16),
                 IsArchived = true
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
                OriginId = 1, // Customer record
                DestinationId = 2, // Purchase record
                RelationshipId = 1,
                DataSourceId = 1,
                ProjectId = 1,
                LastUpdatedBy = "john.smith@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-11),
                 IsArchived = false
            },
            new Edge
            {
                OriginId = 1, // Customer record
                DestinationId = 3, // Segment record
                RelationshipId = 2,
                DataSourceId = 2,
                ProjectId = 1,
                LastUpdatedBy = "sarah.johnson@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-10),
                 IsArchived = false
            },

            // Supply Chain Optimization Edges
            new Edge
            {
                OriginId = 4, // Supplier record
                DestinationId = 5, // Inventory record
                RelationshipId = 4,
                DataSourceId = 3,
                ProjectId = 2,
                LastUpdatedBy = "mike.davis@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-7),
                 IsArchived = false
            },
            new Edge
            {
                OriginId = 5, // Inventory record
                DestinationId = 6, // Warehouse record
                RelationshipId = 5,
                DataSourceId = 4,
                ProjectId = 2,
                LastUpdatedBy = "warehouse.manager@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-6),
                 IsArchived = false
            },
            new Edge
            {
                OriginId = 7, // Shipment record
                DestinationId = 5, // Inventory record
                RelationshipId = 6,
                DataSourceId = 4,
                ProjectId = 2,
                LastUpdatedBy = "logistics.coordinator@company.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-5),
                 IsArchived = false
            },

            // Legacy System Migration Edges (Archived)
            new Edge
            {
                OriginId = 8, // Legacy User record
                DestinationId = 9, // Legacy Data record
                RelationshipId = 7,
                DataSourceId = 5,
                ProjectId = 3,
                LastUpdatedBy = "system.architect@legacy.com",
                LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddMonths(-17),
                 IsArchived = true
            }
        };

        await context.Edges.AddRangeAsync(edges);
        await context.SaveChangesAsync();
    }

    
}