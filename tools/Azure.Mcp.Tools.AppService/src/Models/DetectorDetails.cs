// Copyright (c) Microsoft Corporation.Expand commentComment on line R1Resolved
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AppService.Models;

/// <summary>
/// Represents details about a Web App detector.
/// </summary>
public sealed record DetectorDetails(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("category")] string? Category,
    [property: JsonPropertyName("analysisTypes")] List<string>? AnalysisTypes);
