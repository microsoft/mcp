// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Authorization.Models;

public sealed class RoleAssignmentApproval
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? PrincipalId { get; set; }
    public string? RoleDefinitionId { get; set; }
    public string? RequestorId { get; set; }
    public string? Scope { get; set; }
    public string? Status { get; set; }
    public string? CreatedOn { get; set; }
    public List<RoleAssignmentApprovalStage> Stages { get; set; } = [];
}
