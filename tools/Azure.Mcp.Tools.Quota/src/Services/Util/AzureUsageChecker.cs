// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Tenant;
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
    ContainerInstance,
    SQL,
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
    Task<List<UsageInfo>> GetUsageForLocationAsync(string location, CancellationToken cancellationToken);
}

// Abstract base class for checking Azure quotas
public abstract class AzureUsageChecker : IUsageChecker
{
    protected readonly string SubscriptionId;
    protected readonly ArmClient ResourceClient;
    protected readonly TokenCredential Credential;
    protected readonly ILogger Logger;
    protected readonly ITenantService TenantService;
    protected readonly IHttpClientFactory? HttpClientFactory;

    protected AzureUsageChecker(TokenCredential credential, string subscriptionId, ILogger logger, ITenantService tenantService, IHttpClientFactory? httpClientFactory = null)
    {
        SubscriptionId = subscriptionId;
        Credential = credential ?? throw new ArgumentNullException(nameof(credential));
        TenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        Logger = logger;
        HttpClientFactory = httpClientFactory;
        var clientOptions = new ArmClientOptions { Environment = tenantService.CloudConfiguration.ArmEnvironment };

        ResourceClient = new ArmClient(
            credential,
            subscriptionId,
            clientOptions);
    }

    protected string GetManagementEndpoint()
    {
        return TenantService.CloudConfiguration.ArmEnvironment.Endpoint.ToString().TrimEnd('/');
    }


    public abstract Task<List<UsageInfo>> GetUsageForLocationAsync(string location, CancellationToken cancellationToken);

    protected async Task<JsonDocument?> GetQuotaByUrlAsync(string requestUrl, CancellationToken cancellationToken = default)
    {
        if (HttpClientFactory is null)
        {
            throw new InvalidOperationException($"{nameof(HttpClientFactory)} is required to call {nameof(GetQuotaByUrlAsync)}.");
        }

        try
        {
            var token = await Credential.GetTokenAsync(
                new TokenRequestContext([TenantService.CloudConfiguration.ArmEnvironment.DefaultScope]),
                cancellationToken);

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpClient = HttpClientFactory.CreateClient(nameof(AzureUsageChecker));
            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP error! status: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonDocument.Parse(content);
        }
        catch (Exception error)
        {
            Logger.LogWarning("Error fetching quotas directly: {Error}", error.Message);
            return null;
        }
    }

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
        { "Microsoft.Sql", ResourceProvider.SQL },
        { "Microsoft.ContainerInstance", ResourceProvider.ContainerInstance }
    };

    public static IUsageChecker CreateUsageChecker(TokenCredential credential, string provider, string subscriptionId, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, ITenantService tenantService)
    {
        if (!ProviderMapping.TryGetValue(provider, out var resourceProvider))
        {
            throw new ArgumentException($"Unsupported resource provider: {provider}");
        }

        return resourceProvider switch
        {
            ResourceProvider.Compute => new ComputeUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<ComputeUsageChecker>(), tenantService),
            ResourceProvider.CognitiveServices => new CognitiveServicesUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<CognitiveServicesUsageChecker>(), tenantService),
            ResourceProvider.Storage => new StorageUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<StorageUsageChecker>(), tenantService),
            ResourceProvider.ContainerApp => new ContainerAppUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<ContainerAppUsageChecker>(), tenantService),
            ResourceProvider.Network => new NetworkUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<NetworkUsageChecker>(), tenantService),
            ResourceProvider.MachineLearning => new MachineLearningUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<MachineLearningUsageChecker>(), tenantService),
            ResourceProvider.PostgreSQL => new PostgreSQLUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<PostgreSQLUsageChecker>(), httpClientFactory, tenantService),
            ResourceProvider.HDInsight => new HDInsightUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<HDInsightUsageChecker>(), tenantService),
            ResourceProvider.Search => new SearchUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<SearchUsageChecker>(), tenantService),
            ResourceProvider.ContainerInstance => new ContainerInstanceUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<ContainerInstanceUsageChecker>(), tenantService),
            ResourceProvider.SQL => new SQLUsageChecker(credential, subscriptionId, loggerFactory.CreateLogger<SQLUsageChecker>(), httpClientFactory, tenantService),
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
        ITenantService tenantService,
        ILoggerFactory loggerFactory,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken)
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
                var usageChecker = UsageCheckerFactory.CreateUsageChecker(credential, provider, subscriptionId, loggerFactory, httpClientFactory, tenantService);
                var quotaInfo = await usageChecker.GetUsageForLocationAsync(location, cancellationToken);
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
