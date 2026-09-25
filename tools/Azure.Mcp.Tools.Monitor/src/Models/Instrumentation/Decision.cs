// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Instrumentation;

public record Decision
{
    public string Intent { get; init; } = null!;  // "onboard" | "migrate"
    public string TargetApproach { get; init; } = null!;
    public string Rationale { get; init; } = null!;
}
