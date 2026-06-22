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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("eTag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ETag { get; set; }

    [JsonPropertyName("kind")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Kind { get; set; }

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
    public List<DecisionRuleScope>? Permission { get; set; }

    [JsonPropertyName("constraints")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Constraints? Constraints { get; set; }
}

/// <summary>
/// Row and column constraints for a decision rule (row/column level security).
/// </summary>
public class Constraints
{
    [JsonPropertyName("columns")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ColumnConstraint>? Columns { get; set; }

    [JsonPropertyName("rows")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<RowConstraint>? Rows { get; set; }
}

/// <summary>
/// Column-level security constraint for a specific table.
/// </summary>
public class ColumnConstraint
{
    [JsonPropertyName("tablePath")]
    public string? TablePath { get; set; }

    [JsonPropertyName("columnNames")]
    public List<string>? ColumnNames { get; set; }

    [JsonPropertyName("columnEffect")]
    public string? ColumnEffect { get; set; }

    [JsonPropertyName("columnAction")]
    public List<string>? ColumnAction { get; set; }
}

/// <summary>
/// Row-level security constraint for a specific table using a T-SQL predicate.
/// </summary>
public class RowConstraint
{
    [JsonPropertyName("tablePath")]
    public string? TablePath { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

/// <summary>
/// Scope definition for a decision rule permission.
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
    [JsonPropertyName("sourcePath")]
    public string? SourcePath { get; set; }

    [JsonPropertyName("itemAccess")]
    public List<string>? ItemAccess { get; set; }
}

/// <summary>
/// A Microsoft Entra member in a data access role.
/// </summary>
public class MicrosoftEntraMember
{
    [JsonPropertyName("objectId")]
    public string? ObjectId { get; set; }

    [JsonPropertyName("objectType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ObjectType { get; set; }

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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationToken { get; set; }

    [JsonPropertyName("continuationUri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationUri { get; set; }
}

/// <summary>
/// Request body for the PUT Data Access Roles API (bulk replace). Only 'value' is accepted.
/// </summary>
public class DataAccessRolePutRequest
{
    [JsonPropertyName("value")]
    public List<DataAccessRole> Value { get; set; } = [];
}
