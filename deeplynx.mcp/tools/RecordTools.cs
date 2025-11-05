using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using deeplynx.mcp.helpers;
using ModelContextProtocol.Server;

namespace deeplynx.mcp.tools;

[McpServerToolType]

public static class RecordTools
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri(EnvironmentHelper.GetRequiredEnvironmentVariable("NEXUS_API_URL"))
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
    
    [McpServerTool, Description("Gets all records for a specific project, optionally filtered by datasource and file type. " +
                            "Returns records in a stringified JSON array.")]
    public static async Task<string> GetAllRecords(
        [Description("The ID of the project whose records are to be retrieved")] long projectId,
        [Description("The optional ID of the datasource to filter records by")] long? dataSourceId = null,
        [Description("Indicates if user wants to hide archived records. Default is true")] string hideArchived = "true",
        [Description("Optional file extension to filter by (e.g., pdf, png, jpg)")] string? fileType = null,
        [Description("The optional Auth Bearer token")] string? authToken = null)
    {
        try
        {
            bool hideArchivedBool = bool.Parse(hideArchived);
            var query = HttpUtility.ParseQueryString(string.Empty);
            
            // Build query string
            query["hideArchived"] = hideArchivedBool.ToString().ToLower();
            
            if (dataSourceId.HasValue)
            {
                query["dataSourceId"] = dataSourceId.Value.ToString();
            }
            
            if (!string.IsNullOrWhiteSpace(fileType))
            {
                query["fileType"] = fileType;
            }
            
            var queryString = query.ToString();
            var url = $"projects/{projectId}/records/GetAllRecords?{queryString}";

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
                throw new HttpRequestException($"API request failed with status {response.StatusCode}: {response.ReasonPhrase}");
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
            
            return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
    }
}