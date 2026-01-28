// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace ToolMetadataExporter.Models.Kusto;

public enum McpToolEventType
{
    [JsonStringEnumMemberName("Created")]
    Created,
    [JsonStringEnumMemberName("Updated")]
    Updated,
    [JsonStringEnumMemberName("Deleted")]
    Deleted
}
