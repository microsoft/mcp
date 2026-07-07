// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

/// <summary>
/// Static option definitions for Platform Landing Zone commands.
/// </summary>
public static class PlatformLandingZoneOptionDefinitions
{
    /// <summary>
    /// The scenario option name.
    /// </summary>
    public const string ScenarioName = "scenario";

    /// <summary>
    /// The policy name option name.
    /// </summary>
    public const string PolicyNameName = "policy-name";

    /// <summary>
    /// The list policies option name.
    /// </summary>
    public const string ListPoliciesName = "list-policies";

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
    /// The security subscription ID option name.
    /// </summary>
    public const string SecuritySubscriptionIdName = "security-subscription-id";

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
    /// The migrate project resource ID option name.
    /// </summary>
    public const string MigrateProjectResourceIdName = "migrate-project-resource-id";

    /// <summary>
    /// The location option name.
    /// </summary>
    public const string LocationName = "location";

    /// <summary>
    /// The scenario key for platform landing zone modification.
    /// </summary>
    public static readonly Option<string> Scenario = new(
        $"--{ScenarioName}"
    )
    {
        Description = "The modification scenario key. Valid values: resource-names, management-groups, ddos, bastion, dns, gateways, regions, ip-addresses, policy-enforcement, policy-assignment, ama, amba, defender, zero-trust, slz.",
        Required = true
    };

    /// <summary>
    /// The policy assignment name for policy-related scenarios.
    /// </summary>
    public static readonly Option<string> PolicyName = new(
        $"--{PolicyNameName}"
    )
    {
        Description = "The policy assignment name to look up (e.g., 'Enable-DDoS-VNET'). Used with policy-enforcement or policy-assignment scenarios.",
        Required = false
    };

    /// <summary>
    /// List all available policies organized by archetype.
    /// </summary>
    public static readonly Option<bool> ListPolicies = new(
        $"--{ListPoliciesName}"
    )
    {
        Description = "Set to true to list all available policies organized by archetype. Useful for finding the exact policy name.",
        Required = false
    };

    /// <summary>
    /// The action to perform (update, check, generate, download, status).
    /// </summary>
    public static readonly Option<string> Action = new(
        $"--{ActionName}"
    )
    {
        Description = "The action to perform: 'update' (set parameters), 'check' (check existing platform landing zone), 'generate' (generate platform landing zone), 'download' (get download instructions), 'status' (view parameter status).",
        Required = true
    };

    /// <summary>
    /// The region type (single or multi).
    /// </summary>
    public static readonly Option<string> RegionType = new(
        $"--{RegionTypeName}"
    )
    {
        Description = "The region type for the Platform Landing Zone. Valid values: 'single', 'multi'.",
        Required = false
    };

    /// <summary>
    /// The firewall type (azurefirewall, nva).
    /// </summary>
    public static readonly Option<string> FireWallType = new(
        $"--{FireWallTypeName}"
    )
    {
        Description = "The firewall type for the Platform Landing Zone. Valid values: 'azurefirewall', 'nva'.",
        Required = false
    };

    /// <summary>
    /// The network architecture (hubspoke or vwan).
    /// </summary>
    public static readonly Option<string> NetworkArchitecture = new(
        $"--{NetworkArchitectureName}"
    )
    {
        Description = "The network architecture for the Platform Landing Zone. Valid values: 'hubspoke', 'vwan'.",
        Required = false
    };

    /// <summary>
    /// The identity subscription ID (GUID format).
    /// </summary>
    public static readonly Option<string> IdentitySubscriptionId = new(
        $"--{IdentitySubscriptionIdName}"
    )
    {
        Description = "The Azure subscription ID for the identity management group in Platform Landing Zone (GUID format).",
        Required = false
    };

    /// <summary>
    /// The management subscription ID (GUID format).
    /// </summary>
    public static readonly Option<string> ManagementSubscriptionId = new(
        $"--{ManagementSubscriptionIdName}"
    )
    {
        Description = "The Azure subscription ID for the management group in Platform Landing Zone (GUID format).",
        Required = false
    };

    /// <summary>
    /// The connectivity subscription ID (GUID format).
    /// </summary>
    public static readonly Option<string> ConnectivitySubscriptionId = new(
        $"--{ConnectivitySubscriptionIdName}"
    )
    {
        Description = "The Azure subscription ID for the connectivity group in Platform Landing Zone (GUID format).",
        Required = false
    };

    /// <summary>
    /// The security subscription ID (GUID format).
    /// </summary>
    public static readonly Option<string> SecuritySubscriptionId = new(
        $"--{SecuritySubscriptionIdName}"
    )
    {
        Description = "The Azure subscription ID for security resources in Platform Landing Zone (GUID format).",
        Required = false
    };

    /// <summary>
    /// The comma-separated list of Azure regions.
    /// </summary>
    public static readonly Option<string> Regions = new(
        $"--{RegionsName}"
    )
    {
        Description = "Comma-separated list of Azure regions for Platform Landing Zone (e.g., 'eastus,westus2').",
        Required = false
    };

    /// <summary>
    /// The environment name.
    /// </summary>
    public static readonly Option<string> EnvironmentName = new(
        $"--{EnvironmentNameName}"
    )
    {
        Description = "The environment name for the Platform Landing Zone.",
        Required = false
    };

    /// <summary>
    /// The version control system (local, github, or azuredevops).
    /// </summary>
    public static readonly Option<string> VersionControlSystem = new(
        $"--{VersionControlSystemName}"
    )
    {
        Description = "The version control system for the Platform Landing Zone. Valid values: 'local', 'github', 'azuredevops'.",
        Required = false
    };

    /// <summary>
    /// The migrate project name.
    /// </summary>
    public static readonly Option<string> MigrateProjectName = new(
        $"--{MigrateProjectNameName}"
    )
    {
        Description = "The Azure Migrate project name for Platform Landing Zone generation context.",
        Required = true
    };

    /// <summary>
    /// The organization name.
    /// </summary>
    public static readonly Option<string> OrganizationName = new(
        $"--{OrganizationNameName}"
    )
    {
        Description = "The organization name for the Platform Landing Zone.",
        Required = false
    };

    /// <summary>
    /// The full resource ID of the Azure Migrate project.
    /// </summary>
    public static readonly Option<string> MigrateProjectResourceId = new(
        $"--{MigrateProjectResourceIdName}"
    )
    {
        Description = "The full resource ID of the Azure Migrate project for Platform Landing Zone (alternative to subscription/resourceGroup/migrateProjectName).",
        Required = false
    };

    /// <summary>
    /// The Azure region location for resource creation.
    /// </summary>
    public static readonly Option<string> Location = new(
        $"--{LocationName}"
    )
    {
        Description = "The Azure region location for creating new resources (e.g., 'eastus', 'westus2'). Required for 'createmigrateproject' action.",
        Required = false
    };
}
