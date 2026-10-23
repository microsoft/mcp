// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers.Text;
using System.ClientModel.Primitives;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Models.Paging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Base class for Azure services that need to query Azure Resource Graph for resource management operations.
/// Provides common methods for executing resource queries against Azure Resource Manager resources.
/// </summary>
public abstract class BaseAzureResourceService(IAzureService azureService)
    : BaseAzureService(azureService)
{
    private const string ResourceGraphApiVersion = "2024-04-01";

    /// <summary>
    /// Validates that the specified resource group exists within the given subscription.
    /// </summary>
    /// <param name="subscriptionResource">The subscription resource to check against.</param>
    /// <param name="resourceGroupName">The name of the resource group to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the resource group exists; otherwise, false.</returns>
    private static async Task<bool> ValidateResourceGroupExistsAsync(
        SubscriptionResource subscriptionResource,
        string resourceGroupName,
        CancellationToken cancellationToken = default)
    {
        var resourceGroupCollection = subscriptionResource.GetResourceGroups();
        var result = await resourceGroupCollection.ExistsAsync(resourceGroupName, cancellationToken).ConfigureAwait(false);
        return result.Value;
    }

    /// <summary>
    /// Executes a Resource Graph query and returns a list of resources of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert each resource to</typeparam>
    /// <param name="resourceType">The Azure resource type to query for (e.g., "Microsoft.Sql/servers/databases")</param>
    /// <param name="resourceGroup">The resource group name to filter by (null to query all resource groups)</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="converter">Function to convert JsonElement to the target type</param>
    /// <param name="tableName">Optional table name to query (default: "resources")</param>
    /// <param name="additionalFilter">Optional additional KQL filter condition</param>
    /// <param name="limit">Maximum number of results to return (default: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="tenant">Optional tenant to use for the query</param>
    /// <returns>List of resources converted to the specified type</returns>
    protected async Task<ResourceQueryResults<T>> ExecuteResourceQueryAsync<T>(
        string resourceType,
        string? resourceGroup,
        string subscription,
        Func<JsonElement, T> converter,
        string? tableName = "resources",
        string? additionalFilter = null,
        int limit = 50,
        CancellationToken cancellationToken = default,
        string? tenant = null)
    {
        ValidateRequiredParameters((nameof(resourceType), resourceType), (nameof(subscription), subscription));
        ArgumentNullException.ThrowIfNull(converter);
        ValidateAdditionalFilter(additionalFilter);

        var (subscriptionResource, tenantId) = await ResolveResourceQueryScopeAsync(subscription, tenant, cancellationToken);

        var queryFilter = BuildResourceQuery(tableName, resourceType, resourceGroup, additionalFilter, limit);

        var queryContent = new ResourceQueryContent(queryFilter)
        {
            Subscriptions = { subscriptionResource.Data.SubscriptionId }
        };

        var (result, _) = await ExecuteResourceGraphQueryAsync(queryContent, tenantId, converter, cancellationToken);

        if (result.Results.Count == 0 && !string.IsNullOrEmpty(resourceGroup))
        {
            // If the query returned no results and a resource group filter was applied, validate that the resource group exists to provide better error handling
            if (!await ValidateResourceGroupExistsAsync(subscriptionResource, resourceGroup, cancellationToken))
            {
                throw new KeyNotFoundException($"Resource group '{resourceGroup}' does not exist in subscription '{subscriptionResource.Data.SubscriptionId}'");
            }
        }

        return result;
    }

    /// <summary>
    /// Returns one ordered Resource Graph page with continuation that can be replayed on another instance.
    /// </summary>
    /// <typeparam name="T">The model to which each resource is converted.</typeparam>
    /// <param name="resourceType">The Azure resource type to query.</param>
    /// <param name="resourceGroup">The resource group filter, or <see langword="null"/> for the subscription scope.</param>
    /// <param name="subscription">The subscription ID or name to resolve for the query.</param>
    /// <param name="converter">The function that converts each resource JSON object to <typeparamref name="T"/>.</param>
    /// <param name="page">
    /// The requested page size and optional continuation. An omitted size uses
    /// <see cref="PageRequest.DefaultPageSize"/> for the first page or retains the previous page size.
    /// </param>
    /// <param name="tenant">The optional tenant ID or name used when resolving the subscription.</param>
    /// <param name="cancellationToken">The token that cancels the Azure requests.</param>
    /// <returns>
    /// The converted resources, truncation state, and paging metadata with resolved scope.
    /// An empty resource collection may still have continuation.
    /// </returns>
    /// <remarks>
    /// Uses Resource Graph page-size and skip-token options with resource-ID ordering instead of a KQL
    /// limit. Continuation must preserve the query, resolved subscription and tenant, cloud, and page size.
    /// Each page authenticates independently and requires no state on the issuing server instance.
    /// Resource Graph indexing and concurrent resource changes mean the iteration is not a snapshot.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="converter"/> or <paramref name="page"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// A required input or paging value is invalid, or the continuation is malformed, unsupported,
    /// or inconsistent with the current request.
    /// </exception>
    /// <exception cref="KeyNotFoundException">The subscription or an explicitly queried resource group does not exist.</exception>
    /// <exception cref="InvalidOperationException">The tenant ID cannot be determined for the subscription.</exception>
    /// <exception cref="HttpRequestException">
    /// The backend page is invalid, exceeds the requested item bound, or cannot provide usable continuation.
    /// </exception>
    /// <exception cref="RequestFailedException">An Azure Resource Graph request fails.</exception>
    protected async Task<ResourceQueryResults<T>> ExecuteResourceQueryPageAsync<T>(
        string resourceType,
        string? resourceGroup,
        string subscription,
        Func<JsonElement, T> converter,
        PageRequest page,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(resourceType), resourceType), (nameof(subscription), subscription));
        ArgumentNullException.ThrowIfNull(converter);
        PagingOptionValidator.Validate(page);

        var cursor = ReadContinuationToken(page.ContinuationToken);
        var pageSize = page.PageSize ?? cursor?.PageSize ?? PageRequest.DefaultPageSize;
        if (cursor is not null && cursor.PageSize != pageSize)
        {
            throw new ArgumentException("--page-size must match the previous page. Replay paging.continuationArguments unchanged.", nameof(page));
        }

        var (subscriptionResource, tenantId) = await ResolveResourceQueryScopeAsync(subscription, tenant, cancellationToken);
        var subscriptionId = subscriptionResource.Data.SubscriptionId;
        var cloudEndpoint = AzureService.CloudConfiguration.ArmEnvironment.Endpoint.AbsoluteUri;
        var normalizedResourceGroup = resourceGroup?.ToLowerInvariant();
        var query = BuildResourceQuery("resources", resourceType, normalizedResourceGroup, additionalFilter: null, limit: null);

        if (cursor is not null &&
            (!string.Equals(cursor.CloudEndpoint, cloudEndpoint, StringComparison.OrdinalIgnoreCase) ||
             !string.Equals(cursor.Subscription, subscriptionId, StringComparison.OrdinalIgnoreCase) ||
             !string.Equals(cursor.Tenant, tenantId, StringComparison.OrdinalIgnoreCase) ||
             !string.Equals(cursor.Query, query, StringComparison.Ordinal)))
        {
            throw new ArgumentException("--continuation-token does not match this resource query, subscription, tenant, or cloud. Replay paging.continuationArguments unchanged.", nameof(page));
        }

        var queryContent = new ResourceQueryContent(query)
        {
            Subscriptions = { subscriptionId },
            Options = new ResourceQueryRequestOptions
            {
                Top = pageSize,
                SkipToken = cursor?.SkipToken
            }
        };

        var (result, skipToken) = await ExecuteResourceGraphQueryAsync(
            queryContent, tenantId, converter, cancellationToken, pageSize);

        if (cursor is null && result.Results.Count == 0 && skipToken is null &&
            !string.IsNullOrEmpty(resourceGroup) &&
            !await ValidateResourceGroupExistsAsync(subscriptionResource, resourceGroup, cancellationToken))
        {
            throw new KeyNotFoundException($"Resource group '{resourceGroup}' does not exist in subscription '{subscriptionId}'");
        }

        string? continuationToken = null;
        if (skipToken is not null)
        {
            continuationToken = Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(
                new ResourceGraphContinuationToken(1, pageSize, cloudEndpoint, subscriptionId, tenantId, query, skipToken),
                ResourceGraphPagingJsonContext.Default.ResourceGraphContinuationToken));
            if (continuationToken.Length > PageRequest.MaxContinuationTokenLength)
            {
                throw new HttpRequestException("Resource Graph returned a continuation token that exceeds the supported limit.", null, HttpStatusCode.BadGateway);
            }
        }

        return result with
        {
            Page = new(pageSize, continuationToken, subscriptionId, tenantId, resourceGroup)
        };
    }

    /// <summary>
    /// Resolves the subscription resource and tenant ID used to execute and scope a resource query.
    /// </summary>
    /// <param name="subscription">The subscription ID or name to resolve.</param>
    /// <param name="tenant">The optional tenant ID or name used for subscription resolution.</param>
    /// <param name="cancellationToken">The token that cancels scope resolution.</param>
    /// <returns>The subscription resource and resolved tenant ID.</returns>
    /// <exception cref="KeyNotFoundException">The subscription cannot be found.</exception>
    /// <exception cref="InvalidOperationException">No tenant ID can be determined for the subscription.</exception>
    private async Task<(SubscriptionResource Subscription, string Tenant)> ResolveResourceQueryScopeAsync(
        string subscription,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken)
            ?? throw new KeyNotFoundException($"Subscription '{subscription}' was not found.");
        var tenantId = subscriptionResource.Data.TenantId?.ToString()
            ?? await AzureService.ResolveTenantIdAsync(tenant, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Subscription '{subscriptionResource.Data.SubscriptionId}' does not have a tenant ID.");
        return (subscriptionResource, tenantId);
    }

    /// <summary>
    /// Decodes a continuation payload and validates its version, page size, and required fields.
    /// </summary>
    /// <param name="continuationToken">The encoded token, or <see langword="null"/> for the first page.</param>
    /// <returns>The decoded payload, or <see langword="null"/> when no token was supplied.</returns>
    /// <remarks>Request-scope matching and authorization are performed separately by the query service.</remarks>
    /// <exception cref="ArgumentException">The token is malformed or its payload is invalid or unsupported.</exception>
    private static ResourceGraphContinuationToken? ReadContinuationToken(string? continuationToken)
    {
        if (continuationToken is null)
        {
            return null;
        }

        ResourceGraphContinuationToken? cursor;
        try
        {
            cursor = JsonSerializer.Deserialize(
                Base64Url.DecodeFromChars(continuationToken),
                ResourceGraphPagingJsonContext.Default.ResourceGraphContinuationToken);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            throw new ArgumentException("Invalid --continuation-token. Use the token returned by a previous page.", nameof(continuationToken), ex);
        }

        if (cursor is null || cursor.Version != 1 ||
            cursor.PageSize is < 1 or > PageRequest.MaxPageSize ||
            string.IsNullOrWhiteSpace(cursor.CloudEndpoint) ||
            string.IsNullOrWhiteSpace(cursor.Subscription) ||
            string.IsNullOrWhiteSpace(cursor.Tenant) ||
            string.IsNullOrWhiteSpace(cursor.Query) ||
            string.IsNullOrWhiteSpace(cursor.SkipToken))
        {
            throw new ArgumentException("Invalid or unsupported --continuation-token. Use the token returned by a previous page.", nameof(continuationToken));
        }

        return cursor;
    }

    /// <summary>
    /// Executes a Resource Graph query scoped to a management group and returns a list of resources of the specified type.
    /// </summary>
    /// <remarks>
    /// Resource Graph evaluates a query only within the scopes it is given. A subscription-scoped query therefore
    /// cannot see resources whose IDs live outside any subscription, such as role assignments made directly on a
    /// management group. Use this overload when the caller asked about a management group scope.
    /// </remarks>
    /// <typeparam name="T">The type to convert each resource to</typeparam>
    /// <param name="resourceType">The Azure resource type to query for</param>
    /// <param name="managementGroup">The management group ID to scope the query to</param>
    /// <param name="converter">Function to convert JsonElement to the target type</param>
    /// <param name="tableName">Optional table name to query (default: "resources")</param>
    /// <param name="additionalFilter">Optional additional KQL filter condition</param>
    /// <param name="limit">Maximum number of results to return (default: 50)</param>
    /// <param name="tenant">Optional tenant to use for the query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of resources converted to the specified type</returns>
    protected async Task<ResourceQueryResults<T>> ExecuteManagementGroupResourceQueryAsync<T>(
        string resourceType,
        string managementGroup,
        Func<JsonElement, T> converter,
        string? tableName = "resources",
        string? additionalFilter = null,
        int limit = 50,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(resourceType), resourceType), (nameof(managementGroup), managementGroup));
        ArgumentNullException.ThrowIfNull(converter);
        ValidateAdditionalFilter(additionalFilter);

        var tenantId = await ResolveTenantIdAsync(tenant, cancellationToken);

        var queryFilter = BuildResourceQuery(tableName, resourceType, resourceGroup: null, additionalFilter, limit);

        var queryContent = new ResourceQueryContent(queryFilter)
        {
            ManagementGroups = { managementGroup }
        };

        var (result, _) = await ExecuteResourceGraphQueryAsync(queryContent, tenantId, converter, cancellationToken);
        return result;
    }

    private static void ValidateAdditionalFilter(string? additionalFilter)
    {
        if (!string.IsNullOrEmpty(additionalFilter) && additionalFilter.Contains('|'))
        {
            throw new ArgumentException(
                "additionalFilter must not contain the pipe operator '|' to prevent KQL injection.",
                nameof(additionalFilter));
        }
    }

    /// <summary>
    /// Builds either a limited resource query or a resource-ID-ordered query suitable for continuation.
    /// </summary>
    /// <param name="tableName">The Resource Graph table to query.</param>
    /// <param name="resourceType">The resource type to filter.</param>
    /// <param name="resourceGroup">The optional resource group filter.</param>
    /// <param name="additionalFilter">The optional additional KQL filter condition.</param>
    /// <param name="limit">
    /// The legacy KQL result limit, or <see langword="null"/> to order by resource ID and use request-level paging.
    /// </param>
    /// <returns>The KQL resource query.</returns>
    private static string BuildResourceQuery(
        string? tableName,
        string resourceType,
        string? resourceGroup,
        string? additionalFilter,
        int? limit)
    {
        var queryFilter = $"{tableName} | where type =~ '{EscapeKqlString(resourceType)}'";
        if (!string.IsNullOrEmpty(resourceGroup))
        {
            queryFilter += $" and resourceGroup =~ '{EscapeKqlString(resourceGroup)}'";
        }
        if (!string.IsNullOrEmpty(additionalFilter))
        {
            queryFilter += $" and {additionalFilter}";
        }

        return limit.HasValue
            ? queryFilter + $" | limit {limit.Value}"
            : queryFilter + " | order by id asc";
    }

    /// <summary>
    /// Executes a Resource Graph request and converts its resource data, validating page boundaries when requested.
    /// </summary>
    /// <typeparam name="T">The model to which each resource is converted.</typeparam>
    /// <param name="queryContent">The query, resource scope, and optional provider paging options.</param>
    /// <param name="tenantId">The resolved tenant ID used to authenticate the request.</param>
    /// <param name="converter">The function that converts each resource JSON object.</param>
    /// <param name="cancellationToken">The token that cancels the request or response parsing.</param>
    /// <param name="pageSize">
    /// The maximum permitted number of returned resources. When non-null, also validates the completion
    /// indicator and continuation token; <see langword="null"/> preserves legacy response handling.
    /// </param>
    /// <returns>The converted result and, for a paged request, the provider's next-page skip token.</returns>
    /// <exception cref="RequestFailedException">The Resource Graph request fails.</exception>
    /// <exception cref="HttpRequestException">The backend page or its continuation metadata is invalid.</exception>
    private async Task<(ResourceQueryResults<T> Result, string? SkipToken)> ExecuteResourceGraphQueryAsync<T>(
        ResourceQueryContent queryContent,
        string tenantId,
        Func<JsonElement, T> converter,
        CancellationToken cancellationToken,
        int? pageSize = null)
    {
        var token = await GetArmAccessTokenAsync(tenantId, cancellationToken);
        using var client = AzureService.GetClient();
        var clientOptions = AddDefaultPolicies(new ArmClientOptions
        {
            Transport = new HttpClientTransport(client)
        });
        var pipeline = HttpPipelineBuilder.Build(clientOptions);

        var requestUri = new Uri(
            AzureService.CloudConfiguration.ArmEnvironment.Endpoint,
            $"/providers/Microsoft.ResourceGraph/resources?api-version={ResourceGraphApiVersion}");
        using var request = pipeline.CreateRequest();
        request.Method = RequestMethod.Post;
        request.Uri.Reset(requestUri);
        request.Headers.Add("Authorization", $"Bearer {token.Token}");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Content-Type", "application/json");
        request.Headers.Add("x-ms-client-request-id", Guid.NewGuid().ToString());
        request.Headers.Add("x-ms-return-client-request-id", "true");
        request.Content = CreateResourceGraphRequestContent(queryContent);

        using var response = await pipeline.SendRequestAsync(request, cancellationToken);
        if (response.IsError)
        {
            throw new RequestFailedException(response);
        }

        await using var responseStream = response.Content.ToStream();
        using var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        var root = document.RootElement;
        if (pageSize.HasValue && root.ValueKind != JsonValueKind.Object)
        {
            throw new HttpRequestException("Resource Graph returned an invalid resource page.", null, HttpStatusCode.BadGateway);
        }
        var results = new List<T>();
        var hasData = root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array;
        if (pageSize.HasValue && (!hasData || data.GetArrayLength() > pageSize.Value))
        {
            throw new HttpRequestException("Resource Graph returned an invalid or oversized resource page.", null, HttpStatusCode.BadGateway);
        }
        if (hasData)
        {
            foreach (var item in data.EnumerateArray())
            {
                results.Add(converter(item));
            }
        }

        var isTruncated = root.TryGetProperty("resultTruncated", out var resultTruncated)
            && (resultTruncated.ValueKind == JsonValueKind.True
                || resultTruncated.ValueKind == JsonValueKind.String
                && string.Equals(resultTruncated.GetString(), "true", StringComparison.OrdinalIgnoreCase));

        string? skipToken = null;
        if (pageSize.HasValue)
        {
            var validCompletionIndicator = resultTruncated.ValueKind is JsonValueKind.True or JsonValueKind.False;
            if (resultTruncated.ValueKind == JsonValueKind.String)
            {
                validCompletionIndicator = bool.TryParse(resultTruncated.GetString(), out isTruncated);
            }
            if (!validCompletionIndicator)
            {
                throw new HttpRequestException("Resource Graph returned an invalid page completion indicator.", null, HttpStatusCode.BadGateway);
            }

            if (root.TryGetProperty("$skipToken", out var skipTokenElement) && skipTokenElement.ValueKind != JsonValueKind.Null)
            {
                if (skipTokenElement.ValueKind != JsonValueKind.String)
                {
                    throw new HttpRequestException("Resource Graph returned an invalid continuation token.", null, HttpStatusCode.BadGateway);
                }
                skipToken = skipTokenElement.GetString();
                if (skipToken is { Length: 0 })
                {
                    skipToken = null;
                }
                else if (skipToken is not null && string.IsNullOrWhiteSpace(skipToken))
                {
                    throw new HttpRequestException("Resource Graph returned an invalid continuation token.", null, HttpStatusCode.BadGateway);
                }
            }
            if (isTruncated && skipToken is null)
            {
                throw new HttpRequestException("Resource Graph truncated the resource page without a usable continuation token.", null, HttpStatusCode.BadGateway);
            }
        }

        return (new(results, isTruncated || skipToken is not null), skipToken);
    }

    /// <summary>
    /// Resolves the tenant ID to run a query against when no subscription is available to derive it from.
    /// </summary>
    private async Task<string> ResolveTenantIdAsync(string? tenant, CancellationToken cancellationToken)
    {
        var tenantId = await AzureService.ResolveTenantIdAsync(tenant, cancellationToken);
        if (!string.IsNullOrEmpty(tenantId))
        {
            return tenantId;
        }

        var tenants = await AzureService.GetTenants(cancellationToken);
        if (tenants.Count == 1)
        {
            return tenants[0].Data.TenantId?.ToString()
                ?? throw new InvalidOperationException("The accessible Azure tenant does not have a tenant ID.");
        }

        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found for the current credential.");
        }

        throw new ArgumentException(
            "Multiple tenants are accessible, so the tenant to query cannot be inferred. Specify the tenant explicitly.",
            nameof(tenant));
    }

    /// <summary>
    /// Serializes the Resource Graph query and scope, including object-array format and paging options when present.
    /// </summary>
    /// <param name="queryContent">The query content to serialize.</param>
    /// <returns>The JSON request body for the Resource Graph endpoint.</returns>
    private static RequestContent CreateResourceGraphRequestContent(ResourceQueryContent queryContent)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("query", queryContent.Query);
            WriteStringArray(writer, "subscriptions", queryContent.Subscriptions);
            WriteStringArray(writer, "managementGroups", queryContent.ManagementGroups);
            if (queryContent.Options is { } options)
            {
                writer.WriteStartObject("options");
                writer.WriteString("resultFormat", "objectArray");
                if (options.Top is { } top)
                {
                    writer.WriteNumber("$top", top);
                }
                if (options.SkipToken is not null)
                {
                    writer.WriteString("$skipToken", options.SkipToken);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        return RequestContent.Create(stream.ToArray());
    }

    private static void WriteStringArray(
        Utf8JsonWriter writer,
        string propertyName,
        IEnumerable<string> values)
    {
        if (!values.Any())
        {
            return;
        }

        writer.WriteStartArray(propertyName);
        foreach (var value in values)
        {
            writer.WriteStringValue(value);
        }
        writer.WriteEndArray();
    }

    /// <summary>
    /// Executes a Resource Graph query and returns a single resource of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the resource to</typeparam>
    /// <param name="resourceType">The Azure resource type to query for (e.g., "Microsoft.Sql/servers/databases")</param>
    /// <param name="resourceGroup">The resource group name to filter by (null to query all resource groups)</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="converter">Function to convert JsonElement to the target type</param>
    /// <param name="additionalFilter">Optional additional KQL filter condition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Single resource converted to the specified type, or null if not found</returns>
    protected async Task<T?> ExecuteSingleResourceQueryAsync<T>(
        string resourceType,
        string? resourceGroup,
        string subscription,
        Func<JsonElement, T> converter,
        string? tableName = "resources",
        string? additionalFilter = null,
        string? tenant = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var result = await ExecuteResourceQueryAsync(resourceType, resourceGroup, subscription, converter,
            tableName, additionalFilter, 1, cancellationToken, tenant).ConfigureAwait(false);
        return result.Results.FirstOrDefault();
    }

    /// <summary>
    /// Create an <see cref="ArmClient"/> with the specified API version set for the given resource type.
    /// This wraps <see cref="BaseAzureService.CreateArmClientAsync"/> and configures the <see cref="ArmClientOptions"/> appropriately.
    /// </summary>
    /// <param name="resourceTypeForApiVersion">The resource type token used by the SDK to set a specific API version, e.g. "Microsoft.CognitiveServices/accounts/deployments".</param>
    /// <param name="apiVersion">The API version to set for the specified resource type.</param>
    /// <param name="tenant">Optional tenant to use when creating the client.</param>
    /// <returns>An initialized <see cref="ArmClient"/> configured with the requested API version.</returns>
    protected async Task<ArmClient> CreateArmClientWithApiVersionAsync(
        string resourceTypeForApiVersion,
        string apiVersion,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        var options = new ArmClientOptions();
        options.SetApiVersion(resourceTypeForApiVersion, apiVersion);
        return await CreateArmClientAsync(tenant, options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieve a GenericResource by its <see cref="ResourceIdentifier"/> using the provided <see cref="ArmClient"/>.
    /// This method centralizes the call to GenericResources.GetAsync and validates that the resource contains data.
    /// </summary>
    /// <param name="armClient">The ArmClient to use for the call.</param>
    /// <param name="resourceIdentifier">The resource identifier of the resource to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The <see cref="GenericResource"/> instance for the requested resource.</returns>
    /// <exception cref="ArgumentNullException">Thrown when a required parameter is null.</exception>
    protected static async Task<GenericResource> GetGenericResourceAsync(
        ArmClient armClient,
        ResourceIdentifier resourceIdentifier,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(armClient);

        var genericResources = armClient.GetGenericResources();
        var response = await genericResources.GetAsync(resourceIdentifier, cancellationToken).ConfigureAwait(false);
        var resource = response.Value;
        if (!resource.HasData)
        {
            throw new InvalidOperationException($"Resource '{resourceIdentifier}' not found or not accessible.");
        }

        return resource;
    }

    /// <summary>
    /// Creates or updates a GenericResource with the specified content of type T.
    /// </summary>
    /// <typeparam name="T">Type of the content.</typeparam>
    /// <param name="armClient">The ArmClient instance to use for the operation.</param>
    /// <param name="resourceIdentifier">The resource identifier of the resource to create or update.</param>
    /// <param name="azureLocation">The Azure location for the resource.</param>
    /// <param name="content">The content to create or update the resource with.</param>
    /// <param name="jsonTypeInfo">The JSON type information for serialization.</param>
    /// <returns>The <see cref="GenericResource"/> instance for the requested resource.</returns>
    /// <exception cref="ArgumentNullException">Thrown when a required parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the content is invalid.</exception>
    protected static async Task<GenericResource> CreateOrUpdateGenericResourceAsync<T>(
        ArmClient armClient,
        ResourceIdentifier resourceIdentifier,
        AzureLocation azureLocation,
        T content,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(armClient);

        // Convert from T to GenericResourceData
        byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(content, jsonTypeInfo);
        var reader = new Utf8JsonReader(jsonBytes);
        var dataModel = (IJsonModel<GenericResourceData>)new GenericResourceData(azureLocation);
        GenericResourceData data = dataModel.Create(ref reader, new ModelReaderWriterOptions("W"))
            ?? throw new InvalidOperationException("Failed to create deployment data");
        // Create the resource
        var result = await armClient.GetGenericResources().CreateOrUpdateAsync(WaitUntil.Started, resourceIdentifier, data, cancellationToken);
        await WaitForLroCompletionAsync(result, cancellationToken);
        return result.Value;
    }
}

/// <summary>
/// Contains converted Resource Graph results, their truncation state, and optional paging information.
/// </summary>
/// <typeparam name="T">The converted resource model.</typeparam>
/// <param name="Results">The resources returned by the query.</param>
/// <param name="AreResultsTruncated">Whether the response indicates that additional matching resources were not returned.</param>
public sealed record ResourceQueryResults<T>(List<T> Results, bool AreResultsTruncated)
{
    /// <summary>
    /// Gets the page state and resolved continuation scope, or <see langword="null"/> for a legacy unpaged query.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResourceQueryPage? Page { get; init; }
}
