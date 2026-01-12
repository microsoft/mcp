// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.Models;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents Azure Resource Manager system metadata for created/modified tracking.
/// Per ARM specification, systemData is automatically managed and tracks:
/// - Creator and creation timestamp
/// - Last modifier and modification timestamp
/// - Creator/modifier types (User, Application, ManagedIdentity, Key)
/// </summary>
public sealed record SystemDataSchema(
    [property: JsonPropertyName("createdBy")] string? CreatedBy = null,
    [property: JsonPropertyName("createdByType")] string? CreatedByType = null,
    [property: JsonPropertyName("createdAt")] DateTime? CreatedAt = null,
    [property: JsonPropertyName("lastModifiedBy")] string? LastModifiedBy = null,
    [property: JsonPropertyName("lastModifiedByType")] string? LastModifiedByType = null,
    [property: JsonPropertyName("lastModifiedAt")] DateTime? LastModifiedAt = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public SystemDataSchema() : this(null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a SystemDataSchema from SystemData.
    /// </summary>
    public static SystemDataSchema FromSystemData(SystemData systemData)
    {
        return new SystemDataSchema(
            systemData.CreatedBy,
            systemData.CreatedByType?.ToString(),
            systemData.CreatedOn?.DateTime,
            systemData.LastModifiedBy,
            systemData.LastModifiedByType?.ToString(),
            systemData.LastModifiedOn?.DateTime
        );
    }
}
