// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Options.Traces;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Traces;

[CommandMetadata(
    Id = "b9d54c74-b9f8-4c93-9f2c-e21b31a9c72e",
    Name = "run",
    Title = "Run Distributed Trace Query",
    Description = """
        Run a KQL query on a distributed-trace (Trace Store) account to find, filter, and
        analyze spans, traces, service latency, request errors, top-slowest operations, and
        per-service telemetry from OpenTelemetry-instrumented applications. The trace
        account is an Azure Monitor Workspace enabled for Trace Store. Typical prompts:
        "show traces from service X over the last N minutes", "top slowest traces for
        service X", "find traces with errors in the last hour", "trace by trace-id /
        span-id". The command submits the
        query, polls until it completes (~180s budget), and returns the first page of rows.
        If the poll budget is exhausted it returns `status: "Running"` with an operationId
        — pass `--operation-id <id>` to this same command to resume polling and fetch the
        results. Queries run with the caller's Azure identity — the caller must have read
        access on the Azure Monitor Workspace.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TraceQueryRunCommand(
    ILogger<TraceQueryRunCommand> logger,
    IMonitorTracesService tracesService,
    ISubscriptionResolver subscriptionResolver)
    : BaseTraceCommand<TraceQueryRunOptions, TraceQueryRunCommand.TraceQueryRunCommandResult>(subscriptionResolver)
{
    private const int DefaultMaxResults = 100;
    private const int MaxAllowedResults = 1000;
    private static readonly TimeSpan DefaultTimeRange = TimeSpan.FromMinutes(30);

    public override void ValidateOptions(TraceQueryRunOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        bool hasQuery = !string.IsNullOrWhiteSpace(options.Query);
        bool hasOperationId = !string.IsNullOrWhiteSpace(options.OperationId);
        if (hasQuery == hasOperationId)
        {
            validationResult.AddError(
                "Exactly one of --query (new query) or --operation-id (resume) is required.",
                "Invalid trace query operation selection.");
        }

        if (hasQuery)
        {
            bool validStart = ValidateTimestamp(options.StartTime, "--start-time", validationResult);
            bool validEnd = ValidateTimestamp(options.EndTime, "--end-time", validationResult);
            if (validStart && validEnd)
            {
                DateTimeOffset endTime = ParseUtc(options.EndTime) ?? DateTimeOffset.UtcNow;
                DateTimeOffset startTime = ParseUtc(options.StartTime) ?? endTime - DefaultTimeRange;
                if (startTime >= endTime || endTime - startTime > TimeSpan.FromDays(1))
                {
                    validationResult.AddError(
                        "The query time range must be positive and no longer than one day.",
                        "Invalid trace query time range.");
                }
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        TraceQueryRunOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var maxResults = Math.Clamp(options.MaxResults ?? DefaultMaxResults, 1, MaxAllowedResults);
            var includeSpans = options.IncludeSpans ?? true;
            TraceQueryResult result;
            if (!string.IsNullOrWhiteSpace(options.OperationId))
            {
                result = await tracesService.ResumeQueryAsync(
                    options.Subscription!, options.ResourceGroup, options.Workspace, options.OperationId,
                    maxResults, includeSpans, options.Tenant, cancellationToken);
            }
            else
            {
                DateTimeOffset endTime = ParseUtc(options.EndTime) ?? DateTimeOffset.UtcNow;
                DateTimeOffset startTime = ParseUtc(options.StartTime) ?? endTime - DefaultTimeRange;
                result = await tracesService.RunQueryAsync(
                    options.Subscription!, options.ResourceGroup, options.Workspace, options.Query!,
                    startTime, endTime, maxResults, includeSpans, options.Tenant, cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(
                new TraceQueryRunCommandResult(result),
                MonitorJsonContext.Default.TraceQueryRunCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running trace query. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Workspace: {Workspace}",
                options.Subscription, options.ResourceGroup, options.Workspace);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static bool ValidateTimestamp(string? value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out _))
        {
            validationResult.AddError(
                $"{optionName} must be a UTC ISO 8601 timestamp (for example 2026-07-24T20:00:00Z). Got '{value}'.",
                $"Invalid trace query {optionName}.");
            return false;
        }

        return true;
    }

    private static DateTimeOffset? ParseUtc(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null :
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    public sealed record TraceQueryRunCommandResult(TraceQueryResult Result);
}
