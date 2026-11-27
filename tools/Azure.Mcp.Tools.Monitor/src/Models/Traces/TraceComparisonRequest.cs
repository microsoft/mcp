// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

public sealed class TraceComparisonRequest()
{
    public string? TraceAccountId { get; set; }

    public required TraceComparisonPopulation Current { get; init; }

    public required TraceComparisonPopulation CompareTo { get; set; }
}
