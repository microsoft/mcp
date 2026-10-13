// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Resources;

public sealed class DrillAddOrUpdateResourcesOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = ResilienceManagementOptionDescriptions.Drill)]
    public required string Drill { get; set; }

    [Option(Description = "The fault duration in minutes applied to the drill resources.")]
    public required int FaultDurationMinutes { get; set; }

    [Option(Description =
        "A JSON array of resources to include in the drill. Each item is an object with an \"id\" (the ARM resource ID) " +
        "and optional \"faultProperties\". Example: [{\"id\":\"/subscriptions/.../providers/Microsoft.Compute/virtualMachines/vm1\"}].")]
    public string? IncludeResources { get; set; }

    [Option(Description =
        "A JSON array of already-included drill resources to update. Each item is an object with an \"id\" (the ARM resource ID) " +
        "and optional \"faultProperties\".")]
    public string? UpdateResources { get; set; }

    [Option(Description = "A JSON array of ARM resource ID strings to exclude from the drill.")]
    public string? ExcludeResources { get; set; }

    [Option(Description = "Whether to force inclusion and update of the resources. Allowed values: Enable, Disable.")]
    public string? ForceInclusionAndUpdate { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    internal AddOrUpdateResourcesContent? ParsedContent { get; set; }
}
