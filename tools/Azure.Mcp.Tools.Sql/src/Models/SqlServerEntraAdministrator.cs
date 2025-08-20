// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Models;

/// <summary>
/// Represents an Azure SQL Server Microsoft Entra ID administrator.
/// </summary>
public record SqlServerEntraAdministrator(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("administratorType")] string? AdministratorType,
    [property: JsonPropertyName("login")] string? Login,
    [property: JsonPropertyName("sid")] string? Sid,
    [property: JsonPropertyName("tenantId")] string? TenantId,
    [property: JsonPropertyName("azureADOnlyAuthentication")] bool? AzureADOnlyAuthentication
);
