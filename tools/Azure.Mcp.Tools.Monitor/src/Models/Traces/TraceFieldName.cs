// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

/// <summary>
/// A trace store field. <see cref="Fields"/> is populated only for nested Struct fields.
/// </summary>
public sealed class TraceFieldName
{
    public required string Name { get; init; }

    public required string DataType { get; init; }

    public List<TraceFieldName>? Fields { get; init; }
}
