// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

internal sealed record UnusedAgeSignal(
    string Source,
    string? Query = null,
    string Field = "lastActivityAt",
    int? WindowDays = null,
    IReadOnlyList<string>? MetricNames = null,
    string Aggregation = "Total",
    double ActivityThreshold = 0);
