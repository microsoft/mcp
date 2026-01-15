// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Policy.Models;

namespace Azure.Mcp.Tools.Policy.Options.Assignment;

/// <summary>
/// Options for a future policy assignment <c>get</c> command.
/// </summary>
/// <remarks>
/// This type is intentionally defined ahead of the corresponding command implementation,
/// so it may not be referenced elsewhere in the codebase yet.
/// </remarks>
public class AssignmentGetOptions : BasePolicyOptions
{
    public string? Assignment { get; set; }

    public PolicyAssignment[] policyAssignments { get; set; } = Array.Empty<PolicyAssignment>();
}
