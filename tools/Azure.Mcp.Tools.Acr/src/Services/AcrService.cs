// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Containers.ContainerRegistry;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Acr.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Acr.Services;

public sealed class AcrService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<AcrService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IAcrService
{
    private readonly ILogger<AcrService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<AcrRegistryInfo>> ListRegistries(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            var registries = await ExecuteResourceQueryAsync(
                "Microsoft.ContainerRegistry/registries",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToAcrRegistryInfoModel,
                cancellationToken: cancellationToken);

            return registries;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving container registries: {ex.Message}", ex);
        }
    }

    private async Task<AcrRegistryInfo> GetRegistry(
        string subscription,
        string registry,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        ValidateRequiredParameters((nameof(registry), registry));

        try
        {
            var registrie = await ExecuteSingleResourceQueryAsync(
                        "Microsoft.ContainerRegistry/registries",
                        resourceGroup: resourceGroup,
                        subscription: subscription,
                        retryPolicy: retryPolicy,
                        converter: ConvertToAcrRegistryInfoModel,
                        additionalFilter: $"name =~ '{EscapeKqlString(registry)}'",
                        cancellationToken: cancellationToken);
            if (registrie == null)
            {
                throw new KeyNotFoundException($"Container registry '{registry}' not found for subscription '{subscription}'.");
            }
            return registrie;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving Container registry '{RegistryName}' for subscription '{Subscription}'",
                registry, subscription);
            throw;
        }
    }

    private async Task<List<string>> AddRepositoriesForRegistryAsync(AcrRegistryInfo reg, string? tenant, RetryPolicyOptions? retryPolicy, CancellationToken cancellationToken)
    {
        // Build data-plane client for this login server
        var credential = await GetCredential(tenant, cancellationToken);
        var options = ConfigureRetryPolicy(AddDefaultPolicies(new ContainerRegistryClientOptions()), retryPolicy);
        var acrEndpoint = new Uri($"https://{reg.LoginServer}");
        var client = new ContainerRegistryClient(acrEndpoint, credential, options);

        var repoNames = new List<string>();
        try
        {
            await foreach (var repo in client.GetRepositoryNamesAsync(cancellationToken))
            {
                if (!string.IsNullOrWhiteSpace(repo))
                {
                    repoNames.Add(repo);
                }
            }
        }
        catch (RequestFailedException)
        {
            _logger.LogWarning("Failed to list repositories for registry '{RegistryName}' at '{LoginServer}'", reg.Name, reg.LoginServer);
        }

        return repoNames;
    }

    public async Task<Dictionary<string, List<string>>> ListRegistryRepositories(
        string subscription,
        string? resourceGroup = null,
        string? registry = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(registry))
        {
            var registries = await ListRegistries(subscription, resourceGroup, tenant, retryPolicy, cancellationToken);
            foreach (var reg in registries)
            {
                if (!string.IsNullOrWhiteSpace(reg.Name) && !string.IsNullOrWhiteSpace(reg.LoginServer))
                {
                    result[reg.Name] = await AddRepositoriesForRegistryAsync(reg, tenant, retryPolicy, cancellationToken);
                }
            }
        }
        else
        {
            var reg = await GetRegistry(subscription, registry, resourceGroup, tenant, retryPolicy, cancellationToken);
            if (!string.IsNullOrWhiteSpace(reg.Name) && !string.IsNullOrWhiteSpace(reg.LoginServer))
            {
                result[reg.Name] = await AddRepositoriesForRegistryAsync(reg, tenant, retryPolicy, cancellationToken);
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
        var containerRegistryData = Models.ContainerRegistryData.FromJson(item);
        if (containerRegistryData == null)
            throw new InvalidOperationException("Failed to parse Container Registry data");

        return new AcrRegistryInfo
        (
            containerRegistryData.ResourceName ?? string.Empty,
            containerRegistryData.Location,
            containerRegistryData.Properties?.LoginServer,
            containerRegistryData.Sku?.Name,
            containerRegistryData.Sku?.Tier
        );
    }
}
