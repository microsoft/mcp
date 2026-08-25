// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Areas.Tools.Commands;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Metadata;

namespace Microsoft.Mcp.Core.Models;

[JsonSerializable(typeof(List<CommandInfo>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, bool>))]
[JsonSerializable(typeof(CommandResponse))]
[JsonSerializable(typeof(ETag), TypeInfoPropertyName = "McpETag")]
[JsonSerializable(typeof(ToolMetadata))]
[JsonSerializable(typeof(OperationPlaneMetadataDefinition))]
[JsonSerializable(typeof(ToolsListCommand.ToolsListResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public sealed partial class ModelsJsonContext : JsonSerializerContext;
