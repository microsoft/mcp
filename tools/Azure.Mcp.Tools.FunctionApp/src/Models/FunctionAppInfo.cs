// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FunctionApp.Models;

public record FunctionAppInfo(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("resourceGroupName")] string? ResourceGroupName,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("appServicePlanName")] string? AppServicePlanName,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("defaultHostName")] string? DefaultHostName,
    [property: JsonPropertyName("tags")] IDictionary<string, string>? Tags
);
