// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Quota.Services.Util.Usage;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Helpers;

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

/// <summary>
/// Base class for Azure resource-provider quota checkers.
/// </summary>
public abstract class AzureUsageChecker : IUsageChecker
{
    protected readonly string SubscriptionId;
    protected readonly ArmClient ResourceClient;
    protected readonly TokenCredential Credential;
    protected readonly ILogger Logger;
    protected readonly IAzureService AzureService;

    /// <summary>
    /// Initializes a quota checker with a configured ARM client and raw-request dependencies.
    /// </summary>
    /// <param name="resourceClient">
    /// The ARM client created by the owning <see cref="BaseAzureService"/> so it uses the configured cloud,
    /// credential provider, HTTP transport, user agent, and retry policies.
    /// </param>
    /// <param name="credential">The credential used by raw ARM quota requests.</param>
    /// <param name="subscriptionId">The Azure subscription ID whose quota will be checked.</param>
    /// <param name="logger">The logger used by the concrete quota checker.</param>
    /// <param name="azureService">The Azure service used by raw ARM quota requests.</param>
    protected AzureUsageChecker(
        ArmClient resourceClient,
        TokenCredential credential,
        string subscriptionId,
        ILogger logger,
        IAzureService azureService)
    {
        SubscriptionId = subscriptionId;
        ResourceClient = resourceClient ?? throw new ArgumentNullException(nameof(resourceClient));
        Credential = credential ?? throw new ArgumentNullException(nameof(credential));
        AzureService = azureService ?? throw new ArgumentNullException(nameof(azureService));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public abstract Task<List<UsageInfo>> GetUsageForLocationAsync(string location, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a quota request to a path under the configured Azure Resource Manager endpoint.
    /// </summary>
    /// <param name="relativePath">
    /// The rooted or relative ARM request path. Absolute and network-path values are rejected unless they resolve
    /// to the configured cloud's exact ARM host.
    /// </param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The parsed response body, or <see langword="null"/> when the ARM request fails.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the ARM endpoint allow-list is unavailable.
    /// </exception>
    /// <exception cref="System.Security.SecurityException">
    /// Thrown when the completed request URI is not an HTTPS Azure Resource Manager endpoint for the configured cloud.
    /// </exception>
    protected async Task<JsonDocument?> GetQuotaByUrlAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        ArmEnvironment armEnvironment = AzureService.CloudConfiguration.ArmEnvironment;
        Uri requestUri = new(armEnvironment.Endpoint, relativePath);

        // Uri resolution accepts absolute and network-path inputs that can replace the configured ARM authority.
        // Validate the completed URI before access-token acquisition so derived usage checkers cannot redirect tokens.
        EndpointValidator.ValidateAzureServiceEndpoint(
            endpoint: requestUri.AbsoluteUri,
            serviceType: "arm",
            armEnvironment: armEnvironment,
            executingToolNamespaceName: "quota");

        try
        {
            AccessToken token = await Credential.GetTokenAsync(
                new TokenRequestContext([AzureService.CloudConfiguration.ArmEnvironment.DefaultScope]),
                cancellationToken);

            using HttpRequestMessage request = new(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new("Bearer", token.Token);
            request.Headers.Accept.Add(new("application/json"));

            HttpClient httpClient = AzureService.GetClient(nameof(AzureUsageChecker));
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP error! status: {response.StatusCode}");
            }

            string content = await response.Content.ReadAsStringAsync(cancellationToken);
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

    public static IUsageChecker CreateUsageChecker(
        ArmClient resourceClient,
        TokenCredential credential,
        string provider,
        string subscriptionId,
        ILoggerFactory loggerFactory,
        IAzureService azureService)
    {
        if (!ProviderMapping.TryGetValue(provider, out ResourceProvider resourceProvider))
        {
            throw new ArgumentException($"Unsupported resource provider: {provider}");
        }

        return resourceProvider switch
        {
            ResourceProvider.Compute => new ComputeUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<ComputeUsageChecker>(), azureService),
            ResourceProvider.CognitiveServices => new CognitiveServicesUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<CognitiveServicesUsageChecker>(), azureService),
            ResourceProvider.Storage => new StorageUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<StorageUsageChecker>(), azureService),
            ResourceProvider.ContainerApp => new ContainerAppUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<ContainerAppUsageChecker>(), azureService),
            ResourceProvider.Network => new NetworkUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<NetworkUsageChecker>(), azureService),
            ResourceProvider.MachineLearning => new MachineLearningUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<MachineLearningUsageChecker>(), azureService),
            ResourceProvider.PostgreSQL => new PostgreSQLUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<PostgreSQLUsageChecker>(), azureService),
            ResourceProvider.HDInsight => new HDInsightUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<HDInsightUsageChecker>(), azureService),
            ResourceProvider.Search => new SearchUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<SearchUsageChecker>(), azureService),
            ResourceProvider.ContainerInstance => new ContainerInstanceUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<ContainerInstanceUsageChecker>(), azureService),
            ResourceProvider.SQL => new SQLUsageChecker(resourceClient, credential, subscriptionId, loggerFactory.CreateLogger<SQLUsageChecker>(), azureService),
            _ => throw new ArgumentException($"No implementation for provider: {provider}")
        };
    }
}
