using System.Text.Json.Serialization;
using deeplynx.business;
using deeplynx.datalayer.MigrationRunner;
using deeplynx.datalayer.Models;
using deeplynx.helpers;
using deeplynx.helpers.Hubs;
using deeplynx.interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;
using Log = Serilog.Log;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;



var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => { options.Limits.MaxRequestBodySize = 2L * 1024 * 1024 * 1024; });

builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024; });

var connectionString = ConnectionStringsProvider.GetPostgresConnectionString(builder.Configuration);

// ----------------------------------
// Logger Setup
// ----------------------------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString,
        "logs",
        schemaName: "deeplynx",
        needAutoCreateTable: true,
        batchSizeLimit: 50,
        period: TimeSpan.FromSeconds(15))
    .CreateLogger();
try
{
    Log.Information("Application starting up");

    builder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog(dispose: true);
    });

    // ----------------------------------
    // CORS Configuration
    // ----------------------------------
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5095",
                    "http://localhost:3000",
                    "http://localhost:3001",
                    "http://ui:3000",
                    "http://localhost:5173",
                    "https://*.cluster.local",
                    "http://*.cluster.local",
                    "https://*.svc.cluster.local",
                    "http://*.svc.cluster.local",
                    "https://deeplynx.*.inl.gov", // Matches deeplynx.dev.inl.gov, deeplynx.acc.inl.gov, etc.
                    "https://deeplynx.inl.gov",
                    "https://deeplynx-*.*.inl.gov") // Matches "deeplynx-thing.domain" namespaces like deeplynx-test.dev
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // ----------------------------------
    // Authentication
    // ----------------------------------
    var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
    var secret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
    var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
    var localDevelopment = Environment.GetEnvironmentVariable("DISABLE_BACKEND_AUTHENTICATION");

    if (string.IsNullOrWhiteSpace(issuer))
        throw new InvalidOperationException("JWT_ISSUER not configured");
    if (string.IsNullOrWhiteSpace(secret))
        throw new InvalidOperationException("JWT_SECRET_KEY not configured");
    if (string.IsNullOrWhiteSpace(audience))
        throw new InvalidOperationException("JWT_AUDIENCE not configured");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddScheme<JwtBearerOptions, NexusAuthenticationMiddleware>(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                options.Authority = issuer;
                if (localDevelopment == "true") options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 }
                };
            });

    builder.Services.AddAuthorization();

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.MaxDepth = 64;
        });

    /*
    ╔════════════════════════════╗
    ║  Dependency Injection      ║
    ╚════════════════════════════╝
    */
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddDbContext<DeeplynxContext>(
        options => options.UseNpgsql(connectionString),
        ServiceLifetime.Transient
    );

    builder.Services.AddSignalR(); // Used for event system pub/sub and notifications

    builder.Services.AddTransient<IRecordBusiness, RecordBusiness>();
    builder.Services.AddTransient<IObjectStorageBusiness, ObjectStorageBusiness>();
    builder.Services.AddTransient<IClassBusiness, ClassBusiness>();
    builder.Services.AddTransient<IProjectBusiness, ProjectBusiness>();
    builder.Services.AddTransient<IEdgeBusiness, EdgeBusiness>();
    builder.Services.AddTransient<IDataSourceBusiness, DataSourceBusiness>();
    builder.Services.AddTransient<IRelationshipBusiness, RelationshipBusiness>();
    builder.Services.AddTransient<ITagBusiness, TagBusiness>();
    builder.Services.AddTransient<ITimeseriesBusiness, TimeseriesBusiness>();
    builder.Services.AddTransient<IUserBusiness, UserBusiness>();
    builder.Services.AddTransient<INotificationBusiness, NotificationBusiness>();
    builder.Services.AddTransient<ITokenBusiness, TokenBusiness>();
    builder.Services.AddTransient<IOauthApplicationBusiness, OauthApplicationBusiness>();


    Console.WriteLine("Program cs: " + connectionString);

    builder.Services.AddTransient<IQueryBusiness, QueryBusiness>();
    builder.Services.AddTransient<IMetadataBusiness, MetadataBusiness>();
    builder.Services.AddTransient<IHistoricalRecordBusiness, HistoricalRecordBusiness>();
    builder.Services.AddTransient<IHistoricalEdgeBusiness, HistoricalEdgeBusiness>();
    builder.Services.AddTransient<IEventBusiness, EventBusiness>();
    // builder.Services.AddTransient<ISubscriptionBusiness, SubscriptionBusiness>();
    builder.Services.AddTransient<FileBusiness>();
    builder.Services.AddTransient<FileFilesystemBusiness>();
    builder.Services.AddTransient<FileAzureBusiness>();
    builder.Services.AddTransient<FileS3Business>();
    builder.Services.AddTransient<IFileBusinessFactory, FileBusinessFactory>();
    builder.Services.AddTransient<IOrganizationBusiness, OrganizationBusiness>();
    builder.Services.AddTransient<IGroupBusiness, GroupBusiness>();
    builder.Services.AddTransient<IRoleBusiness, RoleBusiness>();
    builder.Services.AddTransient<ISensitivityLabelBusiness, SensitivityLabelBusiness>();
    builder.Services.AddTransient<IPermissionBusiness, PermissionBusiness>();
    builder.Services.AddTransient<IProjectRolePermissionService, ProjectRolePermissionService>();
    builder.Services.AddTransient<IOrgRolePermissionService, OrgRolePermissionService>();
    builder.Services.AddTransient<ISysAdminService, SysAdminService>();
    builder.Services.AddTransient<IOauthHandshakeBusiness, OauthHandshakeBusiness>();
    builder.Services.AddTransient<IOrganizationService, OrganizationService>();
    builder.Services.AddTransient<ISavedSearchBusiness, SavedSearchBusiness>();
    builder.Services.AddTransient<IGraphBusiness, GraphBusiness>();
    
    // builder.Services.AddOpenApi(options =>
    // {
    //     options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    //     options.AddDocumentTransformer(async (document, context, cancellationToken) =>
    //     {
    //         document.Info.Version = "v1";
    //         document.Info.Title = "DeepLynx Nexus";
    //         document.Info.Description = "DeepLynx Nexus Api Documentation";
    //         // Add security scheme
    //         document.Components ??= new OpenApiComponents();
    //         document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
    //     
    //         var bearerScheme = new OpenApiSecurityScheme
    //         {
    //             Type = SecuritySchemeType.Http,
    //             Scheme = "bearer",
    //             BearerFormat = "JWT",
    //             Description = "Enter your JWT token"
    //         };
    //     
    //         document.Components.SecuritySchemes["Bearer"] = bearerScheme;
    //         // Create a security scheme reference using the constructor
    //         var securitySchemeReference = new OpenApiSecuritySchemeReference("Bearer", document);
    //         var securityRequirement = new OpenApiSecurityRequirement
    //         {
    //             [securitySchemeReference] = new List<string>()
    //         };
    //         // Apply to all operations
    //         foreach (var path in document.Paths.Values)
    //         {
    //             foreach (var operation in path.Operations.Values)
    //             {
    //                 operation.Security ??= new List<OpenApiSecurityRequirement>();
    //                 operation.Security.Add(securityRequirement);
    //             }
    //         }
    //
    //         document.Tags = new HashSet<OpenApiTag>
    //         {
    //             // parent tags
    //             
    //             // Todo: Add this back after new scaler version
    //             
    //             // new()
    //             // {
    //             //     Name = "Project Management",
    //             //     Description =
    //             //         "Handles project-level operations including classes, records, edges, and relationships"
    //             // },
    //             // new()
    //             // {
    //             //     Name = "Organization Management",
    //             //     Description = "Handles organization-level operations including classes, users, groups, and projects"
    //             // },
    //             
    //             // child tags
    //             new()
    //             {
    //                 Name = "Token",
    //                 Description =
    //                     "Provides endpoints to create tokens and api keys"
    //             },
    //             new()
    //             {
    //                 Name = "Class",
    //                 Description =
    //                     "Handles class management including creation, updates, retrieval, and deletion of class entities."
    //             },
    //             new()
    //             {
    //                 Name = "DataSource",
    //                 Description = "Manages data sources, including their creation, retrieval, updating, and deletion."
    //             },
    //             new()
    //             {
    //                 Name = "Edge",
    //                 Description =
    //                     "Oversees relationships between entities, allowing for the creation, retrieval, updating, and deletion of edges."
    //             },
    //             new()
    //             {
    //                 Name = "Event",
    //                 Description = "Handles Event fetching by project and user subscriptions."
    //             },
    //             new()
    //             {
    //                 Name = "File",
    //                 Description = "Handles operations related to file management"
    //             },
    //             new()
    //             {
    //                 Name = "Group",
    //                 Description = "Handles operations related to Group management"
    //             },
    //             new()
    //             {
    //                 Name = "HistoricalEdge",
    //                 Description =
    //                     "Handles operations related to historical edges, including retrieval and analysis of past relationships."
    //             },
    //             new()
    //             {
    //                 Name = "HistoricalRecord",
    //                 Description =
    //                     "Manages operations related to historical records, including retrieval and analysis of past records."
    //             },
    //             new()
    //             {
    //                 Name = "Metadata",
    //                 Description = "Handles the management and processing of metadata associated with various entities."
    //             },
    //             new()
    //             {
    //                 Name = "Notification",
    //                 Description = "Handles notification operations."
    //             },
    //             new()
    //             {
    //                 Name = "OauthApplication",
    //                 Description = "Handles operations related to registered Oauth2 Application management."
    //             },
    //             new()
    //             {
    //                 Name = "OauthHandshake",
    //                 Description =
    //                     "Facilitates the Oauth2 Handshake between Nexus and external apps, with Nexus acting as an Oauth2 provider."
    //             },
    //             new()
    //             {
    //                 Name = "ObjectStorage",
    //                 Description = "Handles the management and processing of metadata associated with object storages."
    //             },
    //             new()
    //             {
    //                 Name = "Organization",
    //                 Description = "Handles operations related to Organization management"
    //             },
    //             new()
    //             {
    //                 Name = "Permission",
    //                 Description = "Handles operations related to Permission management"
    //             },
    //             new()
    //             {
    //                 Name = "Project",
    //                 Description =
    //                     "Facilitates project lifecycle management, including creating, updating, retrieving, and archiving projects."
    //             },
    //             new()
    //             {
    //                 Name = "Query",
    //                 Description = "Facilitates data filtering operations for efficient data retrieval and management."
    //             },
    //             new()
    //             {
    //                 Name = "Record",
    //                 Description =
    //                     "Manages all operations related to record creation, retrieval, updating, deletion, and tagging."
    //             },
    //             new()
    //             {
    //                 Name = "Relationship",
    //                 Description =
    //                     "Handles complex relationships between various entities, allowing for creation, updates, retrieval, and deletion."
    //             },
    //             new()
    //             {
    //                 Name = "Role",
    //                 Description = "Handles operations related to Role management"
    //             },
    //             new()
    //             {
    //                 Name = "SavedSearch",
    //                 Description = "Handles operations related to saving queries for future re-use"
    //             },
    //             new()
    //             {
    //                 Name = "Sensitivity Label",
    //                 Description = "Handles operations related to Sensitivity Label management"
    //             },
    //             new()
    //             {
    //                 Name = "Subscription",
    //                 Description = "Handles operations related to subscription creation, retrieval, and deletion."
    //             },
    //             new()
    //             {
    //                 Name = "Tag",
    //                 Description =
    //                     "Manages tagging operations for entities, including creating, updating, retrieving, and deleting tags."
    //             },
    //             new()
    //             {
    //                 Name = "Timeseries",
    //                 Description =
    //                     "Handles operations related to time-series data, including querying and uploading time-series data."
    //             },
    //             new()
    //             {
    //                 Name = "User",
    //                 Description =
    //                     "Manages user-related operations, including user creation, updates, retrieval, and authentication processes."
    //             }
    //         };
    //     });
    // });
    
   builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "v1",
            Title = "DeepLynx Nexus API",
            Description = "DeepLynx Nexus API Documentation"
        };

        // Define all your tags with hierarchical names
document.Tags = new HashSet<OpenApiTag>
{
    // Authentication
    new() { Name = "OauthHandshake", Description = "OAuth2 authorization flow" },
    new() { Name = "Token", Description = "API key and JWT token management" },
    
    // Class tags - using / separator for hierarchy
    new() { Name = "Class / Organization", Description = "Organization-level class operations" },
    new() { Name = "Class / Project", Description = "Project-level class operations" },
    
    // DataSource tags
    new() { Name = "DataSource / Organization", Description = "Organization-level data sources" },
    new() { Name = "DataSource / Project", Description = "Project-level data sources" },
    
    // Edge tags
    new() { Name = "Edge / Organization", Description = "Organization-level edges" },
    new() { Name = "Edge / Project", Description = "Project-level edges" },
    
    // Relationship tags
    new() { Name = "Relationship / Organization", Description = "Organization-level relationships" },
    new() { Name = "Relationship / Project", Description = "Project-level relationships" },
    
    // Permission tags
    new() { Name = "Permission / Organization", Description = "Organization-level permissions" },
    new() { Name = "Permission / Project", Description = "Project-level permissions" },
    
    // Role tags
    new() { Name = "Role / Organization", Description = "Organization-level roles" },
    new() { Name = "Role / Project", Description = "Project-level roles" },
    
    // Tag tags
    new() { Name = "Tag / Organization", Description = "Organization-level tags" },
    new() { Name = "Tag / Project", Description = "Project-level tags" },
    
    // ObjectStorage tags
    new() { Name = "ObjectStorage / Organization", Description = "Organization-level storage" },
    new() { Name = "ObjectStorage / Project", Description = "Project-level storage" },
    
    // SensitivityLabel tags
    new() { Name = "SensitivityLabel / Organization", Description = "Organization-level labels" },
    new() { Name = "SensitivityLabel / Project", Description = "Project-level labels" },
    
    // Management
    new() { Name = "Management / Organization", Description = "Organization management" },
    new() { Name = "Management / Project", Description = "Project management" },
    new() { Name = "Management / User", Description = "User management" },
    new() { Name = "Management / Group", Description = "Group management" },
    
    // Data
    new() { Name = "Data / Record", Description = "Record management" },
    new() { Name = "Data / File", Description = "File operations" },
    new() { Name = "Data / Metadata", Description = "Metadata operations" },
    
    // Query
    new() { Name = "Query / Search", Description = "Search and filtering" },
    new() { Name = "Query / SavedSearch", Description = "Saved searches" },
    
    // History
    new() { Name = "History / Record", Description = "Record history" },
    new() { Name = "History / Edge", Description = "Edge history" },
    new() { Name = "History / Event", Description = "Event logs" },
    
    // Other
    new() { Name = "Other / Timeseries", Description = "Time-series data" },
    new() { Name = "Other / Notification", Description = "Notifications" },
    new() { Name = "Other / OauthApplication", Description = "OAuth apps" }
};

// Simplified tag groups - let the naming convention handle hierarchy
var tagGroups = new JsonArray
{
    new JsonObject
    {
        ["name"] = "🔐 Authentication",
        ["tags"] = new JsonArray { "OauthHandshake", "Token" }
    },
    new JsonObject
    {
        ["name"] = "📦 Class",
        ["tags"] = new JsonArray { "Organization - Class", "Project - Class" }
    },
    new JsonObject
    {
        ["name"] = "🔌 DataSource",
        ["tags"] = new JsonArray { "DataSource / Organization", "DataSource / Project" }
    },
    new JsonObject
    {
        ["name"] = "🔗 Edge",
        ["tags"] = new JsonArray { "Edge / Organization", "Edge / Project" }
    },
    new JsonObject
    {
        ["name"] = "↔️ Relationship",
        ["tags"] = new JsonArray { "Relationship / Organization", "Relationship / Project" }
    },
    new JsonObject
    {
        ["name"] = "🔒 Permission",
        ["tags"] = new JsonArray { "Permission / Organization", "Permission / Project" }
    },
    new JsonObject
    {
        ["name"] = "👤 Role",
        ["tags"] = new JsonArray { "Role / Organization", "Role / Project" }
    },
    new JsonObject
    {
        ["name"] = "🏷️ Tag",
        ["tags"] = new JsonArray { "Tag / Organization", "Tag / Project" }
    },
    new JsonObject
    {
        ["name"] = "💾 ObjectStorage",
        ["tags"] = new JsonArray { "ObjectStorage / Organization", "ObjectStorage / Project" }
    },
    new JsonObject
    {
        ["name"] = "🔐 SensitivityLabel",
        ["tags"] = new JsonArray { "SensitivityLabel / Organization", "SensitivityLabel / Project" }
    },
    new JsonObject
    {
        ["name"] = "🏢 Management",
        ["tags"] = new JsonArray { "Management / Organization", "Management / Project", "Management / User", "Management / Group" }
    },
    new JsonObject
    {
        ["name"] = "📄 Data",
        ["tags"] = new JsonArray { "Data / Record", "Data / File", "Data / Metadata" }
    },
    new JsonObject
    {
        ["name"] = "🔍 Query",
        ["tags"] = new JsonArray { "Query / Search", "Query / SavedSearch" }
    },
    new JsonObject
    {
        ["name"] = "📊 History",
        ["tags"] = new JsonArray { "History / Record", "History / Edge", "History / Event" }
    },
    new JsonObject
    {
        ["name"] = "⚙️ Other",
        ["tags"] = new JsonArray { "Other / Timeseries", "Other / Notification", "Other / OauthApplication" }
    }
};

        // Initialize Extensions if null, then wrap in JsonNodeExtension for v2
        document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        document.Extensions["x-tagGroups"] = new JsonNodeExtension(tagGroups);
        
        // Wrap in JsonNodeExtension for v2
        document.Extensions["x-tagGroups"] = new JsonNodeExtension(tagGroups);

        // Security scheme
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token"
        };

        return Task.CompletedTask;
    });
});
 
    

/* ╔════════════════════════════╗
   ║      Apply Migrations      ║
   ╚════════════════════════════╝ */
    await MigrationRunner.ApplyMigrations(connectionString);

/* ╔════════════════════════════╗
   ║      App Configurations    ║
   ╚════════════════════════════╝ */
    var app = builder.Build();

/* ╔════════════════════════════╗
   ║      App Base Path         ║
   ╚════════════════════════════╝ */
    PathString basePath = "/api/v1";
    app.UsePathBase(basePath);

    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors("AllowAll");
    app.UseAuthentication(); // Must be first
    app.UseMiddleware<UserContextMiddleware>(); // Second - sets UserId/Email
    app.UseMiddleware<AuthMiddleware>(); // Third - sets OrganizationId
    app.UseAuthorization(); // Fourth
    app.MapControllers(); // Last

    // Check if the notification service is enabled (defaults to false if not set)
    if (Environment.GetEnvironmentVariable("ENABLE_NOTIFICATION_SERVICE") == "true")
        app.MapHub<EventNotificationHub>("/eventNotificationHub"); // endpoint for real-time notifications with SignalR

    /* ╔════════════════════════════╗
    ║   Scalar Configuration     ║
    ╚════════════════════════════╝ */
    // Always using scalar:
    //if (app.Environment.IsDevelopment()) { ...
    // app.UseOpenApi();
    app.MapOpenApi();

    var customcss = File.ReadAllText("moon.css");
    var hostedLink = Environment.GetEnvironmentVariable("HOSTED_LINK");

    // Conditional image hosting
    var imageSrc = "/images/lynx-white.png";

    // Build the HTML content with our image src string interpolation
    var scalarHeaderContent = $@"
    <div class='references-header'>
      <header class='header t-doc__header'>
        <div class='header-container'>
          <div class='header-item header-item-meta'>
            <a class='header-item-logo'>
              <img
                alt='lynx'
                class='header-item-logo-image'
                src='{imageSrc}'
                style='height: 50px; position: sticky; z-index: 1000; padding-left: 20px;' />
            </a>
          </div>
        </div>
      </header>
    </div>";

    app.MapScalarApiReference(options =>
    {
        options.WithDarkMode()
            .WithBaseServerUrl(basePath.ToString())
            .WithTheme(ScalarTheme.Kepler)
            .WithTitle("DeepLynx Nexus API")
            .WithCustomCss(customcss)
            .AddHeaderContent(scalarHeaderContent);

        if (!string.IsNullOrEmpty(hostedLink))
        {
            var hostedLinkWithApi = string.Concat(hostedLink + "/api/v1");
            options.Servers = new List<ScalarServer> { new(hostedLinkWithApi) };
        }
    });

    app.Run();
}
// ignore entity framework aborting in design. See https://github.com/dotnet/efcore/issues/29923
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design")
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
