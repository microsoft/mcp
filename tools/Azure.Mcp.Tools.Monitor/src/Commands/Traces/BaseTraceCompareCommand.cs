// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
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

public abstract class BaseTraceCompareCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(
    ILogger logger,
    IMonitorTracesService service,
    ISubscriptionResolver subscriptionResolver,
    string analysis)
    : BaseTraceCommand<TOptions, TraceQueryCommandResult>(subscriptionResolver)
    where TOptions : class, ITraceCompareOptions
{
    public override void ValidateOptions(TOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        void Reject(string message) => validationResult.AddError(message, message);
        if (options.Workspace?.IndexOfAny(['/', '\\', '?', '#']) >= 0)
        {
            Reject("--workspace must be a workspace resource name.");
        }
        ValidateWindow(options.StartTime, options.EndTime, "current", validationResult);
        if (options.CompareStartTime is not null || options.CompareEndTime is not null)
        {
            ValidateWindow(options.CompareStartTime, options.CompareEndTime, "comparison", validationResult);
        }
        foreach (string? query in new[] { options.Query, options.CompareQuery })
        {
            if (query is not null && (string.IsNullOrWhiteSpace(query) || query.Length > 100000))
            {
                Reject("Population queries must contain 1..100000 characters.");
            }
        }
        if (analysis == TraceComparisonAnalysis.CriticalPath)
        {
            if (options is not TraceCompareCriticalPathOptions criticalPathOptions)
            {
                throw new InvalidOperationException("Critical-path comparison requires critical-path options.");
            }
            ValidateSelection(criticalPathOptions.TraceIds, options.Query, "current", validationResult);
            ValidateSelection(criticalPathOptions.CompareTraceIds, options.CompareQuery, "comparison", validationResult);
            if (criticalPathOptions.AggregateBy is { Length: > 3 }
                || criticalPathOptions.AggregateBy?.Any(field => !TraceQueryValidator.IsFieldReference(NormalizeField(field))) == true)
            {
                Reject("--aggregate-by accepts at most 3 valid field names.");
            }
            TraceQueryValidator.ValidateSubtreePrefixes(
                criticalPathOptions.ExcludeSubtreePrefixes, criticalPathOptions.DeprioritizeSubtreePrefixes,
                (message, _) => Reject(message));
            TraceAnalyzeCriticalPathCommand.ValidateCriticalPathPercentiles(criticalPathOptions.Percentiles, validationResult);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(options.Query))
            {
                Reject("--query is required.");
            }
            if (string.IsNullOrWhiteSpace(options.CompareQuery) && options.CompareStartTime is null)
            {
                Reject("Supply --compare-query or both comparison timestamps.");
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, TOptions options, CancellationToken cancellationToken)
    {
        try
        {
            bool criticalPath = analysis == TraceComparisonAnalysis.CriticalPath;
            TraceCompareCriticalPathOptions? criticalPathOptions = options as TraceCompareCriticalPathOptions;
            if (criticalPath != (criticalPathOptions is not null))
            {
                throw new InvalidOperationException("Comparison options do not match the selected analysis.");
            }
            var request = new TraceComparisonRequest
            {
                Current = new()
                {
                    Query = options.Query,
                    Language = "KQL",
                    TimeRange = Window(options.StartTime, options.EndTime),
                    TraceId = criticalPathOptions?.TraceIds is null ? null : string.Join(",", criticalPathOptions.TraceIds.Select(id => Guid.Parse(id.Trim()).ToString("N"))),
                    Options = new()
                    {
                        MaxResults = Math.Clamp(options.MaxResults ?? 10000, 1, 10000),
                        ResolveTraceLinks = criticalPath ? criticalPathOptions!.ResolveTraceLinks ?? false : null,
                        IncludeSamples = criticalPath ? criticalPathOptions!.IncludeSamples ?? true : null,
                        IncludeRollups = criticalPath ? criticalPathOptions!.IncludeRollups ?? false : null,
                        ExcludeSubtreePrefixes = criticalPathOptions?.ExcludeSubtreePrefixes,
                        DeprioritizeSubtreePrefixes = criticalPathOptions?.DeprioritizeSubtreePrefixes,
                    },
                },
                CompareTo = new()
                {
                    Query = options.CompareQuery,
                    TimeRange = options.CompareStartTime is null ? null : Window(options.CompareStartTime, options.CompareEndTime!),
                    TraceId = criticalPathOptions?.CompareTraceIds is null ? null : string.Join(",", criticalPathOptions.CompareTraceIds.Select(id => Guid.Parse(id.Trim()).ToString("N"))),
                },
            };
            string? aggregation = criticalPath
                ? TraceAnalyzeCriticalPathCommand.BuildCriticalPathQuery(criticalPathOptions!.AggregateBy?.Select(NormalizeField).ToArray(), criticalPathOptions.Percentiles)
                : null;
            TraceQueryResult result = await service.ComparePopulationsAsync(
                options.Subscription!, options.ResourceGroup, options.Workspace, analysis, request,
                aggregation, options.Tenant, cancellationToken);
            context.Response.Results = ResponseResult.Create(new TraceQueryCommandResult(result),
                MonitorJsonContext.Default.TraceQueryCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error comparing trace populations. Analysis: {Analysis}, Subscription: {Subscription}", analysis, options.Subscription);
            HandleException(context, ex);
        }
        return context.Response;
    }

    private static TraceQueryTimeRange Window(string start, string end) => new()
    {
        StartTime = MonitorTracesService.FormatUtc(ParseTime(start)),
        EndTime = MonitorTracesService.FormatUtc(ParseTime(end)),
    };

    private static DateTimeOffset ParseTime(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    private static void ValidateWindow(string? start, string? end, string population, ValidationResult validation)
    {
        if (!DateTimeOffset.TryParse(start, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var startTime)
            || !DateTimeOffset.TryParse(end, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var endTime))
        {
            string message = $"The {population} window requires both ISO 8601 timestamps.";
            validation.AddError(message, message);
            return;
        }
        var windowValidation = new ValidationResult();
        TraceQueryValidator.ValidateWindow(startTime, endTime, windowValidation);
        foreach (string error in windowValidation.Errors)
        {
            validation.AddError($"The {population} window: {error}", error);
        }
    }

    private static void ValidateSelection(string[]? ids, string? query, string population, ValidationResult validation)
    {
        string? error = null;
        if ((ids is null) == (query is null))
        {
            error = $"Supply trace IDs or a filter query for the {population} population, not both.";
        }
        else if (ids is not null && (ids.Length is < 1 or > 1000 || ids.Any(id =>
            !Guid.TryParse(id.Trim(), out _) && (id.Trim().Length != 32 || !id.Trim().All(Uri.IsHexDigit)))))
        {
            error = $"The {population} population requires 1..1000 trace IDs (32 hexadecimal characters or GUIDs).";
        }
        else if (query is not null)
        {
            try
            {
                string structure = LogSearchQueryValidator.BlankQuotedTextAndRejectComments(query);
                string[] sections = structure.Split("$$", 2, StringSplitOptions.None);
                string? spanBlock = sections.Length == 2 ? sections[1].Trim() : null;
                if (!Regex.IsMatch(sections[0], @"^\s*(?:Traces|T)\s*(?:\||$)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1))
                    || Regex.Matches(structure, @"\|\s*(\w+)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1)).Any(match => match.Groups[1].Value != "where")
                    || (spanBlock is not null && (!spanBlock.StartsWith("<<", StringComparison.Ordinal)
                        || spanBlock.IndexOf("<<", 2, StringComparison.Ordinal) >= 0
                        || spanBlock.IndexOf(">>", StringComparison.Ordinal) != spanBlock.Length - 2)))
                {
                    error = $"The {population} selection query must start with Traces or T and contain only where filters and optionally one $$ span-membership block.";
                }
            }
            catch (CommandValidationException ex)
            {
                error = $"Invalid {population} selection query: {ex.Message}";
            }
        }
        if (error is not null)
        {
            validation.AddError(error, error);
        }
    }

    private static string NormalizeField(string field)
    {
        string value = field.Trim();
        return value.StartsWith("['", StringComparison.Ordinal) && value.EndsWith("']", StringComparison.Ordinal) ? value[2..^2] : value;
    }
}
