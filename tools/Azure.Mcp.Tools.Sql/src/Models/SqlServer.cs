// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Models;

public record SqlServer(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("fullyQualifiedDomainName")] string? FullyQualifiedDomainName,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("resourceGroup")] string ResourceGroup,
    [property: JsonPropertyName("subscription")] string Subscription,
    [property: JsonPropertyName("administratorLogin")] string? AdministratorLogin,
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("publicNetworkAccess")] string? PublicNetworkAccess,
    [property: JsonPropertyName("tags")] Dictionary<string, string>? Tags
);
