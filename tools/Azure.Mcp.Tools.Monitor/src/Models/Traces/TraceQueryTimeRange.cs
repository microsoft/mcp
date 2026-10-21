// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

internal sealed class TraceQueryTimeRange
{
    public required string StartTime { get; init; }

    public required string EndTime { get; init; }
}
