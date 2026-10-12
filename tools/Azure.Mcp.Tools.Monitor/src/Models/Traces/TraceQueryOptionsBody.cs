// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

internal sealed class TraceQueryOptionsBody
{
    public required int MaxResults { get; init; }
}
