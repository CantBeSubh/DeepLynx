// Program.cs
using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();
var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:3001");

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Returns the current date as a formatted string.")]
    public static string CurrentDate(string message) => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    
    [McpServerTool, Description("Add two messages together and returns the message back to the client.")]
    public static string Echo2(string message1, string message2) => $"hello {message1} {message2}";
    
}
