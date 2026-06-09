// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Workbooks.Models;

public sealed record WorkbookInfo(
    [property: JsonPropertyName("WorkbookId")] string WorkbookId,
    [property: JsonPropertyName("DisplayName")] string? DisplayName,
    [property: JsonPropertyName("Description")] string? Description,
    [property: JsonPropertyName("Category")] string? Category,
    [property: JsonPropertyName("Location")] string? Location,
    [property: JsonPropertyName("Kind")] string? Kind,
    [property: JsonPropertyName("Tags")] string? Tags,
    [property: JsonPropertyName("SerializedData")] string? SerializedData,
    [property: JsonPropertyName("Version")] string? Version,
    [property: JsonPropertyName("TimeModified")] DateTimeOffset? TimeModified,
    [property: JsonPropertyName("UserId")] string? UserId,
    [property: JsonPropertyName("SourceId")] string? SourceId);
