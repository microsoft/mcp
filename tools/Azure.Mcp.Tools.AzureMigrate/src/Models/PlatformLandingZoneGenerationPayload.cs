// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Models;

/// <summary>
/// Payload for platform landing zone generation.
/// </summary>
public record PlatformLandingZoneGenerationPayload
{
    /// <summary>
    /// Gets or initializes the region type.
    /// </summary>
    public string? RegionType { get; init; }

    /// <summary>
    /// Gets or initializes the firewall type.
    /// </summary>
    public string? FireWallType { get; init; }

    /// <summary>
    /// Gets or initializes the network architecture.
    /// </summary>
    public string? NetworkArchitecture { get; init; }

    /// <summary>
    /// Gets or initializes the identity subscription ID.
    /// </summary>
    public string? IdentitySubscriptionId { get; init; }

    /// <summary>
    /// Gets or initializes the management subscription ID.
    /// </summary>
    public string? ManagementSubscriptionId { get; init; }

    /// <summary>
    /// Gets or initializes the connectivity subscription ID.
    /// </summary>
    public string? ConnectivitySubscriptionId { get; init; }

    /// <summary>
    /// Gets or initializes the version control system.
    /// </summary>
    public string? VersionControlSystem { get; init; }

    /// <summary>
    /// Gets or initializes the regions.
    /// </summary>
    public string[]? Regions { get; init; }

    /// <summary>
    /// Gets or initializes the service name (environment name).
    /// </summary>
    public string? ServiceName { get; init; }

    /// <summary>
    /// Gets or initializes the organization name.
    /// </summary>
    public string? OrganizationName { get; init; }
}
