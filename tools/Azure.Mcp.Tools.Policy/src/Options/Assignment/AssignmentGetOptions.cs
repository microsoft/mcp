// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Policy.Models;

namespace Azure.Mcp.Tools.Policy.Options.Assignment;

public class AssignmentGetOptions : BasePolicyOptions
{
    public string? Assignment { get; set; }

    public PolicyAssignment[] policyAssignments { get; set; } = Array.Empty<PolicyAssignment>();
}
