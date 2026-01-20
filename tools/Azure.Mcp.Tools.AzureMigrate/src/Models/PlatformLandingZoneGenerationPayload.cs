// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Models;

/// <summary>
/// Payload for platform landing zone generation.
/// </summary>
public sealed class PlatformLandingZoneGenerationPayload
{
    /// <summary>
    /// Gets or sets the region type.
    /// </summary>
    public string? RegionType { get; set; }

    /// <summary>
    /// Gets or sets the firewall type.
    /// </summary>
    public string? FireWallType { get; set; }

    /// <summary>
    /// Gets or sets the network architecture.
    /// </summary>
    public string? NetworkArchitecture { get; set; }

    /// <summary>
    /// Gets or sets the identity subscription ID.
    /// </summary>
    public string? IdentitySubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the management subscription ID.
    /// </summary>
    public string? ManagementSubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the connectivity subscription ID.
    /// </summary>
    public string? ConnectivitySubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the version control system.
    /// </summary>
    public string? VersionControlSystem { get; set; }

    /// <summary>
    /// Gets or sets the regions.
    /// </summary>
    public string[]? Regions { get; set; }

    /// <summary>
    /// Gets or sets the service name (environment name).
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    public string? OrganizationName { get; set; }
}
