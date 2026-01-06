// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents Azure Resource Manager system metadata for created/modified tracking.
/// Per ARM specification, systemData is automatically managed and tracks:
/// - Creator and creation timestamp
/// - Last modifier and modification timestamp
/// - Creator/modifier types (User, Application, ManagedIdentity, Key)
/// </summary>
public class SystemDataSchema
{
    /// <summary>
    /// Gets or sets the identity that created the resource.
    /// </summary>
    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the type of identity that created the resource.
    /// Values: User, Application, ManagedIdentity, Key
    /// </summary>
    [JsonPropertyName("createdByType")]
    public string? CreatedByType { get; set; }

    /// <summary>
    /// Gets or sets the timestamp (in UTC) when the resource was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identity that last modified the resource.
    /// </summary>
    [JsonPropertyName("lastModifiedBy")]
    public string? LastModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the type of identity that last modified the resource.
    /// Values: User, Application, ManagedIdentity, Key
    /// </summary>
    [JsonPropertyName("lastModifiedByType")]
    public string? LastModifiedByType { get; set; }

    /// <summary>
    /// Gets or sets the timestamp (in UTC) when the resource was last modified.
    /// </summary>
    [JsonPropertyName("lastModifiedAt")]
    public DateTime? LastModifiedAt { get; set; }
}
