// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs;

public sealed class DrillRunFailoverOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = ResilienceManagementOptionDescriptions.Drill)]
    public required string Drill { get; set; }

    [Option(Description = "The name of the drill run on which to initiate failover.")]
    public required string DrillRun { get; set; }

    [Option(Description = "The physical Azure zones from which resources will be failed over, such as 'eastus-az1'. Omit together with selected-resource-ids and auto-failover to send no request body and use service defaults. Required when either of those options is supplied.")]
    public string[]? SourceLocations { get; set; }

    [Option(Description = "The ARM IDs of recovery resources to fail over. Omit this option to process all qualified resources in the source locations.")]
    public string[]? SelectedResourceIds { get; set; }

    [Option(Description = "Whether to proceed automatically from fault injection to failover. Requires source-locations. Defaults to false when source-locations are supplied; omit all failover inputs to use service defaults.")]
    public bool? AutoFailover { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
