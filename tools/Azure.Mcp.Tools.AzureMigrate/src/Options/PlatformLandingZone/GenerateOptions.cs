// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

/// <summary>
/// Options for the platform landing zone generate landing zone command.
/// </summary>
public class GenerateOptions : BaseAzureMigrateOptions
{
    /// <summary>
    /// Gets or sets the action to perform (update, generate, download, status).
    /// </summary>
    [JsonPropertyName("action")]
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets the region type (single or multi).
    /// </summary>
    [JsonPropertyName("regionType")]
    public string? RegionType { get; set; }

    /// <summary>
    /// Gets or sets the firewall type (azurefirewall or nva).
    /// </summary>
    [JsonPropertyName("fireWallType")]
    public string? FireWallType { get; set; }

    /// <summary>
    /// Gets or sets the network architecture (hubspoke or vwan).
    /// </summary>
    [JsonPropertyName("networkArchitecture")]
    public string? NetworkArchitecture { get; set; }

    /// <summary>
    /// Gets or sets the identity subscription ID (GUID format).
    /// </summary>
    [JsonPropertyName("identitySubscriptionId")]
    public string? IdentitySubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the management subscription ID (GUID format).
    /// </summary>
    [JsonPropertyName("managementSubscriptionId")]
    public string? ManagementSubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the connectivity subscription ID (GUID format).
    /// </summary>
    [JsonPropertyName("connectivitySubscriptionId")]
    public string? ConnectivitySubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the security subscription ID (GUID format).
    /// </summary>
    [JsonPropertyName("securitySubscriptionId")]
    public string? SecuritySubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated list of Azure regions.
    /// </summary>
    [JsonPropertyName("regions")]
    public string? Regions { get; set; }

    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    [JsonPropertyName("environmentName")]
    public string? EnvironmentName { get; set; }

    /// <summary>
    /// Gets or sets the version control system (local, github, or azuredevops).
    /// </summary>
    [JsonPropertyName("versionControlSystem")]
    public string? VersionControlSystem { get; set; }

    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    [JsonPropertyName("organizationName")]
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Gets or sets the migrate project name from context.
    /// </summary>
    [JsonPropertyName("migrateProjectName")]
    public string? MigrateProjectName { get; set; }

    /// <summary>
    /// Gets or sets the full resource ID of the Azure Migrate project.
    /// </summary>
    [JsonPropertyName("migrateProjectResourceId")]
    public string? MigrateProjectResourceId { get; set; }
}
