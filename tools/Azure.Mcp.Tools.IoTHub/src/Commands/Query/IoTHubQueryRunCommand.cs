// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Mcp.Core.Commands.Subscription;
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
        Run an IoT Hub query against the device registry and return a single page of results.
        Provide a raw SQL-like query with --query, OR structured predicates with --filters (each has scope/field/operator/value) that are compiled into the query for you; supply only one of the two.
        When neither is provided, a bare 'SELECT * FROM devices' runs and the response additionally returns a 'discoveredFields' catalog: queryable field paths grouped by device, tags, desired, and reported, with observed types and example values. Forward that catalog to a later --filters call via --discovered-fields so unknown fields are rejected before the query runs.
        Prefer projecting only the specific property fields you need; avoid raw 'SELECT *' unless you want full device twins or the field catalog.
        Use --max-count to set the page size (default 100, maximum 100). Values greater than 100 are capped at 100, so one page is always at most 100 items.
        Never make repeated calls or loop for additional pages in a single user request. Return exactly one page and, when hasMore is true, include the continuationToken for a later explicit next-page request.
        The --continuation-token input must be the opaque continuationToken string returned by a previous iothub_query_run response; do not pass hasMore=true/false or any boolean value.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed partial class IoTHubQueryRunCommand(
    ILogger<IoTHubQueryRunCommand> logger,
    IIoTHubDeviceService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IoTHubQueryRunOptions, IoTHubQueryRunResult>(subscriptionResolver)
{
    private const string DefaultQuery = "SELECT * FROM devices";
    private const int DefaultMaxCount = IoTHubQueryLimits.MaxPageSize;
    private const int MinMaxCount = 1;
    private const int MaxMaxCount = IoTHubQueryLimits.MaxPageSize;

    private readonly ILogger<IoTHubQueryRunCommand> _logger = logger;
    private readonly IIoTHubDeviceService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubQueryRunOptions options,
        CancellationToken cancellationToken)
    {
        var maxCount = options.MaxCount switch
        {
            null => DefaultMaxCount,
            > MaxMaxCount => MaxMaxCount,
            _ => options.MaxCount.Value
        };

        if (maxCount < MinMaxCount)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = $"The entered max-count '{maxCount}' is less than 1 item. Please specify a value of at least {MinMaxCount}.";
            return context.Response;
        }

        if (IsBooleanContinuationToken(options.ContinuationToken))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "The continuation-token value must be the opaque continuationToken string returned by a previous iothub_query_run response, not hasMore=true/false. Omit --continuation-token to fetch the first page.";
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
            if (!TryCompileFilters(context, options, out effectiveQuery))
            {
                return context.Response;
            }
        }
        else
        {
            effectiveQuery = DefaultQuery;
        }

        try
        {
            var page = await _service.RunQuery(
                effectiveQuery,
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                maxCount,
                options.ContinuationToken,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var hasMore = !string.IsNullOrEmpty(page.ContinuationToken);
            var message = hasMore
                ? $"Showing {page.Items.Count} results. More results are available; return this page now and use the continuationToken only on a later explicit next-page request."
                : $"Showing {page.Items.Count} results. No more results are available.";

            // On a bare 'SELECT * FROM <source>' (no WHERE clause) also surface a compact catalog of
            // queryable field paths so callers can build validated --filters without a separate discover tool.
            var discoveredFields = IsUnfilteredSelectStar(effectiveQuery)
                ? IoTHubQueryFieldDiscoverer.Discover(page.Items)
                : null;

            var result = new IoTHubQueryRunResult(
                page.Items,
                page.Items.Count,
                hasMore,
                page.ContinuationToken,
                message,
                discoveredFields);

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

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        TimeoutException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    private static bool IsBooleanContinuationToken(string? continuationToken)
    {
        var normalizedToken = continuationToken?.Trim();
        return string.Equals(normalizedToken, bool.TrueString, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedToken, bool.FalseString, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryCompileFilters(CommandContext context, IoTHubQueryRunOptions options, out string query)
    {
        query = string.Empty;

        List<QueryPredicate>? filters;
        try
        {
            filters = JsonSerializer.Deserialize(options.Filters!, IoTHubJsonContext.Default.ListQueryPredicate);
        }
        catch (JsonException ex)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = $"The --filters value is not valid JSON: {ex.Message}";
            return false;
        }

        if (filters is null || filters.Count == 0)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "The --filters value must be a non-empty JSON array of predicate objects.";
            return false;
        }

        QueryDiscoveredFields? discoveredFields = null;
        if (!string.IsNullOrWhiteSpace(options.DiscoveredFields))
        {
            try
            {
                discoveredFields = JsonSerializer.Deserialize(options.DiscoveredFields, IoTHubJsonContext.Default.QueryDiscoveredFields);
            }
            catch (JsonException ex)
            {
                context.Response.Status = HttpStatusCode.BadRequest;
                context.Response.Message = $"The --discovered-fields value is not valid JSON: {ex.Message}";
                return false;
            }
        }

        var request = new QueryCompileRequest
        {
            Filters = filters,
            From = string.IsNullOrWhiteSpace(options.From) ? "devices" : options.From!,
            LogicalOperator = string.IsNullOrWhiteSpace(options.LogicalOperator) ? "AND" : options.LogicalOperator!,
            DiscoveredFields = discoveredFields
        };

        try
        {
            query = IoTHubQueryCompiler.Compile(request);
        }
        catch (ArgumentException ex)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = ex.Message;
            return false;
        }

        return true;
    }

    private static bool IsUnfilteredSelectStar(string query) =>
        UnfilteredSelectStarRegex().IsMatch(query.Trim());

    [GeneratedRegex(@"^SELECT\s+\*\s+FROM\s+[A-Za-z0-9_.]+\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex UnfilteredSelectStarRegex();
}
