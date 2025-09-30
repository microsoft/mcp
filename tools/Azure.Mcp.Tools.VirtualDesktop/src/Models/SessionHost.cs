// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.VirtualDesktop.Models;

public sealed class SessionHost
{
    public string? Name { get; set; }
    public string? ResourceGroupName { get; set; }
    public string? SubscriptionId { get; set; }
    public string? HostPoolName { get; set; }
    public string? Status { get; set; }
    public int? Sessions { get; set; }
    public string? AgentVersion { get; set; }
    public bool? AllowNewSession { get; set; }
    public string? AssignedUser { get; set; }
    public string? FriendlyName { get; set; }
    public string? OsVersion { get; set; }
    public string? UpdateState { get; set; }
    public string? UpdateErrorMessage { get; set; }
    public IList<SessionHostHealthCheckResult>? SessionHostHealthCheckResults { get; set; }
}
