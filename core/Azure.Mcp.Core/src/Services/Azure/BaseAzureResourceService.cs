// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;

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

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken);
        var tenantId = subscriptionResource!.Data.TenantId?.ToString()
            ?? await AzureService.ResolveTenantIdAsync(tenant, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Subscription '{subscriptionResource.Data.SubscriptionId}' does not have a tenant ID.");

        var queryFilter = BuildResourceQuery(tableName, resourceType, resourceGroup, additionalFilter, limit);

        var queryContent = new ResourceQueryContent(queryFilter)
        {
            Subscriptions = { subscriptionResource.Data.SubscriptionId }
        };

        var result = await ExecuteResourceGraphQueryAsync(queryContent, tenantId, converter, cancellationToken);

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

        return await ExecuteResourceGraphQueryAsync(queryContent, tenantId, converter, cancellationToken);
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

    private static string BuildResourceQuery(
        string? tableName,
        string resourceType,
        string? resourceGroup,
        string? additionalFilter,
        int limit)
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

        return queryFilter + $" | limit {limit}";
    }

    /// <summary>
    /// Executes a Resource Graph query and returns the parsed query result.
    /// </summary>
    /// <param name="queryContent">The Resource Graph query content.</param>
    /// <param name="tenantId">The target tenant ID to scope authentication to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="ResourceGraphQueryResult"/> containing the parsed response.</returns>
    protected async Task<ResourceGraphQueryResult> ExecuteResourceGraphQueryAsync(
        ResourceQueryContent queryContent,
        string tenantId,
        CancellationToken cancellationToken)
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
        var document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        return new ResourceGraphQueryResult(document);
    }

    protected async Task<ResourceQueryResults<T>> ExecuteResourceGraphQueryAsync<T>(
        ResourceQueryContent queryContent,
        string tenantId,
        Func<JsonElement, T> converter,
        CancellationToken cancellationToken)
    {
        using var result = await ExecuteResourceGraphQueryAsync(queryContent, tenantId, cancellationToken);
        var results = new List<T>();
        if (result.Data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in result.Data.EnumerateArray())
            {
                results.Add(converter(item));
            }
        }

        return new ResourceQueryResults<T>(results, result.IsTruncated);
    }

    /// <summary>
    /// Resolves the tenant ID to run a query against when no subscription is available to derive it from.
    /// </summary>
    protected async Task<string> ResolveTenantIdAsync(string? tenant, CancellationToken cancellationToken)
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

    internal static RequestContent CreateResourceGraphRequestContent(ResourceQueryContent queryContent)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("query", queryContent.Query);
            WriteStringArray(writer, "subscriptions", queryContent.Subscriptions);
            WriteStringArray(writer, "managementGroups", queryContent.ManagementGroups);

            if (queryContent.Options is not null)
            {
                writer.WriteStartObject("options");
                if (queryContent.Options.Top.HasValue)
                {
                    writer.WriteNumber("$top", queryContent.Options.Top.Value);
                }
                if (queryContent.Options.Skip.HasValue)
                {
                    writer.WriteNumber("$skip", queryContent.Options.Skip.Value);
                }
                if (!string.IsNullOrEmpty(queryContent.Options.SkipToken))
                {
                    writer.WriteString("$skipToken", queryContent.Options.SkipToken);
                }
                if (queryContent.Options.ResultFormat.HasValue)
                {
                    writer.WriteString("resultFormat", queryContent.Options.ResultFormat.Value.ToString());
                }
                if (queryContent.Options.AllowPartialScopes.HasValue)
                {
                    writer.WriteBoolean("allowPartialScopes", queryContent.Options.AllowPartialScopes.Value);
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

public sealed record ResourceQueryResults<T>(List<T> Results, bool AreResultsTruncated);

/// <summary>
/// Wraps the parsed JSON response of an Azure Resource Graph query.
/// Implements <see cref="IDisposable"/> to ensure the underlying <see cref="JsonDocument"/> is disposed.
/// </summary>
public sealed class ResourceGraphQueryResult : IDisposable
{
    private readonly JsonDocument _document;

    public ResourceGraphQueryResult(JsonDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        var root = document.RootElement;

        Data = root.TryGetProperty("data", out var data) ? data : default;

        if (root.TryGetProperty("count", out var countProp) && countProp.TryGetInt32(out var count))
        {
            Count = count;
        }
        else if (Data.ValueKind == JsonValueKind.Array)
        {
            Count = Data.GetArrayLength();
        }

        if (root.TryGetProperty("totalRecords", out var totalRecordsProp) && totalRecordsProp.TryGetInt64(out var totalRecords))
        {
            TotalRecords = totalRecords;
        }

        if (root.TryGetProperty("$skipToken", out var skipTokenProp) && skipTokenProp.ValueKind == JsonValueKind.String)
        {
            SkipToken = skipTokenProp.GetString();
        }

        if (root.TryGetProperty("facets", out var facetsProp))
        {
            Facets = facetsProp;
        }

        IsTruncated = root.TryGetProperty("resultTruncated", out var resultTruncated)
            && (resultTruncated.ValueKind == JsonValueKind.True
                || (resultTruncated.ValueKind == JsonValueKind.String
                    && string.Equals(resultTruncated.GetString(), "true", StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Gets the underlying <see cref="JsonDocument"/>.
    /// </summary>
    public JsonDocument Document => _document;

    /// <summary>
    /// Gets the data element containing the query results.
    /// </summary>
    public JsonElement Data { get; }

    /// <summary>
    /// Gets the number of records returned in this page.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets the total number of records matching the query, if provided by the response.
    /// </summary>
    public long? TotalRecords { get; }

    /// <summary>
    /// Gets the continuation token for pagination, if any.
    /// </summary>
    public string? SkipToken { get; }

    /// <summary>
    /// Gets the facets element, if facets were requested.
    /// </summary>
    public JsonElement Facets { get; }

    /// <summary>
    /// Gets a value indicating whether the results were truncated.
    /// </summary>
    public bool IsTruncated { get; }

    /// <inheritdoc />
    public void Dispose() => _document.Dispose();
}
