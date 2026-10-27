// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResilienceManagement.Options.Goals.Assignments;

public sealed class GoalAssignmentUpdateOptions : IGoalAssignmentOptions
{
    [Option(Description = ResilienceManagementOptionDescriptions.ServiceGroup)]
    public required string ServiceGroup { get; set; }

    [Option(Description = ResilienceManagementOptionDescriptions.GoalAssignment)]
    public required string GoalAssignment { get; set; }

    [Option(Description = "Replacement ARM IDs of existing Microsoft.Monitor/slis resources in this service group. Specify this or clear-service-level-resources, not both. The zonal requirement is preserved.")]
    public string[]? ServiceLevelResourceIds { get; set; }

    [Option(Description = "Explicitly remove all service-level indicator associations. Does not delete the indicators or change the zonal resiliency requirement.")]
    public bool ClearServiceLevelResources { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
