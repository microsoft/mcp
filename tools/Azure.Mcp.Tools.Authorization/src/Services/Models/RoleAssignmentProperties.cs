// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Authorization.Services.Models;

/// <summary>
/// A class representing the Role Assignment properties data model.
/// </summary>
internal sealed class RoleAssignmentProperties
{
    /// <summary> The role assignment scope. </summary>
    public string? Scope { get; set; }
    /// <summary> The role definition ID. </summary>
    public string? RoleDefinitionId { get; set; }
    /// <summary> The principal ID. </summary>
    public Guid? PrincipalId { get; set; }
    /// <summary> The principal type of the assigned principal ID. </summary>
    public string? PrincipalType { get; set; }
    /// <summary> Description of role assignment. </summary>
    public string? Description { get; set; }
    /// <summary> The conditions on the role assignment. This limits the resources it can be assigned to. e.g.: @Resource[Microsoft.Storage/storageAccounts/blobServices/containers:ContainerName] StringEqualsIgnoreCase 'foo_storage_container'. </summary>
    public string? Condition { get; set; }
    /// <summary> Version of the condition. Currently the only accepted value is '2.0'. </summary>
    public string? ConditionVersion { get; set; }
    /// <summary> Time it was created. </summary>
    public DateTimeOffset? CreatedOn { get; set; }
    /// <summary> Time it was updated. </summary>
    public DateTimeOffset? UpdatedOn { get; set; }
    /// <summary> Id of the user who created the assignment. </summary>
    public string? CreatedBy { get; set; }
    /// <summary> Id of the user who updated the assignment. </summary>
    public string? UpdatedBy { get; set; }
    /// <summary> Id of the delegated managed identity resource. </summary>
    public string? DelegatedManagedIdentityResourceId { get; set; }
}
