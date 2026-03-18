// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.CosmosDB;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Cosmos.Services;

public sealed class CopyJobService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<CopyJobService> logger)
    : BaseAzureService(tenantService), ICopyJobService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<CopyJobService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private const string ApiVersion = "2025-05-01-preview";

    /// <summary>
    /// Finds the Cosmos DB account by name and returns its ARM resource ID.
    /// </summary>
    private async Task<string> GetAccountResourceIdAsync(
        string subscription,
        string accountName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(accountName), accountName));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);

        await foreach (var account in subscriptionResource.GetCosmosDBAccountsAsync(cancellationToken))
        {
            if (string.Equals(account.Data.Name, accountName, StringComparison.OrdinalIgnoreCase))
            {
                return account.Id.ToString();
            }
        }

        throw new Exception($"Cosmos DB account '{accountName}' not found in subscription '{subscription}'.");
    }

    /// <summary>
    /// Gets an HttpClient with ARM auth token set.
    /// </summary>
    private async Task<HttpClient> GetAuthenticatedHttpClientAsync(
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GetArmAccessTokenAsync(tenant, cancellationToken);

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    /// <summary>
    /// Builds the base URL for copyJobs operations.
    /// </summary>
    private string BuildCopyJobsUrl(string accountResourceId, string? jobName = null)
    {
        var armEndpoint = TenantService.CloudConfiguration.ArmEnvironment.Endpoint.ToString().TrimEnd('/');
        var url = $"{armEndpoint}{accountResourceId}/copyJobs";
        if (!string.IsNullOrEmpty(jobName))
        {
            url += $"/{jobName}";
        }
        url += $"?api-version={ApiVersion}";
        return url;
    }

    public async Task<JsonElement> CreateJobAsync(
        string subscription,
        string accountName,
        string jobName,
        string jobPropertiesJson,
        string? mode = null,
        int? workerCount = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(accountName), accountName),
            (nameof(jobName), jobName),
            (nameof(jobPropertiesJson), jobPropertiesJson));

        // Parse and validate the job properties JSON
        JsonElement jobProps;
        try
        {
            jobProps = JsonDocument.Parse(jobPropertiesJson).RootElement;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON in jobProperties: {ex.Message}", ex);
        }

        var accountResourceId = await GetAccountResourceIdAsync(subscription, accountName, tenant, retryPolicy, cancellationToken);

        // Build the request body
        var properties = new Dictionary<string, object>
        {
            ["jobProperties"] = jobProps
        };
        if (!string.IsNullOrEmpty(mode))
        {
            properties["mode"] = mode;
        }
        if (workerCount.HasValue)
        {
            properties["workerCount"] = workerCount.Value;
        }

        var body = JsonSerializer.Serialize(new { properties });

        using var client = await GetAuthenticatedHttpClientAsync(tenant, cancellationToken);
        var url = BuildCopyJobsUrl(accountResourceId, jobName);

        _logger.LogInformation("Creating copy job '{JobName}' on account '{Account}'", jobName, accountName);

        var response = await client.PutAsync(url, new StringContent(body, Encoding.UTF8, "application/json"), cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to create copy job (HTTP {(int)response.StatusCode}): {responseBody}");
        }

        return JsonDocument.Parse(responseBody).RootElement;
    }

    public async Task<JsonElement> GetJobAsync(
        string subscription,
        string accountName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(accountName), accountName),
            (nameof(jobName), jobName));

        var accountResourceId = await GetAccountResourceIdAsync(subscription, accountName, tenant, retryPolicy, cancellationToken);

        using var client = await GetAuthenticatedHttpClientAsync(tenant, cancellationToken);
        var url = BuildCopyJobsUrl(accountResourceId, jobName);

        var response = await client.GetAsync(url, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to get copy job (HTTP {(int)response.StatusCode}): {responseBody}");
        }

        return JsonDocument.Parse(responseBody).RootElement;
    }

    public async Task<List<JsonElement>> ListJobsAsync(
        string subscription,
        string accountName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(accountName), accountName));

        var accountResourceId = await GetAccountResourceIdAsync(subscription, accountName, tenant, retryPolicy, cancellationToken);

        using var client = await GetAuthenticatedHttpClientAsync(tenant, cancellationToken);
        var jobs = new List<JsonElement>();
        var url = BuildCopyJobsUrl(accountResourceId);

        while (!string.IsNullOrEmpty(url))
        {
            var response = await client.GetAsync(url, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to list copy jobs (HTTP {(int)response.StatusCode}): {responseBody}");
            }

            var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("value", out var valueArray))
            {
                foreach (var item in valueArray.EnumerateArray())
                {
                    jobs.Add(item.Clone());
                }
            }

            // Handle pagination
            url = doc.RootElement.TryGetProperty("nextLink", out var nextLink)
                ? nextLink.GetString()
                : null;
        }

        return jobs;
    }

    private async Task<JsonElement> PostJobActionAsync(
        string subscription,
        string accountName,
        string jobName,
        string action,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(accountName), accountName),
            (nameof(jobName), jobName));

        var accountResourceId = await GetAccountResourceIdAsync(subscription, accountName, tenant, retryPolicy, cancellationToken);

        using var client = await GetAuthenticatedHttpClientAsync(tenant, cancellationToken);
        var armEndpoint = TenantService.CloudConfiguration.ArmEnvironment.Endpoint.ToString().TrimEnd('/');
        var url = $"{armEndpoint}{accountResourceId}/copyJobs/{jobName}/{action}?api-version={ApiVersion}";

        _logger.LogInformation("{Action} copy job '{JobName}' on account '{Account}'", action, jobName, accountName);

        var response = await client.PostAsync(url, new StringContent("{}", Encoding.UTF8, "application/json"), cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to {action} copy job (HTTP {(int)response.StatusCode}): {responseBody}");
        }

        // Some actions return empty body on success (e.g., 202 Accepted)
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return JsonDocument.Parse($"{{\"status\":\"{action} accepted\",\"jobName\":\"{jobName}\"}}").RootElement;
        }

        return JsonDocument.Parse(responseBody).RootElement;
    }

    public Task<JsonElement> CancelJobAsync(string subscription, string accountName, string jobName,
        string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
        => PostJobActionAsync(subscription, accountName, jobName, "cancel", tenant, retryPolicy, cancellationToken);

    public Task<JsonElement> PauseJobAsync(string subscription, string accountName, string jobName,
        string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
        => PostJobActionAsync(subscription, accountName, jobName, "pause", tenant, retryPolicy, cancellationToken);

    public Task<JsonElement> ResumeJobAsync(string subscription, string accountName, string jobName,
        string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
        => PostJobActionAsync(subscription, accountName, jobName, "resume", tenant, retryPolicy, cancellationToken);

    public Task<JsonElement> CompleteJobAsync(string subscription, string accountName, string jobName,
        string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
        => PostJobActionAsync(subscription, accountName, jobName, "complete", tenant, retryPolicy, cancellationToken);
}
