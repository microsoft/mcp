// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Models;

/// <summary>
/// Represents the current sensitivity label on a column of an Azure SQL database.
/// </summary>
public record SqlDatabaseSensitivityLabel(
    [property: JsonPropertyName("schemaName")] string? SchemaName,
    [property: JsonPropertyName("tableName")] string? TableName,
    [property: JsonPropertyName("columnName")] string? ColumnName,
    [property: JsonPropertyName("labelName")] string? LabelName,
    [property: JsonPropertyName("labelId")] string? LabelId,
    [property: JsonPropertyName("informationType")] string? InformationType,
    [property: JsonPropertyName("informationTypeId")] string? InformationTypeId,
    [property: JsonPropertyName("rank")] string? Rank
);
