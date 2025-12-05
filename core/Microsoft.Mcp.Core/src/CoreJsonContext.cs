// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Metadata;

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
