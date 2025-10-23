// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using ToolMetadataExporter.Models.Kusto;

namespace ToolMetadataExporter.Models;

[JsonSerializable(typeof(McpToolEvent))]
[JsonSerializable(typeof(McpToolEventType))]
[JsonSerializable(typeof(List<McpToolEvent>))]
public partial class ModelsSerializationContext : JsonSerializerContext
{
}
