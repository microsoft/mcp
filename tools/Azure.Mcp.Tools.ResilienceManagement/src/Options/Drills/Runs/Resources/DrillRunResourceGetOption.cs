// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Runs.Resources;

public sealed class DrillRunResourceGetOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = ResilienceManagementOptionDescriptions.Drill)]
    public required string Drill { get; set; }

    [Option(Description = "The name of the drill run containing the resources.")]
    public required string DrillRun { get; set; }

    [Option(Description = "The name of the drill run resource. Provide this argument to get the details of a particular resource; omit it to list all resources of the drill run (id and name only).")]
    public string? Name { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
