// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Models;

/// <summary>
/// Represents an Azure SQL Server firewall rule.
/// </summary>
public record SqlServerFirewallRule(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("startIpAddress")] string? StartIpAddress,
    [property: JsonPropertyName("endIpAddress")] string? EndIpAddress
);
