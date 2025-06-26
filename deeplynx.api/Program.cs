using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using deeplynx.datalayer.Models;
using deeplynx.business;
using deeplynx.interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(o =>
{
    o.AddPolicy(
            name: "AllowAll",
            builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
        );
});

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
        // Scopes
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
    });

var xmlPath = Path.Combine(AppContext.BaseDirectory, "deeplynx.xml");

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 64; // optional
});
builder.Services.AddHttpContextAccessor();

// Add DbContext with connection string from appsettings.json
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string"
                                           + "'DefaultConnection' not found.");

builder.Services.AddDbContext<DeeplynxContext>(options =>
    options.UseNpgsql(connectionString));

//serves for Dependency Injection
builder.Services.AddTransient<IRecordBusiness, RecordBusiness>();
builder.Services.AddTransient<IClassBusiness, ClassBusiness>();
builder.Services.AddTransient<IProjectBusiness, ProjectBusiness>();
builder.Services.AddTransient<IEdgeBusiness, EdgeBusiness>();
builder.Services.AddTransient<IDataSourceBusiness, DataSourceBusiness>();
builder.Services.AddTransient<IRelationshipBusiness, RelationshipBusiness>();
builder.Services.AddTransient<IRecordMappingBusiness, RecordMappingBusiness>();
builder.Services.AddTransient<IEdgeMappingBusiness, EdgeMappingBusiness>();
builder.Services.AddTransient<ITagBusiness, TagBusiness>();
builder.Services.AddTransient<ILoginBusiness, LoginBusiness>();
builder.Services.AddTransient<ITimeseriesBusiness, TimeseriesBusiness>();
builder.Services.AddTransient<IUserBusiness, UserBusiness>();

builder.Services.AddOpenApi();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o
            .WithTheme(ScalarTheme.Mars)
            .WithTitle("DeepLynx Nexus")
        );
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();