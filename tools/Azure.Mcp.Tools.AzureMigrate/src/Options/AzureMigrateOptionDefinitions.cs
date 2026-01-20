// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Options;

/// <summary>
/// Static option definitions for Azure Migrate commands.
/// </summary>
public static class AzureMigrateOptionDefinitions
{
    /// <summary>
    /// The topic option name.
    /// </summary>
    public const string TopicName = "topic";

    /// <summary>
    /// The action option name.
    /// </summary>
    public const string ActionName = "action";

    /// <summary>
    /// The region type option name.
    /// </summary>
    public const string RegionTypeName = "region-type";

    /// <summary>
    /// The firewall type option name.
    /// </summary>
    public const string FireWallTypeName = "firewall-type";

    /// <summary>
    /// The network architecture option name.
    /// </summary>
    public const string NetworkArchitectureName = "network-architecture";

    /// <summary>
    /// The identity subscription ID option name.
    /// </summary>
    public const string IdentitySubscriptionIdName = "identity-subscription-id";

    /// <summary>
    /// The management subscription ID option name.
    /// </summary>
    public const string ManagementSubscriptionIdName = "management-subscription-id";

    /// <summary>
    /// The connectivity subscription ID option name.
    /// </summary>
    public const string ConnectivitySubscriptionIdName = "connectivity-subscription-id";

    /// <summary>
    /// The regions option name.
    /// </summary>
    public const string RegionsName = "regions";

    /// <summary>
    /// The environment name option name.
    /// </summary>
    public const string EnvironmentNameName = "environment-name";

    /// <summary>
    /// The version control system option name.
    /// </summary>
    public const string VersionControlSystemName = "version-control-system";

    /// <summary>
    /// The migrate project name option name.
    /// </summary>
    public const string MigrateProjectNameName = "migrate-project-name";

    /// <summary>
    /// The organization name option name.
    /// </summary>
    public const string OrganizationNameName = "organization-name";

    /// <summary>
    /// The user's question or modification request for landing zone guidance.
    /// </summary>
    public static readonly Option<string> Topic = new(
        $"--{TopicName}"
    )
    {
        Description = "The user's question or modification request (e.g., 'turn off bastion', 'disable Enable-DDoS-VNET policy', 'change IP ranges').",
        Required = false
    };

    /// <summary>
    /// The action to perform (update, check, generate, download, status).
    /// </summary>
    public static readonly Option<string> Action = new(
        $"--{ActionName}"
    )
    {
        Description = "The action to perform: 'update' (set parameters), 'check' (check existing landing zone), 'generate' (generate landing zone), 'download' (get download instructions), 'status' (view parameter status).",
        Required = true
    };

    /// <summary>
    /// The region type (single or multi).
    /// </summary>
    public static readonly Option<string> RegionType = new(
        $"--{RegionTypeName}"
    )
    {
        Description = "The region type for the landing zone. Valid values: 'single', 'multi'.",
        Required = false
    };

    /// <summary>
    /// The firewall type (azurefirewall, nva, or none).
    /// </summary>
    public static readonly Option<string> FireWallType = new(
        $"--{FireWallTypeName}"
    )
    {
        Description = "The firewall type for the landing zone. Valid values: 'azurefirewall', 'nva', 'none'.",
        Required = false
    };

    /// <summary>
    /// The network architecture (hubspoke or vwan).
    /// </summary>
    public static readonly Option<string> NetworkArchitecture = new(
        $"--{NetworkArchitectureName}"
    )
    {
        Description = "The network architecture for the landing zone. Valid values: 'hubspoke', 'vwan'.",
        Required = false
    };

    /// <summary>
    /// The identity subscription ID (GUID format).
    /// </summary>
    public static readonly Option<string> IdentitySubscriptionId = new(
        $"--{IdentitySubscriptionIdName}"
    )
    {
        Description = "The Azure subscription ID for the identity management group (GUID format).",
        Required = false
    };

    /// <summary>
    /// The management subscription ID (GUID format).
    /// </summary>
    public static readonly Option<string> ManagementSubscriptionId = new(
        $"--{ManagementSubscriptionIdName}"
    )
    {
        Description = "The Azure subscription ID for the management group (GUID format).",
        Required = false
    };

    /// <summary>
    /// The connectivity subscription ID (GUID format).
    /// </summary>
    public static readonly Option<string> ConnectivitySubscriptionId = new(
        $"--{ConnectivitySubscriptionIdName}"
    )
    {
        Description = "The Azure subscription ID for the connectivity group (GUID format).",
        Required = false
    };

    /// <summary>
    /// The comma-separated list of Azure regions.
    /// </summary>
    public static readonly Option<string> Regions = new(
        $"--{RegionsName}"
    )
    {
        Description = "Comma-separated list of Azure regions (e.g., 'eastus,westus2').",
        Required = false
    };

    /// <summary>
    /// The environment name.
    /// </summary>
    public static readonly Option<string> EnvironmentName = new(
        $"--{EnvironmentNameName}"
    )
    {
        Description = "The environment name for the landing zone.",
        Required = false
    };

    /// <summary>
    /// The version control system (local, github, or azuredevops).
    /// </summary>
    public static readonly Option<string> VersionControlSystem = new(
        $"--{VersionControlSystemName}"
    )
    {
        Description = "The version control system for the landing zone. Valid values: 'local', 'github', 'azuredevops'.",
        Required = false
    };

    /// <summary>
    /// The migrate project name.
    /// </summary>
    public static readonly Option<string> MigrateProjectName = new(
        $"--{MigrateProjectNameName}"
    )
    {
        Description = "The Azure Migrate project name for landing zone generation context.",
        Required = false
    };

    /// <summary>
    /// The organization name.
    /// </summary>
    public static readonly Option<string> OrganizationName = new(
        $"--{OrganizationNameName}"
    )
    {
        Description = "The organization name for the landing zone.",
        Required = false
    };
}
