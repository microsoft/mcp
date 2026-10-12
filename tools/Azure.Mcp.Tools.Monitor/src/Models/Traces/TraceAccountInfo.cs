// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

/// <summary>
/// A distributed-tracing account: an Azure Monitor Workspace
/// (<c>Microsoft.Monitor/accounts</c>) that has been enabled for Trace Store by
/// creating the <c>traceContainers/default</c> sub-resource.
/// </summary>
public sealed class TraceAccountInfo
{
    public required string Name { get; init; }

    public required string ResourceId { get; init; }

    public required string ResourceGroup { get; init; }

    public string? Location { get; init; }

    /// <summary>
    /// Retention window (in seconds) configured on the trace container.
    /// </summary>
    public int? TraceDurationWindowInSeconds { get; init; }
}
