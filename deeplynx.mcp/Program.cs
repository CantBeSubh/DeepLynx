// Program.cs
using deeplynx.mcp.helpers;
using DotNetEnv;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// load environment variables
Env.Load();

// used to access request headers in tools
builder.Services.AddHttpContextAccessor();

// Register token service as singleton (manages token cache across requests)
builder.Services.AddSingleton<ITokenHelper, TokenHelper>();

// register the authed HTTP client factory as scoped (per-request)
builder.Services.AddScoped<IAuthenticatedHttpClientFactory, AuthenticatedHttpClientFactory>();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

app.MapMcp("/mcp");

var mcp_server_url = 
    Environment.GetEnvironmentVariable("MCP_SERVER_URL") 
    ?? "http://localhost:43656";
app.Run(mcp_server_url);
