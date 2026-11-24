// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Options.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Monitor.Commands.Traces;

[CommandMetadata(Id = "43a0643d-44ae-4379-a0f0-c2ff841bdbb8", Name = "analyze-compare-paths", Title = "Compare Trace Critical Paths",
    Description = """
        Compare critical-path latency between two trace populations, with independent optional time windows.
        Supply trace-ids or a filters-only query for the current population, and compare-trace-ids or
        compare-query for the comparison. Query discovery must yield complete sets of 1..1000 distinct IDs;
        capped, partial or interrupted discovery fails rather than silently sampling. Each window is at most one day.
        Uses the same generated percentile/grouping shape for both populations. Default grouping span.name,
        percentiles 50/95/99, include-samples true, resolve-trace-links false and include-rollups false.
        Returns every backend joined current/compareTo/diff row and raw coverage metadata.
        Requires resource-group, workspace, start-time and end-time. Subscription may be omitted when an Azure CLI default is available.
        Continue a running comparison with monitor traces query resume using operationId; never resubmit.
        Azure public cloud only; the caller needs workspace read access. Feature-disabled API errors surface directly.
        """,
    OperationPlane = ToolOperationPlane.Data, Destructive = false, Idempotent = true, OpenWorld = false,
    ReadOnly = true, Secret = false, LocalRequired = false)]
public sealed class TraceCompareCriticalPathCommand(
    ILogger<TraceCompareCriticalPathCommand> logger,
    IMonitorTracesService service,
    ISubscriptionResolver subscriptionResolver)
    : BaseTraceCompareCommand<TraceCompareCriticalPathOptions>(logger, service, subscriptionResolver, TraceComparisonAnalysis.CriticalPath);
