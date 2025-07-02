using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using deeplynx.datalayer.Models;
using deeplynx.business;
using deeplynx.interfaces;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------
// CORS Configuration
// ----------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000") //Added this to make work in Dev env, might need to change for Prod env.
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

// ----------------------------------
// Controllers and JSON Options
// ----------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64;
    });

// ----------------------------------
// Dependency Injection
// ----------------------------------
builder.Services.AddHttpContextAccessor();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<DeeplynxContext>(
    options => options.UseNpgsql(connectionString),
    ServiceLifetime.Transient
);

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

// ----------------------------------
// OpenAPI / Swagger
// ----------------------------------
builder.Services.AddOpenApi();

var app = builder.Build();

// ----------------------------------
// Dev Environment API Explorer
// ----------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTheme(ScalarTheme.Mars)
        .AddMetadata("title", "DeepLynx Nexus"));
}

// ----------------------------------
// Middleware Pipeline
// ----------------------------------
app.UseCors("AllowAll");//Added this to make work in Dev env, might need to change for Prod env.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
