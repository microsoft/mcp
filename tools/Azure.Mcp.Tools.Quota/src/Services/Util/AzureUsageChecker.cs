// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Quota.Services.Util.Usage;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util;

// For simplicity, we currently apply a single rule for all Azure resource providers:
//   - Any resource provider not listed in the enum is treated as having no quota limitations.
// Ideally, we'd differentiate between the following cases:
//   1. The resource provider has no quota limitations.
//   2. The resource provider has quota limitations but does not expose a quota API.
//   3. The resource provider exposes a quota API, but it's not yet supported by the checker.

public enum ResourceProvider
{
    CognitiveServices,
    Compute,
    Storage,
    ContainerApp,
    Network,
    MachineLearning,
    PostgreSQL,
    HDInsight,
    Search,
    ContainerInstance
}

public record UsageInfo(
    string Name,
    int Limit,
    int Used,
    string? Unit = null,
    string? Description = null
);

public interface IUsageChecker
{
    Task<List<UsageInfo>> GetUsageForLocationAsync(string location);
}

// Abstract base class for checking Azure quotas
public abstract class AzureUsageChecker : IUsageChecker
{
    protected readonly string SubscriptionId;
    protected readonly ArmClient ResourceClient;
    protected readonly TokenCredential Credential;
    protected readonly ILogger Logger;
    protected const string managementEndpoint = "https://management.azure.com";

    protected AzureUsageChecker(TokenCredential credential, string subscriptionId, ILogger logger)
    {
        SubscriptionId = subscriptionId;
        Credential = credential ?? throw new ArgumentNullException(nameof(credential));
        ResourceClient = new ArmClient(credential, subscriptionId);
        Logger = logger;
    }

    public abstract Task<List<UsageInfo>> GetUsageForLocationAsync(string location);

}

// Factory function to create usage checkers
public static class UsageCheckerFactory
{
    private static readonly Dictionary<string, ResourceProvider> ProviderMapping = new()
    {
        { "Microsoft.CognitiveServices", ResourceProvider.CognitiveServices },
        { "Microsoft.Compute", ResourceProvider.Compute },
        { "Microsoft.Storage", ResourceProvider.Storage },
        { "Microsoft.App", ResourceProvider.ContainerApp },
        { "Microsoft.Network", ResourceProvider.Network },
        { "Microsoft.MachineLearningServices", ResourceProvider.MachineLearning },
        { "Microsoft.DBforPostgreSQL", ResourceProvider.PostgreSQL },
        { "Microsoft.HDInsight", ResourceProvider.HDInsight },
        { "Microsoft.Search", ResourceProvider.Search },
        { "Microsoft.ContainerInstance", ResourceProvider.ContainerInstance }
    };

    public static IUsageChecker CreateUsageChecker(TokenCredential credential, string provider, string subscriptionId, ILoggerFactory loggerFactory, IHttpClientService httpClientService)
    {
        if (!ProviderMapping.TryGetValue(provider, out var resourceProvider))
        {
            throw new ArgumentException($"Unsupported resource provider: {provider}");
        }

        return resourceProvider switch
        {
            ResourceProvider.Compute => new ComputeUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<ComputeUsageChecker>()),
            ResourceProvider.CognitiveServices => new CognitiveServicesUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<CognitiveServicesUsageChecker>()),
            ResourceProvider.Storage => new StorageUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<StorageUsageChecker>()),
            ResourceProvider.ContainerApp => new ContainerAppUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<ContainerAppUsageChecker>()),
            ResourceProvider.Network => new NetworkUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<NetworkUsageChecker>()),
            ResourceProvider.MachineLearning => new MachineLearningUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<MachineLearningUsageChecker>()),
            ResourceProvider.PostgreSQL => new PostgreSQLUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<PostgreSQLUsageChecker>(), httpClientService),
            ResourceProvider.HDInsight => new HDInsightUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<HDInsightUsageChecker>()),
            ResourceProvider.Search => new SearchUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<SearchUsageChecker>()),
            ResourceProvider.ContainerInstance => new ContainerInstanceUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<ContainerInstanceUsageChecker>()),
            _ => throw new ArgumentException($"No implementation for provider: {provider}")
        };
    }
}

// Service to get Azure quota for a list of resource types
public static class AzureQuotaService
{
    public static async Task<Dictionary<string, List<UsageInfo>>> GetAzureQuotaAsync(
        TokenCredential credential,
        List<string> resourceTypes,
        string subscriptionId,
        string location,
        ILoggerFactory loggerFactory,
        IHttpClientService httpClientService)
    {
        // Group resource types by provider to avoid duplicate processing
        var providerToResourceTypes = resourceTypes
            .GroupBy(rt => rt.Split('/')[0])
            .ToDictionary(g => g.Key, g => g.ToList());

        var logger = loggerFactory.CreateLogger(typeof(AzureQuotaService));

        // Use Select to create tasks and await them all
        var quotaTasks = providerToResourceTypes.Select(async kvp =>
        {
            var (provider, resourceTypesForProvider) = (kvp.Key, kvp.Value);
            try
            {
                var usageChecker = UsageCheckerFactory.CreateUsageChecker(credential, provider, subscriptionId, loggerFactory, httpClientService);
                var quotaInfo = await usageChecker.GetUsageForLocationAsync(location);
                logger.LogDebug("Retrieved quota info for provider {Provider}: {ItemCount} items", provider, quotaInfo.Count);

                return resourceTypesForProvider.Select(rt => new KeyValuePair<string, List<UsageInfo>>(rt, quotaInfo));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("Unsupported resource provider", StringComparison.OrdinalIgnoreCase))
            {
                return resourceTypesForProvider.Select(rt => new KeyValuePair<string, List<UsageInfo>>(rt, [
                    new(rt, 0, 0, Description: "No Limit")
                ]));
            }
            catch (Exception error)
            {
                logger.LogWarning("Error fetching quota for provider {Provider}: {Error}", provider, error.Message);
                return resourceTypesForProvider.Select(rt => new KeyValuePair<string, List<UsageInfo>>(rt,
                [
                    new(rt, 0, 0, Description: error.Message)
                ]));
            }
        });

        var results = await Task.WhenAll(quotaTasks);

        // Flatten the results into a single dictionary
        return results
            .SelectMany(i => i)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
