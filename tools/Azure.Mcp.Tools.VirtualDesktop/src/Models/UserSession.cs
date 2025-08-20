// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.DesktopVirtualization;

namespace Azure.Mcp.Tools.VirtualDesktop.Models;

public class UserSession
{
    public UserSession(UserSessionResource resource)
    {
        Name = resource.Data.Name;
        Id = resource.Data.Id?.ToString();
        ResourceGroupName = resource.Id.ResourceGroupName!;
        SubscriptionId = resource.Id.SubscriptionId!;
        HostPoolName = resource.Id.Parent?.Parent?.Name ?? string.Empty;
        SessionHostName = resource.Id.Parent?.Name ?? string.Empty;
        UserPrincipalName = resource.Data.UserPrincipalName;
        ApplicationType = resource.Data.ApplicationType?.ToString();
        SessionState = resource.Data.SessionState?.ToString();
        ActiveDirectoryUserName = resource.Data.ActiveDirectoryUserName;
        CreateTime = resource.Data.CreateOn;
    }

    /// <summary> Default constructor for serialization. </summary>
    public UserSession() { }

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
