// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Authorization.Models;

public sealed class RoleAssignmentApprovalStage
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public bool? AssignedToMe { get; set; }
    public string? Status { get; set; }
    public string? ReviewResult { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewedDateTime { get; set; }
    public string? Justification { get; set; }
}
