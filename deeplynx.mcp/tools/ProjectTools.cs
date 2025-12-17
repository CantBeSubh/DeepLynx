using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using deeplynx.mcp.helpers;
using ModelContextProtocol.Server;

namespace deeplynx.mcp.tools;

[McpServerToolType]
public static class ProjectTools
{
    public class Organization
    {
        [Description("The database ID of the organization")]
        public long Id { get; set; }

        [Description("The name of the organization")]
        public string Name { get; set; } = null!;

        [Description("The description of the organization")]
        public string? Description { get; set; }

        [Description("A boolean that indicates whether an organization is archived")]
        public bool IsArchived { get; set; }
    }

    public class Project
    {
        [Description("The database ID of the project")]
        public long Id { get; set; }
        
        [Description("The name of the project")]
        public string Name { get; set; } = null!;
        
        [Description("The abbreviation of the project. Used for easy lookups.")]
        public string? Abbreviation { get; set; }

        [Description("The time the project was last updated in the database")]
        public DateTime LastUpdatedAt { get; set; }
    
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
        BaseAddress = new Uri(EnvironmentHelper.GetRequiredEnvironmentVariable("NEXUS_API_URL")),
    };

    [McpServerTool, Description("Get all organizations the user has access to. Returns stringified JSON array of organizations. " +
                                "Use this to find the organizationId needed for other API calls.")]
    public static async Task<string> GetAllOrganizations(
        [Description("Indicates whether to hide archived organizations. Default is true.")] string hideArchived = "true",
        [Description("An optional bearer token for authentication")] string? authToken = null)
    {
        try
        {
            bool hideArchivedBool = bool.Parse(hideArchived);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["hideArchived"] = hideArchivedBool.ToString().ToLower();

            var queryString = query.ToString();
            var url = $"organizations/user?{queryString}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (!string.IsNullOrEmpty(authToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"API request failed with status {response.StatusCode}: {response.ReasonPhrase}. Details: {errorContent}");
            }

            var content = await response.Content.ReadFromJsonAsync<List<Organization>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return JsonSerializer.Serialize(content, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            var errorResponse = new
            {
                success = false,
                error = "Exception",
                message = ex.Message,
                stackTrace = ex.StackTrace
            };

            return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }

    [McpServerTool(UseStructuredContent = true), Description("Get all projects for a specific organization. Returns stringified JSON array of projects. Optional parameters: hideArchived (default true).")]
    public static async Task<string> GetAllProjects(
        [Description("The ID of the organization to list projects from")] long organizationId,
        [Description("Indicates whether to hide archived projects. Default is true.")] string hideArchived = "true",
        [Description("An optional bearer token for authentication")] string? authToken = null)
    {
        try
        {
            bool hideArchivedBool = bool.Parse(hideArchived);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["hideArchived"] = hideArchivedBool.ToString().ToLower();

            var queryString = query.ToString();
            var url = $"organizations/{organizationId}/projects?{queryString}";

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
            
            var content = await response.Content.ReadFromJsonAsync<List<Project>>(new JsonSerializerOptions() 
            { 
                PropertyNameCaseInsensitive = true,
            });
            
            // Serialize it back with indentation for readability
            return JsonSerializer.Serialize(content, new JsonSerializerOptions 
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
            
            throw new Exception(errorResponse.ToString());
        }
    }
}