// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;

/// <summary>
/// Options for the platform landing zone get guidance command.
/// </summary>
public sealed class GetGuidanceOptions
{
    /// <summary>
    /// Gets or sets the scenario key for the modification.
    /// </summary>
    [Option(Description = "The modification scenario key. Valid values: resource-names, management-groups, ddos, bastion, dns, gateways, regions, ip-addresses, policy-enforcement, policy-assignment, ama, amba, defender, zero-trust, slz.")]
    public required string Scenario { get; set; }

    /// <summary>
    /// Gets or sets the policy name for policy-related scenarios.
    /// </summary>
    [Option(Description = "The policy assignment name to look up (e.g., 'Enable-DDoS-VNET'). Used with policy-enforcement or policy-assignment scenarios.")]
    public string? PolicyName { get; set; }

    /// <summary>
    /// Gets or sets whether to list all policies by archetype.
    /// </summary>
    [Option(Description = "Set to true to list all available policies organized by archetype. Useful for finding the exact policy name.")]
    public bool ListPolicies { get; set; }
}
