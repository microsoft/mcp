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
    /// Gets or sets the action to perform (update, generate, download, status, check).
    /// </summary>
    [Option(Description = "The action to perform: 'update' (set parameters), 'check' (check existing platform landing zone), 'generate' (generate platform landing zone), 'download' (get download instructions), 'status' (view parameter status), 'createmigrateproject' (create new migration project).")]
    public required string Action { get; set; }

    /// <summary>
    /// Gets or sets the region type (single or multi).
    /// </summary>
    [Option(Description = "The region type for the Platform Landing Zone. Valid values: 'single', 'multi'.")]
    public string? RegionType { get; set; }

    /// <summary>
    /// Gets or sets the firewall type (azurefirewall or nva).
    /// </summary>
    [Option(Description = "The firewall type for the Platform Landing Zone. Valid values: 'azurefirewall', 'nva'.")]
    public string? FirewallType { get; set; }

    /// <summary>
    /// Gets or sets the network architecture (hubspoke or vwan).
    /// </summary>
    [Option(Description = "The network architecture for the Platform Landing Zone. Valid values: 'hubspoke', 'vwan'.")]
    public string? NetworkArchitecture { get; set; }

    /// <summary>
    /// Gets or sets the identity subscription ID (GUID format).
    /// </summary>
    [Option(Description = "The Azure subscription ID for the identity management group in Platform Landing Zone (GUID format).")]
    public string? IdentitySubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the management subscription ID (GUID format).
    /// </summary>
    [Option(Description = "The Azure subscription ID for the management group in Platform Landing Zone (GUID format).")]
    public string? ManagementSubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the connectivity subscription ID (GUID format).
    /// </summary>
    [Option(Description = "The Azure subscription ID for the connectivity group in Platform Landing Zone (GUID format).")]
    public string? ConnectivitySubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated list of Azure regions.
    /// </summary>
    [Option(Description = "Comma-separated list of Azure regions for Platform Landing Zone (e.g., 'eastus,westus2').")]
    public string? Regions { get; set; }

    /// <summary>
    /// Gets or sets the environment name.
    /// </summary>
    [Option(Description = "The environment name for the Platform Landing Zone.")]
    public string? EnvironmentName { get; set; }

    /// <summary>
    /// Gets or sets the version control system (local, github, or azuredevops).
    /// </summary>
    [Option(Description = "The version control system for the Platform Landing Zone. Valid values: 'local', 'github', 'azuredevops'.")]
    public string? VersionControlSystem { get; set; }

    /// <summary>
    /// Gets or sets the organization name.
    /// </summary>
    [Option(Description = "The organization name for the Platform Landing Zone.")]
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Gets or sets the migrate project name from context.
    /// </summary>
    [Option(Description = "The Azure Migrate project name for Platform Landing Zone generation context.")]
    public required string MigrateProjectName { get; set; }

    /// <summary>
    /// Gets or sets the Azure region location for resource creation.
    /// </summary>
    [Option(Description = "The Azure region location for creating new resources (e.g., 'eastus', 'westus2'). Required for 'createmigrateproject' action.")]
    public string? Location { get; set; }

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

    /// <summary>
    /// Gets or sets the retry policy options for HTTP requests.
    /// </summary>
    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
