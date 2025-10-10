// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Search.Commands.Index;
using Azure.Mcp.Tools.Search.Commands.Knowledge;
using Azure.Mcp.Tools.Search.Commands.Service;
using Azure.Mcp.Tools.Search.Models;

namespace Azure.Mcp.Tools.Search.Commands;

[JsonSerializable(typeof(FieldInfo))]
[JsonSerializable(typeof(IndexGetCommand.IndexGetCommandResult))]
[JsonSerializable(typeof(IndexInfo))]
[JsonSerializable(typeof(List<JsonElement>))]
[JsonSerializable(typeof(ServiceListCommand.ServiceListCommandResult))]
[JsonSerializable(typeof(KnowledgeSourceGetCommand.KnowledgeSourceGetCommandResult))]
[JsonSerializable(typeof(KnowledgeBaseGetCommand.KnowledgeBaseGetCommandResult))]
[JsonSerializable(typeof(KnowledgeBaseRetrieveCommand.KnowledgeBaseRetrieveCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class SearchJsonContext : JsonSerializerContext
{
    // This class is generated at runtime by the source generator.
}
