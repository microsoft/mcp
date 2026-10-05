// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ServiceFabric.Commands;
using Azure.Mcp.Tools.ServiceFabric.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.ServiceFabric.Services;

public sealed class ServiceFabricService(IAzureService azureService)
    : BaseAzureService(azureService), IServiceFabricService
{
    private const string ApiVersion = "2024-04-01";

    private string GetManagementBaseUrl() =>
        AzureService.CloudConfiguration.ArmEnvironment.Endpoint.ToString().TrimEnd('/');

    public async Task<List<ManagedClusterNode>> ListManagedClusterNodes(
        string subscription,
        string resourceGroup,
        string clusterName,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(clusterName), clusterName));

        SubscriptionResource subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
        string subscriptionId = subscriptionResource.Id.SubscriptionId
            ?? throw new InvalidOperationException("The resolved subscription does not have a subscription ID.");
        Uri? requestUri = CreateAndValidateRequestUri(
            $"{GetManagementBaseUrl()}/subscriptions/{Uri.EscapeDataString(subscriptionId)}/resourceGroups/{Uri.EscapeDataString(resourceGroup)}/providers/Microsoft.ServiceFabric/managedClusters/{Uri.EscapeDataString(clusterName)}/nodes?api-version={ApiVersion}");

        AccessToken token = await GetArmAccessTokenAsync(tenant, cancellationToken);

        var client = AzureService.GetClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);

        var allNodes = new List<ManagedClusterNode>();

        while (requestUri != null)
        {
            using HttpResponseMessage response = await client.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var listResponse = JsonSerializer.Deserialize(content, ServiceFabricJsonContext.Default.ListNodesResponse)
                ?? throw new InvalidOperationException("Failed to deserialize the managed cluster nodes response.");

            if (listResponse.Value != null)
            {
                allNodes.AddRange(listResponse.Value);
            }

            // A continuation is a new destination, even when it came from ARM. Authorize it before sending
            // another request with the bearer token; never resolve an untrusted link against a trusted base.
            requestUri = string.IsNullOrEmpty(listResponse.NextLink)
                ? null
                : CreateAndValidateRequestUri(listResponse.NextLink);
        }

        return allNodes;
    }

    public async Task<ManagedClusterNode> GetManagedClusterNode(
        string subscription,
        string resourceGroup,
        string clusterName,
        string nodeName,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(clusterName), clusterName),
            (nameof(nodeName), nodeName));

        SubscriptionResource subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
        string subscriptionId = subscriptionResource.Id.SubscriptionId
            ?? throw new InvalidOperationException("The resolved subscription does not have a subscription ID.");
        Uri requestUri = CreateAndValidateRequestUri(
            $"{GetManagementBaseUrl()}/subscriptions/{Uri.EscapeDataString(subscriptionId)}/resourceGroups/{Uri.EscapeDataString(resourceGroup)}/providers/Microsoft.ServiceFabric/managedClusters/{Uri.EscapeDataString(clusterName)}/nodes/{Uri.EscapeDataString(nodeName)}?api-version={ApiVersion}");

        AccessToken token = await GetArmAccessTokenAsync(tenant, cancellationToken);

        var client = AzureService.GetClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);

        using HttpResponseMessage response = await client.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content, ServiceFabricJsonContext.Default.ManagedClusterNode)
            ?? throw new InvalidOperationException("Failed to deserialize the managed cluster node response.");
    }

    public async Task<RestartNodeResponse> RestartManagedClusterNodes(
        string subscription,
        string resourceGroup,
        string clusterName,
        string nodeType,
        string[] nodes,
        UpdateType updateType = UpdateType.Default,
        string? tenant = null,
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

        SubscriptionResource subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
        string subscriptionId = subscriptionResource.Id.SubscriptionId
            ?? throw new InvalidOperationException("The resolved subscription does not have a subscription ID.");
        Uri requestUri = CreateAndValidateRequestUri(
            $"{GetManagementBaseUrl()}/subscriptions/{Uri.EscapeDataString(subscriptionId)}/resourceGroups/{Uri.EscapeDataString(resourceGroup)}/providers/Microsoft.ServiceFabric/managedClusters/{Uri.EscapeDataString(clusterName)}/nodeTypes/{Uri.EscapeDataString(nodeType)}/restart?api-version={ApiVersion}");

        AccessToken token = await GetArmAccessTokenAsync(tenant, cancellationToken);

        var client = AzureService.GetClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);

        var requestBody = new RestartNodeRequest
        {
            Nodes = [.. nodes],
            UpdateType = updateType.ToString()
        };

        using var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody, ServiceFabricJsonContext.Default.RestartNodeRequest),
            Encoding.UTF8,
            "application/json");

        using HttpResponseMessage response = await client.PostAsync(requestUri, jsonContent, cancellationToken);
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

    /// <summary>
    /// Parses and authorizes a completed request or pagination URL for the configured Azure Resource Manager cloud.
    /// </summary>
    /// <returns>The same parsed URI to use for the HTTP request.</returns>
    /// <exception cref="ArgumentException">Thrown when the ARM endpoint allow-list is unavailable.</exception>
    /// <exception cref="SecurityException">
    /// Thrown when the URL is not absolute HTTPS or its host is not the configured cloud's ARM endpoint.
    /// </exception>
    private Uri CreateAndValidateRequestUri(string requestUrl)
    {
        if (!Uri.TryCreate(requestUrl, UriKind.Absolute, out Uri? requestUri))
        {
            throw new SecurityException("Service Fabric request URL must be an absolute Azure Resource Manager URL.");
        }

        // These are control-plane REST requests, not Service Fabric cluster data-plane endpoints.
        // Initial paths escape caller-supplied segments; the final URI and every nextLink must independently
        // match the shared exact-host ARM policy before the HTTP client can send them.
        EndpointValidator.ValidateAzureServiceEndpoint(
            endpoint: requestUri.AbsoluteUri,
            serviceType: "arm",
            armEnvironment: AzureService.CloudConfiguration.ArmEnvironment,
            executingToolNamespaceName: "servicefabric");

        return requestUri;
    }
}
