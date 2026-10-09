// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options.Disk;

public class DiskDiagnoseOptions
{
    [Option(Description = "The full Azure resource ID of a standalone virtual machine, canonical virtual machine scale set instance, or attached managed disk to diagnose. Do not combine with --resource-group or --vm.")]
    public string? ResourceId { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = "The resource group containing the virtual machine. Required with --vm when --resource-id is omitted.")]
    public string? ResourceGroup { get; set; }

    [Option(Description = "The name of the virtual machine to diagnose. Required with --resource-group when --resource-id is omitted.")]
    public string? Vm { get; set; }

    [Option(Description = "The names of up to 64 specific disks attached to the virtual machine to analyze. If omitted, all attached disks are analyzed.")]
    public string[]? Disk { get; set; }

    [Option(Description = "The analysis start time in ISO 8601 format with an explicit Z, +HH:mm, or -HH:mm UTC offset. Must be provided with --end-time. If both timestamps are omitted, the analysis covers the previous 24 hours.")]
    public string? StartTime { get; set; }

    [Option(Description = "The analysis end time in ISO 8601 format with an explicit Z, +HH:mm, or -HH:mm UTC offset. Must be provided with --start-time. The analysis window cannot exceed 24 hours.")]
    public string? EndTime { get; set; }
}
