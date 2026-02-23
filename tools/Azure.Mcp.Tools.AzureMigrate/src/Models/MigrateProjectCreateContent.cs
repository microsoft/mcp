// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureMigrate.Models;

/// <summary>
/// Content for creating or updating an Azure Migrate project.
/// </summary>
public sealed class MigrateProjectCreateContent
{
    /// <summary>
    /// Gets or sets the Azure location for the migrate project.
    /// </summary>
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the properties of the migrate project.
    /// </summary>
    [JsonPropertyName("properties")]
    public MigrateProjectProperties? Properties { get; set; }
}

/// <summary>
/// Properties for an Azure Migrate project.
/// </summary>
public sealed class MigrateProjectProperties
{
}
