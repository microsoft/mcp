// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.VirtualDesktop.Models;

public sealed class SessionHostHealthCheckResult
{
    public string? HealthCheckName { get; set; }
    public string? HealthCheckResult { get; set; }
    public SessionHostHealthCheckFailureDetails? AdditionalFailureDetails { get; set; }
}
