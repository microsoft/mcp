// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.OneLake.Models;

/// <summary>
/// Represents a data access role defined on a OneLake item.
/// </summary>
public class DataAccessRole
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("decisionRules")]
    public List<DecisionRule>? DecisionRules { get; set; }

    [JsonPropertyName("members")]
    public DataAccessRoleMembers? Members { get; set; }
}

/// <summary>
/// Decision rule within a data access role defining access permissions.
/// </summary>
public class DecisionRule
{
    [JsonPropertyName("effect")]
    public string? Effect { get; set; }

    [JsonPropertyName("permission")]
    public string? Permission { get; set; }

    [JsonPropertyName("scope")]
    public List<DecisionRuleScope>? Scope { get; set; }
}

/// <summary>
/// Scope definition for a decision rule.
/// </summary>
public class DecisionRuleScope
{
    [JsonPropertyName("attributeName")]
    public string? AttributeName { get; set; }

    [JsonPropertyName("attributeValueIncludedIn")]
    public List<string>? AttributeValueIncludedIn { get; set; }
}

/// <summary>
/// Members of a data access role.
/// </summary>
public class DataAccessRoleMembers
{
    [JsonPropertyName("fabricItemMembers")]
    public List<FabricItemMember>? FabricItemMembers { get; set; }

    [JsonPropertyName("microsoftEntraMembers")]
    public List<MicrosoftEntraMember>? MicrosoftEntraMembers { get; set; }
}

/// <summary>
/// A Fabric item member in a data access role.
/// </summary>
public class FabricItemMember
{
    [JsonPropertyName("sourceItemId")]
    public string? SourceItemId { get; set; }
}

/// <summary>
/// A Microsoft Entra member in a data access role.
/// </summary>
public class MicrosoftEntraMember
{
    [JsonPropertyName("objectId")]
    public string? ObjectId { get; set; }

    [JsonPropertyName("tenantId")]
    public string? TenantId { get; set; }
}

/// <summary>
/// Response from the List Data Access Roles API.
/// </summary>
public class DataAccessRoleListResponse
{
    [JsonPropertyName("value")]
    public List<DataAccessRole>? Value { get; set; }

    [JsonPropertyName("continuationToken")]
    public string? ContinuationToken { get; set; }

    [JsonPropertyName("continuationUri")]
    public string? ContinuationUri { get; set; }
}
