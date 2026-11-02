// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.ServiceGroupIntelligence;

/// <summary> Options for getting the Advisor status of Service Groups. </summary>
public sealed class ServiceGroupIntelligenceGetOptions
{
    [Option(Description = "Service Group name (for example 'commerce'), not its full ARM ID. " +
        "Omit to return the status of all Service Groups.")]
    public string? ServiceGroup { get; set; }

    [Option(Description = "Filter by criticality tier: MissionCritical (Tier 0), BusinessCritical (Tier 1), BusinessOperational (Tier 2), Administrative (Tier 3) or NotApplicable. " +
        "Case-insensitive; Tier0-Tier3 and 0-3 are also accepted. Use MissionCritical for mission-critical Service Groups.")]
    public string? Criticality { get; set; }

    [Option(Description = "Filter by status: 'On Track' (no significant risks), 'Attention Needed' (limited, manageable issues), 'At Risk' (material risks), 'Critical' (immediate action required) or 'Unavailable' (no active recommendations). " +
        "Case-insensitive; spaces are optional. Use 'Attention Needed' for Service Groups that need attention.")]
    public string? Status { get; set; }

    [Option(Description = "Maximum number of Service Groups to return (1-500, default 50).")]
    public int? Top { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
