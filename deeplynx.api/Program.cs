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


builder.Services.AddAuthorization();
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

var xmlPath = Path.Combine(AppContext.BaseDirectory, "deeplynx.api.xml");
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(async (document, context, cancellationToken) =>
    {
        document.Info.Version = "v1";
        document.Info.Title = "DeepLynx Nexus";
        document.Info.Description =
            "DeepLynx Nexus Api Documentation";
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

