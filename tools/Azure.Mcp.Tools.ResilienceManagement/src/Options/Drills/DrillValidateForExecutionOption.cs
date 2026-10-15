// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Drills;

public sealed class DrillValidateForExecutionOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = ResilienceManagementOptionDescriptions.Drill)]
    public required string Drill { get; set; }

    [Option(Description = "Physical source locations from which resources will be failed over or faulted, such as 'westus2-az1'. Required unless operation-name is Reprotect, including when operation-name is omitted.")]
    public string[]? SourceLocations { get; set; }

    [Option(Description = "Optional operation to validate: Failover, Reprotect, FailoverReverse, or ReprotectReverse. Omit to use the service's default validation.")]
    public DrillExecutionOperation? OperationName { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
