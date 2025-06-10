using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;  
using deeplynx.datalayer.Models;
using deeplynx.business;
using deeplynx.interfaces;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(o =>
{
    o.AddPolicy(
            name:"AllowAll",
            builder  => builder.AllowAnyOrigin()
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
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
// }).AddJwtBearer(options =>
// {
//     options.SaveToken = true;
//     options.RequireHttpsMetadata = false;
//     options.TokenValidationParameters = new TokenValidationParameters()
//     {
//         ValidateIssuer = false,
//         ValidateAudience = false
//     };
// });
builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 64; // optional
});
builder.Services.AddHttpContextAccessor();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
builder.Services.AddTransient<IRoleBusiness, RoleBusiness>();
builder.Services.AddTransient<IRecordMappingBusiness, RecordMappingBusiness>();
builder.Services.AddTransient<IEdgeMappingBusiness, EdgeMappingBusiness>();
builder.Services.AddTransient<ITagBusiness, TagBusiness>();
builder.Services.AddTransient<ILoginBusiness, LoginBusiness>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();