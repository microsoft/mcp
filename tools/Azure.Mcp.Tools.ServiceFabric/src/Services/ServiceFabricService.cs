// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ServiceFabric.Commands;
using Azure.Mcp.Tools.ServiceFabric.Models;

namespace Azure.Mcp.Tools.ServiceFabric.Services;

public sealed class ServiceFabricService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IHttpClientFactory httpClientFactory) : BaseAzureService(tenantService), IServiceFabricService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    private const string AzureManagementBaseUrl = "https://management.azure.com";
    private const string ApiVersion = "2024-04-01";

    public async Task<List<ManagedClusterNode>> ListManagedClusterNodes(
        string subscription,
        string resourceGroup,
        string clusterName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(clusterName), clusterName));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var subscriptionId = subscriptionResource.Id.SubscriptionId;

        var credential = await GetCredential(tenant, cancellationToken);
        var token = await credential.GetTokenAsync(
            new TokenRequestContext([$"{AzureManagementBaseUrl}/.default"]),
            cancellationToken);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        var requestUrl = $"{AzureManagementBaseUrl}/subscriptions/{subscriptionId}/resourceGroups/{Uri.EscapeDataString(resourceGroup)}/providers/Microsoft.ServiceFabric/managedClusters/{Uri.EscapeDataString(clusterName)}/nodes?api-version={ApiVersion}";

        var allNodes = new List<ManagedClusterNode>();

        while (!string.IsNullOrEmpty(requestUrl))
        {
            using var response = await client.GetAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var listResponse = JsonSerializer.Deserialize(content, ServiceFabricJsonContext.Default.ListNodesResponse)
                ?? throw new InvalidOperationException("Failed to deserialize the managed cluster nodes response.");

            if (listResponse.Value != null)
            {
                allNodes.AddRange(listResponse.Value);
            }

            requestUrl = listResponse.NextLink;
        }

        return allNodes;
    }

    public async Task<RestartNodeResponse> RestartManagedClusterNodes(
        string subscription,
        string resourceGroup,
        string clusterName,
        string nodeType,
        string[] nodes,
        string updateType = "Default",
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(clusterName), clusterName),
            (nameof(nodeType), nodeType));

        ArgumentNullException.ThrowIfNull(nodes);
        if (nodes.Length == 0)
        {
            throw new ArgumentException("At least one node name must be specified.", nameof(nodes));
        }

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var subscriptionId = subscriptionResource.Id.SubscriptionId;

        var credential = await GetCredential(tenant, cancellationToken);
        var token = await credential.GetTokenAsync(
            new TokenRequestContext([$"{AzureManagementBaseUrl}/.default"]),
            cancellationToken);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        var requestUrl = $"{AzureManagementBaseUrl}/subscriptions/{subscriptionId}/resourceGroups/{Uri.EscapeDataString(resourceGroup)}/providers/Microsoft.ServiceFabric/managedClusters/{Uri.EscapeDataString(clusterName)}/nodeTypes/{Uri.EscapeDataString(nodeType)}/restart?api-version={ApiVersion}";

        var requestBody = new RestartNodeRequest
        {
            Nodes = [.. nodes],
            UpdateType = updateType
        };

        using var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody, ServiceFabricJsonContext.Default.RestartNodeRequest),
            Encoding.UTF8,
            "application/json");

        using var response = await client.PostAsync(requestUrl, jsonContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = new RestartNodeResponse
        {
            StatusCode = (int)response.StatusCode
        };

        if (response.Headers.TryGetValues("Azure-AsyncOperation", out var asyncOp))
        {
            result.AsyncOperationUrl = asyncOp.FirstOrDefault();
        }

        if (response.Headers.Location != null)
        {
            result.Location = response.Headers.Location.ToString();
        }

        return result;
    }
}
