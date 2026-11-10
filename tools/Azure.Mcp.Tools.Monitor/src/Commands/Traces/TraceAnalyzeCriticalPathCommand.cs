// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Options.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Azure.Mcp.Tools.Monitor.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Traces;

[CommandMetadata(
    Id = "5a97dc52-dc23-4f16-99d5-11e92c7f7d09",
    Name = "analyze-critical-path",
    Title = "Analyze Trace Store Distributed Trace Critical Paths",
    Description = """
        Critical-path latency breakdown for given trace IDs: which services and operations dominate
        end-to-end latency. Requires trace IDs from monitor traces query run. Returns p50/p95/p99
        critical-path durations grouped by service and operation, optional samples and per-trace
        totals; supports cross-trace links and excluding or lowering the priority of named subtrees.
        Trace Store data; workspace read access. If status is Running, use monitor traces query
        resume with the returned operationId instead of resubmitting.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TraceAnalyzeCriticalPathCommand(
    ILogger<TraceAnalyzeCriticalPathCommand> logger,
    IMonitorTracesService tracesService,
    ISubscriptionResolver subscriptionResolver)
    : BaseTraceCommand<TraceAnalyzeCriticalPathOptions, TraceQueryCommandResult>(subscriptionResolver)
{
    public override void ValidateOptions(TraceAnalyzeCriticalPathOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        TraceQueryValidator.ValidateWindow(options.StartTime, options.EndTime, validationResult);
        if (options.AggregateBy?.Any(dimension => !TraceQueryValidator.IsFieldReference(dimension)) == true)
        {
            validationResult.AddError("--aggregate-by accepts bare field references using ASCII letters, digits, underscores, and dots.", "Invalid aggregate-by.");
        }
        ValidateCriticalPathPercentiles(options.Percentiles, validationResult);
        if (options.ExcludeSubtreePrefixes?.Any(string.IsNullOrWhiteSpace) == true)
        {
            validationResult.AddError("Exclude subtree prefixes must not be empty or whitespace.", "Invalid exclude subtree prefixes.");
        }
        if (options.DeprioritizeSubtreePrefixes?.Any(string.IsNullOrWhiteSpace) == true)
        {
            validationResult.AddError("Deprioritize subtree prefixes must not be empty or whitespace.", "Invalid deprioritize subtree prefixes.");
        }
        if (options.ExcludeSubtreePrefixes is { } excludePrefixes
            && options.DeprioritizeSubtreePrefixes is { } deprioritizePrefixes
            && !excludePrefixes.Any(string.IsNullOrWhiteSpace)
            && !deprioritizePrefixes.Any(string.IsNullOrWhiteSpace)
            && excludePrefixes.Intersect(deprioritizePrefixes, StringComparer.Ordinal).Any())
        {
            validationResult.AddError("The exact same subtree prefix cannot appear in both prefix lists.", "Conflicting subtree prefixes.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, TraceAnalyzeCriticalPathOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var query = BuildCriticalPathQuery(options.AggregateBy, options.Percentiles);
            var body = new TraceQueryOptionsBody
            {
                MaxResults = Math.Clamp(options.MaxResults ?? 10, 1, 1000),
                ResolveTraceLinks = options.ResolveTraceLinks ?? false,
                IncludeSamples = options.IncludeSamples ?? true,
                IncludeRollups = options.IncludeRollups ?? false,
                ExcludeSubtreePrefixes = options.ExcludeSubtreePrefixes,
                DeprioritizeSubtreePrefixes = options.DeprioritizeSubtreePrefixes,
            };
            var result = await tracesService.CriticalPathAsync(
                options.Subscription!, options.ResourceGroup, options.Workspace, query,
                options.StartTime, options.EndTime, options.TraceIds, body,
                options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(
                new TraceQueryCommandResult(result), MonitorJsonContext.Default.TraceQueryCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error analyzing distributed trace critical paths. Subscription: {Subscription}", options.Subscription);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal static void ValidateCriticalPathPercentiles(int[]? percentiles, ValidationResult result)
    {
        if (percentiles?.Any(p => p is < 1 or > 99) == true)
        {
            result.AddError("--percentiles values must be between 1 and 99.", "Invalid percentiles.");
        }
    }

    internal static string BuildCriticalPathQuery(string[]? aggregateBy, int[]? percentiles)
    {
        var dimensions = aggregateBy is { Length: > 0 } ? aggregateBy : ["span.name"];
        var values = percentiles is { Length: > 0 } ? percentiles : [50, 95, 99];
        var clauses = values.Select(p => FormattableString.Invariant($"p{p}=percentile(duration_nano, {p})"));
        return $"Traces | summarize {string.Join(", ", clauses)} by {string.Join(", ", dimensions.Select(d => $"['{d}']"))}";
    }
}
