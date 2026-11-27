// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

/// <summary>
/// Response of <c>GET {amwEndpoint}/field-names</c>, grouped into trace and span fields.
/// </summary>
public sealed class TraceFieldNames
{
    public required List<TraceFieldName> TraceFields { get; init; }

    public required List<TraceFieldName> SpanFields { get; init; }
}
