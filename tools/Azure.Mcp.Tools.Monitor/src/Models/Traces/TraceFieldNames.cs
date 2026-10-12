// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

/// <summary>
/// A single field exposed by the trace store. <see cref="Fields"/> is populated only for
/// nested Struct fields (e.g. <c>attributes</c>) and is otherwise null.
/// </summary>
public sealed class TraceFieldName
{
    public required string Name { get; init; }

    public required string DataType { get; init; }

    public List<TraceFieldName>? Fields { get; init; }
}

/// <summary>
/// Response of <c>GET {amwEndpoint}/field-names</c>. Buckets fields by whether they live
/// on the trace row (<see cref="TraceFields"/>) or on individual spans within a trace
/// (<see cref="SpanFields"/>). Callers use this to compose valid KQL queries.
/// </summary>
public sealed class TraceFieldNames
{
    public required List<TraceFieldName> TraceFields { get; init; }

    public required List<TraceFieldName> SpanFields { get; init; }
}
