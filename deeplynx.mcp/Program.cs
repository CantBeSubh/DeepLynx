// Program.cs
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
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

[McpServerToolType]
public static class ProjectApiTool
{
    public class Project
    {
        [Description("The database ID of the project")]
        public long Id { get; set; }
        
        [Description("The name of the project")]
        public string Name { get; set; } = null!;
        
        [Description("The abbreviation of the project. Used for easy lookups.")]
        public string? Abbreviation { get; set; }

        [Description("The time the project was last updated in the database")]
        public string LastUpdatedAt { get; set; } = null!;
    
        [Description("The user that last updated the project in the database")]
        public string? LastUpdatedBy { get; set; }
    
        [Description("The project description that contains useful information about the purpose of the project")]
        public string? Description { get; set; }
    
        [Description("A JSON string that contains valuable properties of the project to the user.")]
        public string Config { get; set; } = null!;
    
        [Description("A boolean that indicates whether a project is archived")]
        public bool IsArchived { get; set; }
    
        [Description("The database ID of the organization that the project is in")]
        public long? OrganizationId { get; set; }
        
    }
    
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5095/api/")
    };

    [McpServerTool(UseStructuredContent = true), Description("Get all projects from the API. Returns stringified JSON array of projects. Optional parameters: organizationId (filter by organization), hideArchived (default true).")]
    public static async Task<List<Project>?> GetAllProjects(
        [Description("An optional parameter of the ID of the organization to filter by")]long? organizationId = null, 
        [Description("Indicates whether to hide a project where isArchived is true. This needs to be a boolean value, not a string")]bool hideArchived = true,
        [Description("An optional bearer token for testing")]string? authToken = null)
    {
        try
        {
            
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["hideArchived"] = hideArchived.ToString().ToLower();
            // Build query string
            
            if (organizationId.HasValue)
                query["organizationId"] = organizationId.ToString();
            
            var queryString = query.ToString();
            var url = $"projects/GetAllProjects?{queryString}";

            // Create request
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // Add authorization header if token provided
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            }

            // Make the API call
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
            
            var content = await response.Content.ReadFromJsonAsync<List<Project>>(new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
            });

            return content;
        }
        catch (Exception ex)
        {
            // Return exception as JSON object
            var errorResponse = new
            {
                success = false,
                error = "Exception",
                message = ex.Message,
                stackTrace = ex.StackTrace
            };
            
            throw new Exception(errorResponse.ToString());
        }
    }
}

[McpServerToolType]

public static class RecordTools
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:5095/api/")
    };
    
    [McpServerTool, Description("Gets all of the records that are related to the record with the ID that is provided. " +
                                "Returns related records in a stringified JSON array. ")]
    public static async Task<string> GetRelatedRecords(
        [Description("The ID of the record we want to get related records for.")]long recordId,
        [Description("Indicates wehter to find relationships where the recordId is the origin of the relationship or not")]string isOrigin,
        [Description("The results are paginated. This is the page number that the user wants")] int page,
        [Description("The page size. Default is 20.")] int pageSize = 20,
        [Description("Indicates if user wants to hide archived records. Default is true")] string hideArchived = "true",
        [Description("The optional Auth Bearer token")] string? authToken = null)
    {
        try
        {
            bool isOriginBool = bool.Parse(isOrigin);
            bool hideArchivedBool = bool.Parse(hideArchived);
            var query = HttpUtility.ParseQueryString(string.Empty);
            // Build query string
            query["isOrigin"] = isOriginBool.ToString().ToLower();
            query["hideArchived"] = hideArchivedBool.ToString().ToLower();
            query["recordId"] = recordId.ToString();
            query["page"] = page.ToString();
            query["pageSize"] = pageSize.ToString();
            
            var queryString = query.ToString();
            var url = $"projects/{0}/edges/GetAllEdgesByRecord?{queryString}";

            // Create request
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            // Add authorization header if token provided
            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            }

            // Make the API call
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
            // Read the response content as a string
            var jsonString = await response.Content.ReadAsStringAsync();

            // Parse the JSON string into a JsonDocument
            using var jsonDoc = JsonDocument.Parse(jsonString);

            // Serialize it back with indentation for readability
            return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            // Return exception as JSON object
            var errorResponse = new
            {
                success = false,
                error = "Exception",
                message = ex.Message,
                stackTrace = ex.StackTrace
            };
            
            return new Exception(errorResponse.ToString()).ToString();
        }
    }
}
