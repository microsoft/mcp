// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

/// <summary>
/// Options for the platform landing zone request command.
/// </summary>
public sealed class RequestOptions : ISubscriptionOption
{
    /// <summary>
    /// Gets or sets the action to perform.
    /// </summary>
    [Option(Description = "The action to perform: 'createmigrateproject' (create a new Azure Migrate project), 'list' (list landing zones under the project), 'get' (read the landing zone and its generation status), 'create' (create or update the landing zone and start generation), 'wait' (poll until generation reaches a terminal status), 'download' (download the generated output).")]
    public required string Action { get; set; }

    /// <summary>
    /// Gets or sets the network architecture (hubspoke or vwan).
    /// </summary>
    [Option(Description = "The network architecture for the Platform Landing Zone. Valid values: 'hubspoke', 'vwan'. Maps to connectivity.topology.")]
    public string? NetworkArchitecture { get; set; }

    /// <summary>
    /// Gets or sets the firewall type (azurefirewall, nva or none).
    /// </summary>
    [Option(Description = "The firewall type for the Platform Landing Zone. Valid values: 'azurefirewall', 'nva'. Maps to connectivity.firewall.kind. A landing zone with no firewall is not supported by generation yet.")]
    public string? FirewallType { get; set; }

    /// <summary>
    /// Gets or sets whether Azure Bastion is deployed.
    /// </summary>
    [Option(Description = "Whether Azure Bastion is deployed. Valid values: 'enabled', 'disabled'. Omit to accept the service default (enabled).")]
    public string? Bastion { get; set; }

    /// <summary>
    /// Gets or sets whether DDoS protection is deployed.
    /// </summary>
    [Option(Description = "Whether DDoS network protection is deployed. Valid values: 'enabled', 'disabled'. Omit to accept the service default (enabled). This is the single largest cost line item in a landing zone.")]
    public string? Ddos { get; set; }

    /// <summary>
    /// Gets or sets whether private DNS zones are deployed.
    /// </summary>
    [Option(Description = "Whether Private DNS zones and centralized resolution are deployed. Valid values: 'enabled', 'disabled'. Omit to accept the service default (enabled). Disabling also switches off the Deploy-Private-DNS-Zones policy assignment, which this tool handles automatically.")]
    public string? PrivateDns { get; set; }

    /// <summary>
    /// Gets or sets whether an ExpressRoute gateway is deployed.
    /// </summary>
    [Option(Description = "Whether an ExpressRoute gateway is deployed. Valid values: 'enabled', 'disabled'. Omit to accept the service default (enabled). Enabling requires --network-architecture in the same call.")]
    public string? ExpressRoute { get; set; }

    /// <summary>
    /// Gets or sets whether a VPN gateway is deployed.
    /// </summary>
    [Option(Description = "Whether a VPN gateway is deployed. Valid value: 'enabled'. Omit to accept the service default (enabled). Enabling requires --network-architecture in the same call. Removing the VPN gateway is not supported by generation yet.")]
    public string? VpnGateway { get; set; }

    /// <summary>
    /// Gets or sets the deployment scale tier.
    /// </summary>
    [Option(Description = "The deployment scale for the Platform Landing Zone. Valid values: 'full', 'managementonly'. 'managementonly' omits connectivity entirely.")]
    public string? ScaleTier { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated list of Azure regions.
    /// </summary>
    [Option(Description = "Comma-separated list of Azure regions for the Platform Landing Zone (e.g., 'eastus,westus2'). The first region is primary. Supplying more than one region makes this a multi-region landing zone; there is no separate region-type parameter.")]
    public string? Regions { get; set; }

    /// <summary>
    /// Gets or sets the identity subscription ID (GUID format).
    /// </summary>
    [Option(Description = "The Azure subscription ID for identity platform resources (GUID format).")]
    public string? IdentitySubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the management subscription ID (GUID format).
    /// </summary>
    [Option(Description = "The Azure subscription ID for management platform resources (GUID format).")]
    public string? ManagementSubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the connectivity subscription ID (GUID format).
    /// </summary>
    [Option(Description = "The Azure subscription ID for connectivity platform resources (GUID format).")]
    public string? ConnectivitySubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the security subscription ID (GUID format).
    /// </summary>
    [Option(Description = "The Azure subscription ID for security platform resources (GUID format).")]
    public string? SecuritySubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the parent management group resource ID.
    /// </summary>
    [Option(Description = "Resource ID of the management group under which the Azure Landing Zones management-group hierarchy is created. Create-only: it cannot be changed after the landing zone is created.")]
    public string? ParentManagementGroupId { get; set; }

    /// <summary>
    /// Gets or sets the version control system (local, github, or azuredevops).
    /// </summary>
    [Option(Description = "The version control system used to materialize the landing zone. Valid values: 'local', 'github', 'azuredevops'.")]
    public string? VersionControlSystem { get; set; }

    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    [Option(Description = "The name of the organization or environment that owns this landing zone. Surfaced to the generator as ENVIRONMENT_NAME.")]
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Gets or sets the service name.
    /// </summary>
    [Option(Description = "The name of the service associated with this landing zone. Surfaced to the generator as SERVICE_NAME.")]
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the migrate project name from context.
    /// </summary>
    [Option(Description = "The Azure Migrate project name that owns the Platform Landing Zone.")]
    public required string MigrateProjectName { get; set; }

    /// <summary>
    /// Gets or sets the Azure region location for resource creation.
    /// </summary>
    [Option(Description = "The Azure region location for creating new resources (e.g., 'eastus', 'westus2'). Required for the 'createmigrateproject' action.")]
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the timeout, in minutes, applied by the 'wait' action.
    /// </summary>
    [Option(Description = "Maximum number of minutes the 'wait' action polls for generation to reach a terminal status. Defaults to 30.")]
    public int? TimeoutMinutes { get; set; }

    /// <summary>
    /// Gets or sets whether the design document is downloaded alongside the output archive.
    /// </summary>
    [Option(Description = "Set to true to also download design-document.md alongside output.zip during the 'download' action.")]
    public bool IncludeDesignDocument { get; set; }

    /// <summary>
    /// Gets or sets the resource group name for resource creation.
    /// </summary>
    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    /// <summary>
    /// Gets or sets the subscription ID for resource creation.
    /// </summary>
    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID for resource creation.
    /// </summary>
    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
