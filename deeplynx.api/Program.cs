using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using deeplynx.datalayer.Models;
using deeplynx.business;
using deeplynx.interfaces;
using deeplynx.graph;
using Microsoft.OpenApi.Models;
using Serilog;
using Log = Serilog.Log;

var builder = WebApplication.CreateBuilder(args);

var connectionString = ConnectionStringsProvider.GetPostgresConnectionString(builder.Configuration);

// ----------------------------------
// Logger Setup
// ----------------------------------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext() 
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: connectionString,
        tableName: "logs",
        schemaName:"deeplynx",
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
                    "http://localhost:3000") //Added this to make work in Dev env, might need to change for Prod env.
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // ----------------------------------
    // Authentication
    // ----------------------------------
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";
            options.DefaultChallengeScheme = "oidc";
        })
        .AddCookie("Cookies")
        .AddOpenIdConnect("oidc", options =>
        {
            options.Authority = "https://identity-preview.inl.gov/";
            options.ClientId = "client-id";
            options.ClientSecret = "secret";
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
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

    builder.Services.AddTransient<IRecordBusiness, RecordBusiness>();
    builder.Services.AddTransient<IObjectStorageBusiness, ObjectStorageBusiness>();
    builder.Services.AddTransient<IClassBusiness, ClassBusiness>();
    builder.Services.AddTransient<IProjectBusiness, ProjectBusiness>();
    builder.Services.AddTransient<IEdgeBusiness, EdgeBusiness>();
    builder.Services.AddTransient<IDataSourceBusiness, DataSourceBusiness>();
    builder.Services.AddTransient<IRelationshipBusiness, RelationshipBusiness>();
    builder.Services.AddTransient<IRecordMappingBusiness, RecordMappingBusiness>();
    builder.Services.AddTransient<IEdgeMappingBusiness, EdgeMappingBusiness>();
    builder.Services.AddTransient<ITagBusiness, TagBusiness>();
    builder.Services.AddTransient<ITimeseriesBusiness, TimeseriesBusiness>();
    builder.Services.AddTransient<IUserBusiness, UserBusiness>();
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
    builder.Services.AddTransient<FileBusiness>();
    builder.Services.AddTransient<FileFilesystemBusiness>();
    builder.Services.AddTransient<FileAzureBusiness>();
    builder.Services.AddTransient<FileS3Business>();
    builder.Services.AddTransient<IFileBusinessFactory, FileBusinessFactory>();

    
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "deeplynx.api.xml");

    builder.Services.AddOpenApi(options =>
    {
        options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
        options.AddDocumentTransformer(async (document, context, cancellationToken) =>
        {
            document.Info.Version = "v1";
            document.Info.Title = "DeepLynx Nexus";
            document.Info.Description = "DeepLynx Nexus Api Documentation";

            document.Tags = new HashSet<OpenApiTag>
            {
                new OpenApiTag
                {
                    Name = "Class",
                    Description =
                        "Handles class management including creation, updates, retrieval, and deletion of class entities."
                },
                new OpenApiTag
                {
                    Name = "DataSource",
                    Description = "Manages data sources, including their creation, retrieval, updating, and deletion."
                },
                new OpenApiTag
                {
                    Name = "Edge",
                    Description =
                        "Oversees relationships between entities, allowing for the creation, retrieval, updating, and deletion of edges."
                },
                new OpenApiTag
                {
                    Name = "EdgeMapping",
                    Description =
                        "Manages mappings of edges to entities, allowing for creation, updating, retrieval, and deletion."
                },
                new OpenApiTag
                {
                    Name = "Query",
                    Description = "Facilitates data filtering operations for efficient data retrieval and management."
                },
                new OpenApiTag
                {
                    Name = "HistoricalEdge",
                    Description =
                        "Handles operations related to historical edges, including retrieval and analysis of past relationships."
                },
                new OpenApiTag
                {
                    Name = "HistoricalRecord",
                    Description =
                        "Manages operations related to historical records, including retrieval and analysis of past records."
                },
                new OpenApiTag
                {
                    Name = "KuzuDatabaseManager",
                    Description =
                        "Oversees operations related to Kuzu database management, including data export and querying."
                },
                new OpenApiTag
                {
                    Name = "Metadata",
                    Description = "Handles the management and processing of metadata associated with various entities."
                },
                new OpenApiTag
                {
                    Name = "ObjectStorage",
                    Description = "Handles the management and processing of metadata associated with object storages."
                },
                new OpenApiTag
                {
                    Name = "Project",
                    Description =
                        "Facilitates project lifecycle management, including creating, updating, retrieving, and archiving projects."
                },
                new OpenApiTag
                {
                    Name = "Record",
                    Description =
                        "Manages all operations related to record creation, retrieval, updating, deletion, and tagging."
                },
                new OpenApiTag
                {
                    Name = "RecordMapping",
                    Description =
                        "Facilitates the mapping of records to other entities, including creation, updating, retrieval, and deletion."
                },
                new OpenApiTag
                {
                    Name = "Relationship",
                    Description =
                        "Handles complex relationships between various entities, allowing for creation, updates, retrieval, and deletion."
                },
                new OpenApiTag
                {
                    Name = "Tag",
                    Description =
                        "Manages tagging operations for entities, including creating, updating, retrieving, and deleting tags."
                },
                new OpenApiTag
                {
                    Name = "Timeseries",
                    Description =
                        "Handles operations related to time-series data, including querying and uploading time-series data."
                },
                new OpenApiTag
                {
                    Name = "User",
                    Description =
                        "Manages user-related operations, including user creation, updates, retrieval, and authentication processes."
                },
                new OpenApiTag
                {
                    Name = "Event",
                    Description = "Handles Event fetching by project and user subscriptions."
                },
                new OpenApiTag
                {
                    Name = "File",
                    Description = "Handles operations related to file management"
                }
            };
        });
    });

    var app = builder.Build();

    app.UseOpenApi();

    var customcss = File.ReadAllText("moon.css");
    app.UseStaticFiles();
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(o => o
            .WithDarkMode(true)
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
                        style='height: 50px; position: sticky; z-index: 1000; padding-left: 20px;' />
                    </a>
                  </div>
                </div>
              </header>
            </div>"));
    }

    app.UseCors("AllowAll"); //Added this to make work in Dev env, might need to change for Prod env.
    app.UseHttpsRedirection();
    app.UseAuthorization();
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

