// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Instrumentation;

public sealed record ImplementationTemplate
{
    public required string ClassName { get; init; }
    public required string File { get; init; }
    public required string Purpose { get; init; }
}
