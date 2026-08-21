// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Containers.ContainerRegistry;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Acr.Models;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Acr.Services;

public sealed class AcrService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IAcrService
{
    public async Task<ResourceQueryResults<AcrRegistryInfo>> ListRegistries(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var registries = await ExecuteResourceQueryAsync(
            "Microsoft.ContainerRegistry/registries",
            resourceGroup,
            subscription,
            retryPolicy: null,
            ConvertToAcrRegistryInfoModel,
            tenant: tenant,
            cancellationToken: cancellationToken);

        return registries;
    }

    private async Task<AcrRegistryInfo> GetRegistry(
        string subscription,
        string registry,
        string? resourceGroup = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(registry), registry));

        var registries = await ExecuteSingleResourceQueryAsync(
            "Microsoft.ContainerRegistry/registries",
            resourceGroup: resourceGroup,
            subscription: subscription,
            retryPolicy: null,
            converter: ConvertToAcrRegistryInfoModel,
            additionalFilter: $"name =~ '{EscapeKqlString(registry)}'",
            tenant: tenant,
            cancellationToken: cancellationToken);

        if (registries == null)
        {
            throw new KeyNotFoundException($"Container registry '{registry}' not found for subscription '{subscription}'.");
        }
        return registries;
    }

    private async Task<List<string>> AddRepositoriesForRegistryAsync(AcrRegistryInfo reg, string? tenant, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(reg.LoginServer))
        {
            var acrEndpointString = $"https://{reg.LoginServer}";
            EndpointValidator.ValidateAzureServiceEndpoint(acrEndpointString, "acr", AzureService.CloudConfiguration.ArmEnvironment);
        }

        // Build data-plane client for this login server
        var credential = await GetCredential(tenant, cancellationToken);
        var options = AddDefaultPolicies(new ContainerRegistryClientOptions());
        options.Transport = new HttpClientTransport(AzureService.GetClient());
        options.Audience = GetAcrAudience();
        var acrEndpoint = new Uri($"https://{reg.LoginServer}");
        var client = new ContainerRegistryClient(acrEndpoint, credential, options);

        var repoNames = new List<string>();
        await foreach (var repo in client.GetRepositoryNamesAsync(cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(repo))
            {
                repoNames.Add(repo);
            }
        }

        return repoNames;
    }

    public async Task<Dictionary<string, List<string>>> ListRegistryRepositories(
        string subscription,
        string? resourceGroup = null,
        string? registry = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(registry))
        {
            var registries = await ListRegistries(subscription, resourceGroup, tenant, cancellationToken);
            foreach (var reg in registries.Results)
            {
                if (!string.IsNullOrWhiteSpace(reg.Name) && !string.IsNullOrWhiteSpace(reg.LoginServer))
                {
                    result[reg.Name] = await AddRepositoriesForRegistryAsync(reg, tenant, cancellationToken);
                }
            }
        }
        else
        {
            var reg = await GetRegistry(subscription, registry, resourceGroup, tenant, cancellationToken);
            if (!string.IsNullOrWhiteSpace(reg.Name) && !string.IsNullOrWhiteSpace(reg.LoginServer))
            {
                result[reg.Name] = await AddRepositoriesForRegistryAsync(reg, tenant, cancellationToken);
            }
        }

        return result;
    }

    /// <summary>
    /// Converts a JsonElement from Azure Resource Graph query to a Container Registry model.
    /// </summary>
    /// <param name="item">The JsonElement containing Container Registry data</param>
    /// <returns>The Container Registry model</returns>
    private static AcrRegistryInfo ConvertToAcrRegistryInfoModel(JsonElement item)
    {
        var containerRegistryData = Models.ContainerRegistryData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to parse Container Registry data");

        return new AcrRegistryInfo(
            containerRegistryData.ResourceName ?? string.Empty,
            containerRegistryData.Location,
            containerRegistryData.Properties?.LoginServer,
            containerRegistryData.Sku?.Name,
            containerRegistryData.Sku?.Tier);
    }

    private ContainerRegistryAudience GetAcrAudience()
    {
        return AzureService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => ContainerRegistryAudience.AzureResourceManagerPublicCloud,
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => ContainerRegistryAudience.AzureResourceManagerChina,
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => ContainerRegistryAudience.AzureResourceManagerGovernment,
            _ => ContainerRegistryAudience.AzureResourceManagerPublicCloud
        };
    }
}
