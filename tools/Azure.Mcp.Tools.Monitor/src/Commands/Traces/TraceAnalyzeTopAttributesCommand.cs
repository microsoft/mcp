// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.RegularExpressions;
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
    Id = "86c3b5bc-4031-4254-a0eb-dc9c4a522ffc",
    Name = "analyze-top-attributes",
    Title = "Find Top Distinguishing Trace Store Attributes",
    Description = """
        Compare two trace cohorts to rank the attribute values that distinguish a selected cohort
        (field/operator/value within the selection window) from the baseline (the rest of the overall
        query and window); use it to explain latency, error, retry, or deployment differences.
        Returns ranked attributes with per-value cohort counts, not causal proof. Trace Store data;
        workspace read access. If status is Running, use monitor traces query resume with the
        returned operationId instead of resubmitting.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TraceAnalyzeTopAttributesCommand(
    ILogger<TraceAnalyzeTopAttributesCommand> logger,
    IMonitorTracesService tracesService,
    ISubscriptionResolver subscriptionResolver)
    : BaseTraceCommand<TraceAnalyzeTopAttributesOptions, TraceQueryCommandResult>(subscriptionResolver)
{
    public override void ValidateOptions(TraceAnalyzeTopAttributesOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        TraceQueryValidator.ValidateWindow(options.OverallStartTime, options.OverallEndTime, validationResult);
        TraceQueryValidator.ValidateWindow(options.SelectionStartTime, options.SelectionEndTime, validationResult);
        if (options.SelectionStartTime < options.OverallStartTime || options.SelectionEndTime > options.OverallEndTime)
        {
            validationResult.AddError("The selection window must be contained within the overall window.", "Invalid selection window.");
        }
        if (!IsIdentifier(options.SelectionField))
        {
            validationResult.AddError("--selection-field must be a bare identifier using letters, digits, underscores, and dots.", "Invalid selection-field.");
        }
        if (options.SelectionOp is not (">" or ">=" or "<" or "<=" or "==" or "!="))
        {
            validationResult.AddError("--selection-op must be >, >=, <, <=, ==, or !=.", "Invalid selection-op.");
        }
        if (!IsLiteral(options.SelectionValue))
        {
            validationResult.AddError("--selection-value must be a finite number or a single-quoted string literal.", "Invalid selection-value.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, TraceAnalyzeTopAttributesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var query = string.IsNullOrWhiteSpace(options.OverallQuery) ? "Traces" : options.OverallQuery.Trim();
            // A standalone span filter needs a trace-level pipeline for the compare stage.
            if (query.StartsWith("<<", StringComparison.Ordinal))
            {
                query = "Traces $$ " + query;
            }
            query += FormattableString.Invariant(
                $" | compare({options.SelectionField} {options.SelectionOp} {options.SelectionValue}, {options.TopN ?? 8}, {options.SelectionStartTime.UtcDateTime:O}, {options.SelectionEndTime.UtcDateTime:O})");
            var result = await tracesService.CompareAsync(
                options.Subscription!, options.ResourceGroup, options.Workspace, query,
                options.OverallStartTime, options.OverallEndTime, Math.Clamp(options.MaxResults ?? 10000, 1, 10000),
                options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(
                new TraceQueryCommandResult(result), MonitorJsonContext.Default.TraceQueryCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error comparing distributed traces. Subscription: {Subscription}", options.Subscription);
            HandleException(context, ex);
        }
        return context.Response;
    }

    private static bool IsIdentifier(string? value) =>
        value is { Length: > 0 and <= 1024 } &&
        Regex.IsMatch(value, @"\A[a-zA-Z_][a-zA-Z0-9_.]*\z", RegexOptions.NonBacktracking);

    private static bool IsLiteral(string? value) =>
        value is { Length: > 0 and <= 4096 } &&
        ((double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) && double.IsFinite(number))
        || Regex.IsMatch(value, @"\A'(?:[^'\\]|''|\\['\\])*'\z", RegexOptions.NonBacktracking));
}
