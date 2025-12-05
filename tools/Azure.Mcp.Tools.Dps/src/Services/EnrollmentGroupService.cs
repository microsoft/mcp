using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Dps.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Dps.Services;

[JsonSerializable(typeof(QueryRequest))]
internal partial class QueryequestJsonContext : JsonSerializerContext
{
}

internal sealed class QueryRequest
{
    private const string QueryAllEnrollmentGroups = "SELECT * FROM enrollmentGroups";

    [JsonPropertyName("query")]
    public string Query { get; } = QueryAllEnrollmentGroups;
}

[JsonSerializable(typeof(EnrollmentGroup))]
internal partial class EnrollmentGroupJsonContext : JsonSerializerContext
{
}

[JsonSerializable(typeof(List<EnrollmentGroup>))]
internal partial class EnrollmentGroupListJsonContext : JsonSerializerContext
{
}

public class EnrollmentGroupService(
    ILogger<EnrollmentGroupService> logger,
    ISubscriptionService subscriptionService,
    ITenantService tenantService) : BaseAzureResourceService(subscriptionService, tenantService), IEnrollmentGroupService
{
    private readonly ILogger<EnrollmentGroupService> _logger = logger;
    private const string ApiVersion = "2021-10-01";
    private static readonly string[] s_scopes = ["https://azure-devices-provisioning.net/.default"];

    private async Task<HttpClient> GetHttpClientAsync(
        string dpsHostName,
        CancellationToken cancellationToken = default)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri($"https://{dpsHostName}/")
        };
        TokenCredential cred = await GetCredential(cancellationToken);
        AccessToken token = await cred.GetTokenAsync(
            new TokenRequestContext(
                s_scopes),
            cancellationToken);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.TokenType, token.Token);
        return httpClient;
    }

    public async Task<EnrollmentGroup> CreateOrUpdateAsync(
        string dpsHostName,
        EnrollmentGroup enrollmentGroup,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating or updating enrollment group {EnrollmentGroupId} in DPS instance: {DpsHostName}", enrollmentGroup.EnrollmentGroupId, dpsHostName);
        var path = new Uri($"/enrollmentGroups/{enrollmentGroup.EnrollmentGroupId}?api-version={ApiVersion}", UriKind.Relative);


        using HttpClient httpClient = await GetHttpClientAsync(dpsHostName, cancellationToken);

        using HttpResponseMessage getResponse = await httpClient.GetAsync(path, cancellationToken);

        if (getResponse.IsSuccessStatusCode)
        {
            _logger.LogInformation("Enrollment group {EnrollmentGroupId} already exists in DPS instance: {DpsHostName}, it will be updated.", enrollmentGroup.EnrollmentGroupId, dpsHostName);
            var egContent = await getResponse.Content.ReadAsStringAsync(cancellationToken);
            var egExisting = JsonSerializer.Deserialize(
                egContent,
                EnrollmentGroupJsonContext.Default.EnrollmentGroup);
            if (egExisting != null)
            {
                enrollmentGroup.CreatedDateTimeUtc = egExisting.CreatedDateTimeUtc;
                enrollmentGroup.Etag = egExisting.Etag;
                // add etag headerfor concurrency control
                httpClient.DefaultRequestHeaders.IfMatch.Add(new EntityTagHeaderValue($"\"{egExisting.Etag}\""));
            }
        }
        else
        {
            _logger.LogInformation("Enrollment group {EnrollmentGroupId} does not exist in DPS instance: {DpsHostName}, it will be created.", enrollmentGroup.EnrollmentGroupId, dpsHostName);
        }

        using var jsonContent = new StringContent(
            JsonSerializer.Serialize(enrollmentGroup, EnrollmentGroupJsonContext.Default.EnrollmentGroup),
            Encoding.UTF8,
            "application/json");

        Console.WriteLine(JsonSerializer.Serialize(enrollmentGroup, EnrollmentGroupJsonContext.Default.EnrollmentGroup));

        using HttpResponseMessage createResponse = await httpClient.PutAsync(path, jsonContent, cancellationToken);

        var content = await createResponse.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine(content);
        Console.WriteLine(createResponse.StatusCode);

        createResponse.EnsureSuccessStatusCode();
        // parse into EnrollmentGroup using source-generated context for AOT safety
        EnrollmentGroup? createdEnrollmentGroup = JsonSerializer.Deserialize(
            content,
            EnrollmentGroupJsonContext.Default.EnrollmentGroup);
        return createdEnrollmentGroup ?? throw new InvalidOperationException($"Failed to create or update enrollment group {enrollmentGroup.EnrollmentGroupId} in DPS instance {dpsHostName}.");
    }

    public async Task<EnrollmentGroup> GetEnrollmentGroupAsync(
        string dpsHostName,
        string enrollmentGroupId,
        CancellationToken cancellationToken = default)
    {
        // GET https://your-dps.azure-devices-provisioning.net/enrollmentGroups/{id}?api-version=2021-10-01
        _logger.LogInformation("Getting enrollment group {EnrollmentGroupId} in DPS instance: {DpsHostName}", enrollmentGroupId, dpsHostName);
        var path = new Uri($"/enrollmentGroups/{enrollmentGroupId}?api-version={ApiVersion}", UriKind.Relative);
        using HttpClient httpClient = await GetHttpClientAsync(dpsHostName, cancellationToken);
        using HttpResponseMessage response = await httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        // parse into EnrollmentGroup using source-generated context for AOT safety
        EnrollmentGroup? enrollmentGroup = JsonSerializer.Deserialize(
            content,
            EnrollmentGroupJsonContext.Default.EnrollmentGroup);

        _logger.LogInformation("Enrollment group {content} retrieved successfully from DPS instance: {DpsHostName}", content, dpsHostName);

        return enrollmentGroup ?? throw new InvalidOperationException($"Enrollment group {enrollmentGroupId} not found in DPS instance {dpsHostName}.");
    }

    public async Task<IList<EnrollmentGroup>> ListAllEnrollmentGroupsAsync(
        string dpsHostName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listing all enrollment groups in DPS instance: {DpsHostName}", dpsHostName);
        var path = new Uri($"/enrollmentGroups/query?api-version={ApiVersion}", UriKind.Relative);
        var requestBody = new QueryRequest();
        using var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody, QueryequestJsonContext.Default.QueryRequest),
            Encoding.UTF8,
            "application/json");

        using HttpClient httpClient = await GetHttpClientAsync(dpsHostName, cancellationToken);

        // add any additional headers 
        httpClient.DefaultRequestHeaders.Add("x-ms-max-item-count", 200.ToString());
        using HttpResponseMessage response = await httpClient.PostAsync(path, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        // parse into array of enrollment groups using source-generated context for AOT safety
        List<EnrollmentGroup>? enrollmentGroups = JsonSerializer.Deserialize(
            content,
            EnrollmentGroupListJsonContext.Default.ListEnrollmentGroup);

        return enrollmentGroups ?? [];
    }
}
