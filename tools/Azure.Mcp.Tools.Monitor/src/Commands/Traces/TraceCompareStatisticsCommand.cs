// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Options.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Monitor.Commands.Traces;

[CommandMetadata(Id = "785fb343-54c0-42f1-846a-d4643f649920", Name = "compare-statistics", Title = "Compare Trace Statistics",
    Description = """
        Compare statistics for two independent distributed-trace queries and/or time windows.
        Use matching named aggregations and grouping keys in both queries; the backend validates shape compatibility.
        Returns joined current/compareTo/diff rows and coverage metadata. max-results limits each
        population's aggregation rows, not its input trace count, before the join.
        Unlike selection-based attribute comparison, the populations can have disjoint time windows.
        Requires resource-group, workspace, query, start-time and end-time, plus compare-query
        or both comparison timestamps. Subscription may be omitted when an Azure CLI default is available.
        Omitted comparison fields inherit the current population.
        A running result includes an operationId; continue with monitor traces query resume, never resubmit.
        Azure public cloud only; the caller needs workspace read access. Feature-disabled API errors surface directly.
        """,
    OperationPlane = ToolOperationPlane.Data, Destructive = false, Idempotent = true, OpenWorld = false,
    ReadOnly = true, Secret = false, LocalRequired = false)]
public sealed class TraceCompareStatisticsCommand(
    ILogger<TraceCompareStatisticsCommand> logger,
    IMonitorTracesService service,
    ISubscriptionResolver subscriptionResolver)
    : BaseTraceCompareCommand<TraceCompareOptions>(logger, service, subscriptionResolver, TraceComparisonAnalysis.Statistics);
