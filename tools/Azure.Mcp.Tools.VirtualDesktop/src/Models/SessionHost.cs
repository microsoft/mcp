// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.DesktopVirtualization;

namespace Azure.Mcp.Tools.VirtualDesktop.Models;

public class SessionHost
{
    public SessionHost(SessionHostResource resource)
    {
        Name = resource.Data.Name;
        ResourceGroupName = resource.Id.ResourceGroupName!;
        SubscriptionId = resource.Id.SubscriptionId!;
        HostPoolName = resource.Id.Parent?.Name ?? string.Empty;
        Status = resource.Data.Status?.ToString();
        Sessions = resource.Data.Sessions;
        AgentVersion = resource.Data.AgentVersion;
        AllowNewSession = resource.Data.AllowNewSession;
        AssignedUser = resource.Data.AssignedUser;
        FriendlyName = resource.Data.FriendlyName;
        OsVersion = resource.Data.OSVersion;
        UpdateState = resource.Data.UpdateState?.ToString();
        UpdateErrorMessage = resource.Data.UpdateErrorMessage;
        SessionHostHealthCheckResults = resource.Data.SessionHostHealthCheckResults?
            .Select(report => new SessionHostHealthCheckResult(report))
            .ToList();
    }

    /// <summary> Default constructor for serialization. </summary>
    public SessionHost() { }

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
