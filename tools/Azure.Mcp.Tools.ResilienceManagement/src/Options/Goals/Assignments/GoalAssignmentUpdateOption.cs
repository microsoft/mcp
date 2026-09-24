// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;

public sealed class GoalAssignmentUpdateOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = "The name of the existing goal assignment to update.")]
    public required string GoalAssignment { get; set; }

    [Option(Description = "The full Azure resource ID of the service-level indicator resource.")]
    public required string ServiceLevelIndicatorResourceId { get; set; }

    [Option(Description = "The full Azure resource ID of the service-level objective resource.")]
    public required string ServiceLevelObjectiveResourceId { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
