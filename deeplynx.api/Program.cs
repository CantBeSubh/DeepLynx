using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using deeplynx.datalayer.Models;
using deeplynx.business;
using deeplynx.interfaces;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;



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
    options.UseNpgsql(connectionString),
        ServiceLifetime.Transient);

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


builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(async (document, context, cancellationToken) =>
    {
        document.Info.Version = "v1";
        document.Info.Title = "DeepLynx Nexus";
        document.Info.Description = "Hello you have reached deeplynx, please please please work this time I am tired of this";
    });
});

var app = builder.Build();

app.UseOpenApi(); 

var customcss = File.ReadAllText("windows95.css");
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o
        .WithTitle("DeepLynx Nexus API")
        .AddMetadata("title", "DeepLynx Nexus API")
        .AddMetadata("version", "1.0.0")
        .AddMetadata("contact", "{ \"name\": \"API Support\",\n    \"url\": \"http://www.example.com/support\",\n    \"email\": \"support@example.com\"}")
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
                        style='height: 50px; position: sticky; z-index: 1000;'>
                    </a>
                  </div>
                </div>
              </header>
            </div>"));
}




app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

