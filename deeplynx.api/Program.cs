using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using deeplynx.business;
using deeplynx.datalayer.MigrationRunner;
using deeplynx.datalayer.Models;
using deeplynx.graph;
using deeplynx.helpers;
using deeplynx.interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;
using Log = Serilog.Log;

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
                    "https://*.cluster.local",
                    "http://*.cluster.local",
                    "https://*.svc.cluster.local",
                    "http://*.svc.cluster.local",
                    "https://deeplynx.*.inl.gov",  // Matches deeplynx.dev.inl.gov, deeplynx.acc.inl.gov, etc.
                    "https://deeplynx.inl.gov")
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
    var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
    var localDevelopment = Environment.GetEnvironmentVariable("DISABLE_BACKEND_AUTHENTICATION");

    if (string.IsNullOrWhiteSpace(issuer))
        throw new InvalidOperationException("JWT_ISSUER not configured");
    if (string.IsNullOrWhiteSpace(audience))
        throw new InvalidOperationException("JWT_AUDIENCE not configured");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddScheme<JwtBearerOptions, NexusAuthenticationMiddleware>(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                options.Authority = issuer;
                if (localDevelopment == "true")
                {
                    options.RequireHttpsMetadata = false;
                }

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


    // Register Cache Service as a singleton
    var cacheProviderType = Environment.GetEnvironmentVariable("CACHE_PROVIDER_TYPE");
    if (cacheProviderType == "redis")
    {
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            builder.Services.AddSingleton(provider =>
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.AllowAdmin = true;
                return ConnectionMultiplexer.Connect(options);
            });
            builder.Services.AddSingleton<ICacheBusiness, RedisCacheBusiness>();
        }
        else
        {
            throw new Exception("Redis connection string not found in environment variables.");
        }
    }
    else
    {
        // Default to memory cache if CACHE_PROVIDER_TYPE is not set or is not "redis"
        builder.Services.AddSingleton<ICacheBusiness, MemoryCacheBusiness>();
    }

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

    Console.WriteLine("Program cs: " + connectionString);

    builder.Services.AddTransient<IKuzuDatabaseManager>(provider =>
    {
        var configuration = provider.GetRequiredService<IConfiguration>();
        return new KuzuDatabaseManager(configuration, connectionString, "d332f23f");
    });
    builder.Services.AddTransient<IQueryBusiness, QueryBusiness>();
    builder.Services.AddTransient<IMetadataBusiness, MetadataBusiness>();
    builder.Services.AddTransient<IHistoricalRecordBusiness, HistoricalRecordBusiness>();
    builder.Services.AddTransient<IHistoricalEdgeBusiness, HistoricalEdgeBusiness>();
    builder.Services.AddTransient<IEventBusiness, EventBusiness>();
    builder.Services.AddTransient<ISubscriptionBusiness, SubscriptionBusiness>();
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
    
    builder.Services.AddOpenApi(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
        options.AddDocumentTransformer(async (document, context, cancellationToken) =>
        {
            document.Info.Version = "v1";
            document.Info.Title = "DeepLynx Nexus";
            document.Info.Description = "DeepLynx Nexus Api Documentation";

            document.Tags = new HashSet<OpenApiTag>
            {
                new()
                {
                    Name = "Token",
                    Description =
                        "Provides endpoints to create tokens and api keys"
                },
                new()
                {
                    Name = "Class",
                    Description =
                        "Handles class management including creation, updates, retrieval, and deletion of class entities."
                },
                new()
                {
                    Name = "DataSource",
                    Description = "Manages data sources, including their creation, retrieval, updating, and deletion."
                },
                new()
                {
                    Name = "Edge",
                    Description =
                        "Oversees relationships between entities, allowing for the creation, retrieval, updating, and deletion of edges."
                },
                new()
                {
                    Name = "Query",
                    Description = "Facilitates data filtering operations for efficient data retrieval and management."
                },
                new()
                {
                    Name = "HistoricalEdge",
                    Description =
                        "Handles operations related to historical edges, including retrieval and analysis of past relationships."
                },
                new()
                {
                    Name = "HistoricalRecord",
                    Description =
                        "Manages operations related to historical records, including retrieval and analysis of past records."
                },
                new()
                {
                    Name = "KuzuDatabaseManager",
                    Description =
                        "Oversees operations related to Kuzu database management, including data export and querying."
                },
                new()
                {
                    Name = "Metadata",
                    Description = "Handles the management and processing of metadata associated with various entities."
                },
                new()
                {
                    Name = "ObjectStorage",
                    Description = "Handles the management and processing of metadata associated with object storages."
                },
                new()
                {
                    Name = "Project",
                    Description =
                        "Facilitates project lifecycle management, including creating, updating, retrieving, and archiving projects."
                },
                new()
                {
                    Name = "Record",
                    Description =
                        "Manages all operations related to record creation, retrieval, updating, deletion, and tagging."
                },
                new()
                {
                    Name = "Relationship",
                    Description =
                        "Handles complex relationships between various entities, allowing for creation, updates, retrieval, and deletion."
                },
                new()
                {
                    Name = "Tag",
                    Description =
                        "Manages tagging operations for entities, including creating, updating, retrieving, and deleting tags."
                },
                new()
                {
                    Name = "Timeseries",
                    Description =
                        "Handles operations related to time-series data, including querying and uploading time-series data."
                },
                new()
                {
                    Name = "User",
                    Description =
                        "Manages user-related operations, including user creation, updates, retrieval, and authentication processes."
                },
                new()
                {
                    Name = "Event",
                    Description = "Handles Event fetching by project and user subscriptions."
                },
                new()
                {
                    Name = "Subscription",
                    Description = "Handles operations related to subscription creation, retrieval, and deletion."
                },
                new()
                {
                    Name = "File",
                    Description = "Handles operations related to file management"
                },
                new()
                {
                    Name = "Notification",
                    Description = "Handles notification operations."
                },
                new()
                {
                    Name = "SensitivityLabel",
                    Description = "Handles operations related to Sensitivity Label management"
                },
                new()
                {
                    Name = "Group",
                    Description = "Handles operations related to Group management"
                },
                new()
                {
                    Name = "Organization",
                    Description = "Handles operations related to Organization management"
                },
                new()
                {
                    Name = "Role",
                    Description = "Handles operations related to Role management"
                },
                new()
                {
                    Name = "Permission",
                    Description = "Handles operations related to Permission management"
                }
            };
        });
    });

    /* ╔════════════════════════════╗
       ║      Apply Migrations      ║
       ╚════════════════════════════╝ */
    await MigrationRunner.ApplyMigrations(connectionString);

    var app = builder.Build();

    app.UseOpenApi();

    var customcss = File.ReadAllText("moon.css");

    app.UseStaticFiles();

    
    // add endpoint for real-time notifications with SignalR
    app.UseRouting();

    app.MapHub<EventNotificationHub>("/eventNotificationHub");
    
    // We're always using scalar for now.
    //if (app.Environment.IsDevelopment()) { ...
    app.MapOpenApi();
    app.MapScalarApiReference(o => o
        .WithDarkMode()
        .WithTheme(ScalarTheme.Kepler)
        .WithTitle("DeepLynx Nexus API")
        .WithCustomCss(customcss)
        .AddHeaderContent(@"
        <div class='references-header'>
            <header class='header t-doc__header'>
                <div class='header-container'>
                    <div class='header-item header-item-meta'>
                        <a class='header-item-logo'>
                            <img
                                alt='lynx'
                                class='header-item-logo-image'
                                src='/images/lynx-white.png'
                                style='height: 50px; position: sticky; z-index: 1000; padding-left: 20px;'/>
                        </a>
                    </div>
                </div>
            </header>
        </div>"));

    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseMiddleware<UserContextMiddleware>();
    app.MapControllers();
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