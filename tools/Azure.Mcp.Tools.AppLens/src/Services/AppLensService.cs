// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AppLens.Models;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AppLens.Services;

/// <summary>
/// Service implementation for AppLens diagnostic operations.
/// Uses Azure Resource Graph to discover resources by name and validates
/// that the resource type is supported by AppLens before creating a session.
/// </summary>
public class AppLensService(
    IHttpClientFactory httpClientFactory,
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<AppLensService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IAppLensService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<AppLensService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly AppLensOptions _options = new();

    /// <inheritdoc />
    public async Task<DiagnosticResult> DiagnoseResourceAsync(
        string question,
        string resource,
        string? subscription = null,
        string? resourceGroup = null,
        string? resourceType = null,
        string? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Find the resource using ARG
        var findResult = await FindResourceAsync(resource, subscription, resourceGroup, resourceType, tenantId, cancellationToken);

        if (findResult is DidNotFindResourceResult notFound)
        {
            throw new InvalidOperationException(notFound.Message);
        }

        var foundResource = (FoundResourceResult)findResult;

        // Step 2: Get AppLens session
        var session = await GetAppLensSessionAsync(foundResource.ResourceId, tenantId, cancellationToken);

        if (session is FailedAppLensSessionResult failed)
        {
            throw new InvalidOperationException(failed.Message);
        }

        var successfulSession = (SuccessfulAppLensSessionResult)session;

        // Step 3: Ask AppLens the diagnostic question
        var insights = await CollectInsightsAsync(successfulSession.Session, question, cancellationToken);

        return new DiagnosticResult(
            insights.Insights,
            insights.Solutions,
            foundResource.ResourceId,
            foundResource.ResourceTypeAndKind);
    }

    /// <summary>
    /// Discovers a resource using Azure Resource Graph, progressively filtering by
    /// subscription, resource group, and resource type when provided. Only returns
    /// resources whose type is supported by AppLens.
    /// </summary>
    internal async Task<FindResourceIdResult> FindResourceAsync(
        string resourceName,
        string? subscription,
        string? resourceGroup,
        string? resourceType,
        string? tenantId,
        CancellationToken cancellationToken)
    {
        // Query ARG for resources matching the name
        var queryResults = await ExecuteArgQueryAsync(resourceName, subscription, tenantId, cancellationToken);

        if (queryResults.Length == 0)
        {
            return new DidNotFindResourceResult($"No resources found with name '{resourceName}'.");
        }

        var filteredResults = queryResults;

        // There is no need to filter by the given subscription (if any) because that would
        // have been taken care of by the ARG query.

        // Progressive filtering by resource group
        if (!string.IsNullOrEmpty(resourceGroup))
        {
            var rgFiltered = filteredResults
                .Where(r => r.ResourceGroup.Equals(resourceGroup, StringComparison.OrdinalIgnoreCase))
                .ToImmutableArray();

            if (rgFiltered.Length == 0)
            {
                var foundGroups = string.Join("\n", filteredResults
                    .Select(r => r.ResourceGroup)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(rg => $"- {rg}"));
                return new DidNotFindResourceResult(
                    $"Found resources with name '{resourceName}' but not in resource group '{resourceGroup}'.\n" +
                    $"Resources were found in the following resource groups:\n{foundGroups}");
            }

            filteredResults = rgFiltered;
        }

        // Progressive filtering by resource type
        if (!string.IsNullOrEmpty(resourceType))
        {
            var typeFiltered = filteredResults
                .Where(r => r.ResourceType.Equals(resourceType, StringComparison.OrdinalIgnoreCase))
                .ToImmutableArray();

            if (typeFiltered.Length == 0)
            {
                var foundTypes = string.Join("\n", filteredResults
                    .Select(r => r.ResourceType)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(t => $"- {t}"));
                return new DidNotFindResourceResult(
                    $"Found resources with name '{resourceName}' but not of type '{resourceType}'.\n" +
                    $"Found resources of the following types:\n{foundTypes}");
            }

            filteredResults = typeFiltered;
        }

        // Filter to supported resource types
        var supportedResults = filteredResults
            .Where(r => IsResourceTypeSupported(r.ResourceType, r.ResourceKind))
            .ToImmutableArray();

        if (supportedResults.Length == 0)
        {
            var supportedTypes = string.Join("\n", SupportedResourceTypes().Select(t => $"- {t}"));
            return new DidNotFindResourceResult(
                $"No supported resources found with name '{resourceName}'.\n" +
                $"Supported resource types:\n{supportedTypes}");
        }

        // Disambiguation when multiple resources match
        if (supportedResults.Length > 1)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Found multiple resources matching '{resourceName}':");

            foreach (var result in supportedResults)
            {
                sb.AppendLine($"- Subscription: {result.SubscriptionId}, resource group: {result.ResourceGroup}, type: {result.ResourceType}, name: {result.ResourceName}");
            }

            sb.AppendLine();
            sb.AppendLine("Which one do you want to diagnose? Please specify --subscription, --resource-group, or --resource-type to narrow down the results.");

            return new DidNotFindResourceResult(sb.ToString());
        }

        // Single result found
        var resource = supportedResults[0];
        var resourceTypeAndKind = resource.ResourceType;
        if (!string.IsNullOrEmpty(resource.ResourceKind))
        {
            resourceTypeAndKind += "/" + resource.ResourceKind;
        }

        return new FoundResourceResult(resource.Id, resourceTypeAndKind, null);
    }

    /// <summary>
    /// Executes an Azure Resource Graph query to find resources by name.
    /// </summary>
    private async Task<ImmutableArray<AppLensArgQueryResult>> ExecuteArgQueryAsync(
        string resourceName,
        string? subscription,
        string? tenantId,
        CancellationToken cancellationToken)
    {
        var escapedName = EscapeKqlString(resourceName);
        var kqlQuery = $"resources | where name =~ '{escapedName}' | project id, subscriptionId, resourceGroup, type, kind, name | limit 50";

        var queryContent = new ResourceQueryContent(kqlQuery);

        // If subscription is provided, scope to that subscription and the related tenant.
        // Otherwise query all accessible subscriptions.
        Guid? targetTenantId = null;
        if (!string.IsNullOrEmpty(subscription))
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenantId, cancellationToken: cancellationToken);
            queryContent.Subscriptions.Add(subscriptionResource.Data.SubscriptionId);
            targetTenantId = subscriptionResource.Data.TenantId;
        }
        else if (!string.IsNullOrEmpty(tenantId))
        {
            targetTenantId = Guid.Parse(tenantId);
        }

        var tenants = await TenantService.GetTenants(cancellationToken);
        var tenantResource = targetTenantId.HasValue
            ? tenants.FirstOrDefault(t => t.Data.TenantId == targetTenantId)
            : tenants.FirstOrDefault();
        if (tenantResource is null)
        {
            throw new InvalidOperationException("No accessible tenants found.");
        }

        var response = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
        var result = response.Value;

        if (result == null || result.Count == 0)
        {
            return [];
        }

        using var jsonDocument = JsonDocument.Parse(result.Data);
        var dataArray = jsonDocument.RootElement;

        if (dataArray.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var results = new List<AppLensArgQueryResult>();
        foreach (var item in dataArray.EnumerateArray())
        {
            results.Add(new AppLensArgQueryResult(
                Id: item.GetProperty("id").GetString() ?? string.Empty,
                SubscriptionId: item.GetProperty("subscriptionId").GetString() ?? string.Empty,
                ResourceGroup: item.GetProperty("resourceGroup").GetString() ?? string.Empty,
                ResourceType: item.GetProperty("type").GetString() ?? string.Empty,
                ResourceKind: item.TryGetProperty("kind", out var kindProp) ? kindProp.GetString() ?? string.Empty : string.Empty,
                ResourceName: item.GetProperty("name").GetString() ?? string.Empty));
        }

        return [.. results];
    }

    /// <summary>
    /// Checks whether a resource type (and optionally kind) is supported by AppLens diagnostics.
    /// </summary>
    internal static bool IsResourceTypeSupported(string resourceType, string resourceKind)
    {
        if (resourceType.Equals("microsoft.web/sites", StringComparison.OrdinalIgnoreCase))
        {
            return resourceKind.Equals("app", StringComparison.OrdinalIgnoreCase)
                || resourceKind.Equals("linux", StringComparison.OrdinalIgnoreCase)
                || resourceKind.Equals("functionapp", StringComparison.OrdinalIgnoreCase);
        }

        return resourceType.Equals("microsoft.containerservice/managedclusters", StringComparison.OrdinalIgnoreCase)
            || resourceType.Equals("microsoft.apimanagement/service", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns the list of resource types supported by AppLens diagnostics.
    /// </summary>
    internal static IEnumerable<string> SupportedResourceTypes()
    {
        yield return "microsoft.web/sites";
        yield return "microsoft.containerservice/managedclusters";
        yield return "microsoft.apimanagement/service";
    }

    private async Task<GetAppLensSessionResult> GetAppLensSessionAsync(string resourceId, string? tenantId = null, CancellationToken cancellationToken = default)
    {
        // Get Azure credential using BaseAzureService
        var credential = await GetCredential(tenantId, cancellationToken);

        // Get ARM token
        var token = await credential.GetTokenAsync(
            new TokenRequestContext([GetManagementImpersonationEndpoint()]),
            cancellationToken);

        // Call the AppLens token endpoint
        using var request = new HttpRequestMessage(HttpMethod.Get,
            GetAppLensTokenEndpoint(resourceId));

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

        var client = _httpClientFactory.CreateClient();
        using var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new FailedAppLensSessionResult("The specified resource could not be found or does not support diagnostics.");
            }
            return new FailedAppLensSessionResult($"Failed to create diagnostics session for resource {resourceId}, http response code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var appLensSession = ParseGetTokenResponse(content);

        return new SuccessfulAppLensSessionResult(appLensSession with { ResourceId = resourceId });
    }

    /// <summary>
    /// Asks the AppLens API a single <paramref name="question"/> about a resource associated with the given <paramref name="session"/> and returns the response as stream of asynchronous messages.
    /// </summary>
    /// <param name="question">The question or query to pose to AppLens.</param>
    /// <param name="session">The <see cref="AppLensSession"/> representing the overall conversation.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
    public async IAsyncEnumerable<ChatMessageResponseBody> AskAppLensAsync(
        AppLensSession session,
        string question,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // agent using signal r client to stream answers
        Channel<ChatMessageResponseBody> channel = Channel.CreateUnbounded<ChatMessageResponseBody>(new()
        {
            SingleWriter = true,
            SingleReader = true,
            AllowSynchronousContinuations = false
        });

        await using HubConnection connection = new HubConnectionBuilder()
            .AddJsonProtocol(options =>
            {
                // TypeInfo resolver chain needs to be update to use AppLensJsonContext first to resolve App Lens
                // types. Without this it'll attempt to use reflection-based serialization, which will result in a
                // runtime error when using the native AoT server, as that disables reflection.
                // More information about configuring HubConnection JSON serialization can be found here:
                // https://learn.microsoft.com/aspnet/core/signalr/configuration?view=aspnetcore-9.0&tabs=dotnet#jsonmessagepack-serialization-options
                options.PayloadSerializerOptions.TypeInfoResolverChain.Insert(0, AppLensJsonContext.Default);
            })
            .WithUrl(new Uri(GetConversationalDiagnosticsSignalREndpoint()), options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(session.Token)!;
                options.Headers.Add("origin", GetDiagnosticsPortalEndpoint());
            })
            .WithAutomaticReconnect()
            .Build();

        await connection.StartAsync(cancellationToken);

        connection.On<string>("MessageReceived", async (response) =>
        {
            ChatMessageResponseBody responseBody = ChatMessageResponseBody.FromJson(response);
            await channel.Writer.WriteAsync(responseBody, cancellationToken);
        });

        connection.On<string>("MessageCancelled", async (response) =>
        {
            // If the API cancels the request just treat it as the conversation being complete.
            ChatMessageResponseBody responseBody = new()
            {
                Error = response,
                SessionId = "",
                Message = new ChatMessage
                {
                    Id = Guid.NewGuid().ToString(),
                    DisplayMessage = "Request cancelled",
                    Message = "Request cancelled",
                    MessageDisplayDate = DateTime.UtcNow.ToString("O"),
                    UserFeedback = ""
                },
                SuggestedPrompts = [],
                DebugTrace = "",
                ResponseStatus = QueryResponseStatus.Finished,
                ResponseType = MessageResponseType.SystemMessage
            };

            await channel.Writer.WriteAsync(responseBody, cancellationToken);
        });

        bool completed = false;

        // Read the token so we can get the user ID
        JwtSecurityTokenHandler tokenHandler = new();
        JwtSecurityToken jsonToken = (JwtSecurityToken)tokenHandler.ReadToken(session.Token);
        string userId = jsonToken.Claims.First(claim => claim.Type == "user").Value;

        // fill this from context and request
        ChatMessageRequestBody appLensRequest = new()
        {
            ResourceId = session.ResourceId,
            SessionId = session.SessionId,
            Message = question,
            UserId = userId,
            ResourceKind = "app",
            StartTime = "",
            EndTime = "",
            IsDiagnosticsCall = true,
            ClientName = _options.ClientName
        };

        // fire request
        Task request = connection.InvokeAsync("sendMessage", JsonSerializer.Serialize(appLensRequest, AppLensJsonContext.Default.ChatMessageRequestBody), cancellationToken: cancellationToken);

        bool waitForRequestToFinish = true;
        while (!completed)
        {
            // Ending session if we don't get any messages for some period of time.
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.MessageTimeOutInSeconds));

            try
            {
                await channel.Reader.WaitToReadAsync(cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Cancellation has been triggered by the timeout waiting for the service to respond. Finish up as if it had completed normally
                // rather than let the exception propagate.
                completed = true;
                // We're bailing out early, so we don't need to wait for the request to finish.
                waitForRequestToFinish = false;
            }

            while (channel.Reader.TryRead(out ChatMessageResponseBody? message))
            {
                completed |= message.ResponseStatus == QueryResponseStatus.Finished;

                yield return message;
            }
        }

        if (waitForRequestToFinish || request.IsFaulted)
        {
            await request;
        }

        await connection.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Collects insights from AppLens diagnostic conversation.
    /// </summary>
    /// <param name="session">The AppLens session.</param>
    /// <param name="question">The diagnostic question.</param>
    /// <returns>A task containing diagnostic insights and solutions.</returns>
    private async Task<DiagnosticResult> CollectInsightsAsync(AppLensSession session, string question, CancellationToken cancellationToken)
    {
        var insights = new List<string>();
        var solutions = new List<string>();

        await foreach (var message in AskAppLensAsync(session, question, cancellationToken))
        {
            if (message.ResponseType == MessageResponseType.SystemMessage && !string.IsNullOrEmpty(message.Message?.Message))
            {
                insights.Add(message.Message.Message);
            }

            if (message.ResponseType == MessageResponseType.LoadingMessage && !string.IsNullOrEmpty(message.Message?.Message))
            {
                solutions.Add(message.Message.Message);
            }
        }

        return new(insights, solutions, session.ResourceId, "Resource");
    }

    private static AppLensSession ParseGetTokenResponse(string rawResponse)
    {
        using var jsonDoc = JsonDocument.Parse(rawResponse);

        AppLensSession? session = null;

        // parse response
        const int SessionIdColumnIndex = 0;
        const int TokenColumnIndex = 1;
        const int ExpiresInColumnIndex = 2;

        if (!jsonDoc.RootElement.TryGetProperty("properties", out JsonElement propertiesElement))
        {
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Object)
            {
                IEnumerable<string> propertyNames = jsonDoc.RootElement.EnumerateObject().Select(property => property.Name);
                string joinedPropertyNames = string.Join(", ", propertyNames);
                throw new Exception($"The top-level property named 'properties' not found. The actual top-level properties are: {joinedPropertyNames}.");
            }

            throw new Exception("'properties' not found. Root element is not a JSON object.");
        }

        if (!propertiesElement.TryGetProperty("dataset", out JsonElement datasetElement))
        {
            throw new Exception("'properties.dataset' not found.");
        }

        foreach (JsonElement datasetItem in datasetElement.EnumerateArray())
        {
            if (!datasetItem.TryGetProperty("table", out JsonElement tableElement))
            {
                throw new Exception("'properties.dataset.table' not found.");
            }

            if (!tableElement.TryGetProperty("rows", out JsonElement rowsElement))
            {
                throw new Exception("'properties.dataset.table.rows' not found.");
            }

            foreach (JsonElement rowJsonElement in rowsElement.EnumerateArray())
            {
                JsonElement[] row = rowJsonElement.Deserialize(AppLensJsonContext.Default.JsonElementArray)!;

                session = new(
                    SessionId: row[SessionIdColumnIndex].GetString()!,
                    ResourceId: "", // Filled in by the caller
                    Token: row[TokenColumnIndex].GetString()!,
                    ExpiresIn: row[ExpiresInColumnIndex].GetInt32()
                );

                break;
            }
        }

        if (session is null)
        {
            throw new Exception("session is null.");
        }

        return session;
    }

    private string GetConversationalDiagnosticsSignalREndpoint()
    {
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => "https://diagnosticschat.azure.com/chatHub",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => "https://diagnosticschat.azure.cn/chatHub",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => "https://diagnosticschat.azure.us/chatHub",
            _ => "https://diagnosticschat.azure.com/chatHub",
        };
    }

    private string GetManagementImpersonationEndpoint()
    {
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => "https://management.azure.com/user_impersonation",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => "https://management.chinacloudapi.cn/user_impersonation",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => "https://management.usgovcloudapi.net/user_impersonation",
            _ => "https://management.azure.com/user_impersonation",
        };
    }

    private string GetAppLensTokenEndpoint(string resourceId)
    {
        const string detectorsTokenPath = "detectors/GetToken-db48586f-7d94-45fc-88ad-b30ccd3b571c?api-version=2015-08-01";
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => $"https://management.azure.com/{resourceId}/{detectorsTokenPath}",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => $"https://management.chinacloudapi.cn/{resourceId}/{detectorsTokenPath}",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => $"https://management.usgovcloudapi.net/{resourceId}/{detectorsTokenPath}",
            _ => $"https://management.azure.com/{resourceId}/{detectorsTokenPath}",
        };
    }

    private string GetDiagnosticsPortalEndpoint()
    {
        return _tenantService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => "https://appservice-diagnostics.trafficmanager.net",
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => "https://appservice-diagnostics.azure.cn",
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => "https://appservice-diagnostics.azure.us",
            _ => "https://appservice-diagnostics.trafficmanager.net",
        };
    }
}
