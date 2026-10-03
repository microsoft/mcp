// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Adme.Models;

public sealed class AdmeResponse<T>(T result, string? correlationId)
{
    public T Result { get; } = result;

    public string? CorrelationId { get; } = correlationId;
}
