// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Models;

/// <summary>
/// Represents details about a Web App diagnostic category.
/// </summary>
public sealed record WebappDiagnosticCategoryDetails(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("description")] string Description);
