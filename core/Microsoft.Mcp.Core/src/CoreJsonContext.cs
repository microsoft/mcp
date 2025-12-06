// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Metadata;

namespace Azure.Mcp.Core.Areas.Server;

[JsonSerializable(typeof(ExceptionResult))]
[JsonSerializable(typeof(ToolMetadata))]
[JsonSerializable(typeof(MetadataDefinition))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
internal sealed partial class CoreJsonContext : JsonSerializerContext
{
}
