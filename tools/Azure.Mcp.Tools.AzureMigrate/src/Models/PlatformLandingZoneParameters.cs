// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureMigrate.Models;

/// <summary>
/// Represents the parameters for platform landing zone generation.
/// </summary>
public record PlatformLandingZoneParameters
{
    /// <summary>
    /// Gets or initializes the region type (single or multi).
    /// </summary>
    [JsonPropertyName("regionType")]
    public string? RegionType { get; init; }

    /// <summary>
    /// Gets or initializes the firewall type (azurefirewall, nva, or none).
    /// </summary>
    [JsonPropertyName("fireWallType")]
    public string? FireWallType { get; init; }

    /// <summary>
    /// Gets or initializes the network architecture (hubspoke or vwan).
    /// </summary>
    [JsonPropertyName("networkArchitecture")]
    public string? NetworkArchitecture { get; init; }

    /// <summary>
    /// Gets or initializes the identity subscription ID.
    /// </summary>
    [JsonPropertyName("identitySubscriptionId")]
    public string? IdentitySubscriptionId { get; init; }

    /// <summary>
    /// Gets or initializes the management subscription ID.
    /// </summary>
    [JsonPropertyName("managementSubscriptionId")]
    public string? ManagementSubscriptionId { get; init; }

    /// <summary>
    /// Gets or initializes the connectivity subscription ID.
    /// </summary>
    [JsonPropertyName("connectivitySubscriptionId")]
    public string? ConnectivitySubscriptionId { get; init; }

    /// <summary>
    /// Gets or initializes the Azure regions (comma-separated string).
    /// </summary>
    [JsonPropertyName("regions")]
    public string? Regions { get; init; }

    /// <summary>
    /// Gets or initializes the environment name.
    /// </summary>
    [JsonPropertyName("environmentName")]
    public string? EnvironmentName { get; init; }

    /// <summary>
    /// Gets or initializes the version control system.
    /// </summary>
    [JsonPropertyName("versionControlSystem")]
    public string? VersionControlSystem { get; init; }

    /// <summary>
    /// Gets or initializes the organization name.
    /// </summary>
    [JsonPropertyName("organizationName")]
    public string? OrganizationName { get; init; }

    /// <summary>
    /// Gets or initializes the timestamp when these parameters were cached.
    /// </summary>
    [JsonIgnore]
    public DateTime CachedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a value indicating whether all required parameters are complete.
    /// </summary>
    [JsonIgnore]
    public bool IsComplete =>
        !string.IsNullOrEmpty(RegionType) &&
        !string.IsNullOrEmpty(FireWallType) &&
        !string.IsNullOrEmpty(NetworkArchitecture) &&
        !string.IsNullOrEmpty(IdentitySubscriptionId) &&
        !string.IsNullOrEmpty(ManagementSubscriptionId) &&
        !string.IsNullOrEmpty(ConnectivitySubscriptionId) &&
        !string.IsNullOrEmpty(Regions) &&
        !string.IsNullOrEmpty(EnvironmentName) &&
        !string.IsNullOrEmpty(VersionControlSystem) &&
        !string.IsNullOrEmpty(OrganizationName);
}

/// <summary>
/// Represents the context for a platform landing zone generation session.
/// </summary>
public record PlatformLandingZoneContext(
    string SubscriptionId,
    string ResourceGroupName,
    string MigrateProjectName);
