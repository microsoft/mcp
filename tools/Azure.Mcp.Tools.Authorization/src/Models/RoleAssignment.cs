// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Authorization.Models;

public class RoleAssignment
{
    /// <summary>Fully qualified resource ID for the resource.</summary>
    public string? Id { get; set; }

    /// <summary>The name of the resource.</summary>
    public string? Name { get; set; }

    /// <summary>The role definition ID.</summary>
    public string? RoleDefinitionId { get; set; }

    /// <summary> The role assignment scope.</summary>
    public string? Scope { get; set; }

    /// <summary>The principal ID.</summary>
    public Guid? PrincipalId { get; set; }

    /// <summary>The principal type of the assigned principal ID.</summary>
    public string? PrincipalType { get; set; }

    /// <summary>Description of role assignment.</summary>
    public string? Description { get; set; }

    /// <summary>Id of the delegated managed identity resource.</summary>
    public string? DelegatedManagedIdentityResourceId { get; set; }

    /// <summary>The conditions on the role assignment. This limits the resources it can be assigned to. e.g.: @Resource[Microsoft.Storage/storageAccounts/blobServices/containers:ContainerName] StringEqualsIgnoreCase 'foo_storage_container'.</summary>
    public string? Condition { get; set; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;
        if (obj is not RoleAssignment other)
            return false;
        return string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Name, other.Name, StringComparison.Ordinal)
            && string.Equals(RoleDefinitionId, other.RoleDefinitionId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Scope, other.Scope, StringComparison.OrdinalIgnoreCase)
            && Nullable.Equals(PrincipalId, other.PrincipalId)
            && string.Equals(PrincipalType, other.PrincipalType, StringComparison.Ordinal)
            && string.Equals(Description, other.Description, StringComparison.Ordinal)
            && string.Equals(DelegatedManagedIdentityResourceId, other.DelegatedManagedIdentityResourceId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Condition, other.Condition, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id, StringComparer.OrdinalIgnoreCase);
        hash.Add(Name, StringComparer.Ordinal);
        hash.Add(RoleDefinitionId, StringComparer.OrdinalIgnoreCase);
        hash.Add(Scope, StringComparer.OrdinalIgnoreCase);
        hash.Add(PrincipalId);
        hash.Add(PrincipalType, StringComparer.Ordinal);
        hash.Add(Description, StringComparer.Ordinal);
        hash.Add(DelegatedManagedIdentityResourceId, StringComparer.OrdinalIgnoreCase);
        hash.Add(Condition, StringComparer.OrdinalIgnoreCase);
        return hash.ToHashCode();
    }
}
