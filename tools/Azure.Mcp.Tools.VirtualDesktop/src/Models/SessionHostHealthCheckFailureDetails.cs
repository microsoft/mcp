// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.VirtualDesktop.Models;

public sealed class SessionHostHealthCheckFailureDetails
{
    public string? Message { get; set; }
    public int? ErrorCode { get; set; }
    public DateTimeOffset? LastHealthCheckOn { get; set; }
}
