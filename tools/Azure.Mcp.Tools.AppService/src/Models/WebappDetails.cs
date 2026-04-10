// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Models;

/// <summary>
/// Represents details about a Web App.
/// </summary>
public sealed record WebappDetails(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("enabled")] bool? Enabled,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("resourceGroup")] string ResourceGroup,
    [property: JsonPropertyName("hostNames")] IReadOnlyList<string> HostNames,
    [property: JsonPropertyName("lastModifiedTimeUtc")] DateTimeOffset? LastModifiedTimeUtc,
    [property: JsonPropertyName("sku")] string Sku);
