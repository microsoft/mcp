// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options.Query;
using Azure.Mcp.Tools.IoTHub.Query;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTHub.Commands.Query;

[CommandMetadata(
    Id = "2d8e60b2-87e5-4e2d-ac91-615e02e2ac21",
    Name = "run",
    Title = "Run IoT Hub Query",
    Description = """
        Run an IoT Hub query against the device registry and return the matching results. The tool pages through IoT Hub internally and aggregates every page, so a single call returns the full result set - the caller never has to follow a continuation token or make repeated next-page requests. Choose this tool whenever the request is phrased as a query or filter over devices - for example 'query the devices', 'query all devices', 'find devices where <field> <op> <value>', or a raw IoT Hub SQL statement. For a plain 'list/show the registered devices' request with no query wording, use the iothub device list command instead.
        Provide a raw SQL-like query with --query, OR structured predicates with --filters (each has scope/field/operator/value) that are compiled into the query for you; supply only one of the two.
        When --filters is used, the tool first samples the twin registry to discover which fields exist, validates every predicate field against them, and fails with an error if a referenced tag or property is not found; it then compiles a valid query and runs it.
        When neither --query nor --filters is provided, a bare 'SELECT * FROM devices' runs. Prefer projecting only the specific property fields you need; avoid raw 'SELECT *' unless you want full device twins.
        Use --max-count to cap the total number of items returned across all pages; omit it to return every matching item. If the query matches more items than the cap, the tool returns an error stating the max-count limit was hit (the partial items are still included) - raise --max-count, narrow the query, or omit --max-count to get a complete result.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubQueryRunCommand(
    ILogger<IoTHubQueryRunCommand> logger,
    IIoTHubDeviceService service,
    ISubscriptionResolver subscriptionResolver)
    : BaseIoTHubCommand<IoTHubQueryRunOptions, IoTHubQueryRunResult>(subscriptionResolver)
{
    private const string DefaultQuery = "SELECT * FROM devices";
    private const int MinMaxCount = 1;
    private const int PageSize = IoTHubQueryLimits.MaxPageSize;

    private readonly ILogger<IoTHubQueryRunCommand> _logger = logger;
    private readonly IIoTHubDeviceService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubQueryRunOptions options,
        CancellationToken cancellationToken)
    {
        var maxCount = options.MaxCount;
        if (maxCount is < MinMaxCount)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = $"The entered max-count '{maxCount}' is less than 1 item. Please specify a value of at least {MinMaxCount}, or omit it to return every matching item.";
            return context.Response;
        }

        var hasQuery = !string.IsNullOrWhiteSpace(options.Query);
        var hasFilters = !string.IsNullOrWhiteSpace(options.Filters);

        if (hasQuery && hasFilters)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "Provide either --query or --filters, not both. Use --query for raw IoT Hub SQL, or --filters to compile structured predicates.";
            return context.Response;
        }

        string effectiveQuery;
        if (hasQuery)
        {
            effectiveQuery = options.Query!;
        }
        else if (hasFilters)
        {
            var compiledQuery = await DiscoverCompileFiltersAsync(context, options, cancellationToken);
            if (compiledQuery is null)
            {
                return context.Response;
            }

            effectiveQuery = compiledQuery;
        }
        else
        {
            effectiveQuery = DefaultQuery;
        }

        try
        {
            var items = new List<JsonElement>();
            var seenIdentities = new HashSet<string>(StringComparer.Ordinal);
            string? continuationToken = null;
            var truncated = false;
            do
            {
                var pageSize = maxCount.HasValue
                    ? Math.Min(maxCount.Value - items.Count, PageSize)
                    : PageSize;

                var page = await _service.RunQuery(
                    effectiveQuery,
                    options.HubName,
                    options.ResourceGroup,
                    options.Subscription!,
                    pageSize,
                    continuationToken,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                foreach (var item in page.Items)
                {
                    var identity = TryGetResultIdentity(item);
                    if (identity is null || seenIdentities.Add(identity))
                    {
                        items.Add(item);
                    }
                }

                continuationToken = page.ContinuationToken;

                if (maxCount.HasValue && items.Count >= maxCount.Value)
                {
                    truncated = !string.IsNullOrEmpty(continuationToken);
                    break;
                }
            }
            while (!string.IsNullOrEmpty(continuationToken));

            var message = truncated
                ? $"The max-count limit of {maxCount} was reached before all matching items were returned, so the results are incomplete. Raise --max-count to return more items, narrow the query, or omit --max-count to return every matching item."
                : $"Showing all {items.Count} results.";

            // A hit cap means the query matched more items than the caller allowed, so surface it as an
            // error (the partial items are still included) instead of silently returning a truncated set.
            if (truncated)
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = message;
            }

            var result = new IoTHubQueryRunResult(
                items,
                items.Count,
                truncated,
                message);

            context.Response.Results = ResponseResult.Create(
                result,
                IoTHubJsonContext.Default.IoTHubQueryRunResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running query against IoT Hub '{HubName}'.", options.HubName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Extracts a stable identity for a query result row so that duplicates produced by IoT Hub's
    // unordered registry paging can be removed. Covers the documented query sources: devices
    // (deviceId), devices.modules (deviceId + moduleId) and devices.jobs (jobId). Returns null when
    // no identity field is projected, in which case the row cannot be safely de-duplicated and is
    // kept as-is.
    private static string? TryGetResultIdentity(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (TryGetStringProperty(element, "deviceId", out var deviceId))
        {
            return TryGetStringProperty(element, "moduleId", out var moduleId)
                ? $"d:{deviceId}\u0000m:{moduleId}"
                : $"d:{deviceId}";
        }

        if (TryGetStringProperty(element, "jobId", out var jobId))
        {
            return $"j:{jobId}";
        }

        return null;
    }

    private static bool TryGetStringProperty(JsonElement element, string propertyName, out string? value)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString();
            return !string.IsNullOrEmpty(value);
        }

        value = null;
        return false;
    }

    // Filters pipeline: sample the twin registry to discover which fields exist, validate the predicates
    // against them (failing on any unknown tag/property), then compile a valid query. Returns the compiled
    // query, or null when the request was rejected (context.Response is populated with the reason).
    private async Task<string?> DiscoverCompileFiltersAsync(
        CommandContext context,
        IoTHubQueryRunOptions options,
        CancellationToken cancellationToken)
    {
        List<QueryPredicate>? filters;
        try
        {
            filters = JsonSerializer.Deserialize(options.Filters!, IoTHubJsonContext.Default.ListQueryPredicate);
        }
        catch (JsonException ex)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = $"The --filters value is not valid JSON: {ex.Message}";
            return null;
        }

        if (filters is null || filters.Count == 0)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "The --filters value must be a non-empty JSON array of predicate objects.";
            return null;
        }

        var source = string.IsNullOrWhiteSpace(options.From) ? "devices" : options.From!;

        QueryDiscoveredFields discoveredFields;
        try
        {
            discoveredFields = await IoTHubQueryDiscovery.DiscoverFieldsAsync(
                _service,
                source,
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering fields for IoT Hub '{HubName}'.", options.HubName);
            HandleException(context, ex);
            return null;
        }

        var request = new QueryCompileRequest
        {
            Filters = filters,
            From = source,
            LogicalOperator = string.IsNullOrWhiteSpace(options.LogicalOperator) ? "AND" : options.LogicalOperator!,
            DiscoveredFields = discoveredFields
        };

        try
        {
            return IoTHubQueryCompiler.Compile(request);
        }
        catch (ArgumentException ex)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = ex.Message;
            return null;
        }
    }
}
