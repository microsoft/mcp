// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.VirtualDesktop.Models;

public sealed class UserSession
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? ResourceGroupName { get; set; }
    public string? SubscriptionId { get; set; }
    public string? HostPoolName { get; set; }
    public string? SessionHostName { get; set; }
    public string? UserPrincipalName { get; set; }
    public string? ApplicationType { get; set; }
    public string? SessionState { get; set; }
    public string? ActiveDirectoryUserName { get; set; }
    public DateTimeOffset? CreateTime { get; set; }
}
