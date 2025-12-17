// Program.cs
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using DotNetEnv;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();
var app = builder.Build();

app.MapMcp("/mcp");

Env.Load();
var mcp_server_url = 
    Environment.GetEnvironmentVariable("MCP_SERVER_URL") 
    ?? "http://localhost:43656";
app.Run(mcp_server_url);
