// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Core.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectRequiredConstructorParameters = true,
    AllowDuplicateProperties = false,
    MaxDepth = 8)]
[JsonSerializable(typeof(FabricItemDefinition))]
[JsonSerializable(typeof(FabricItemDefinitionFile[]))]
internal partial class FabricItemDefinitionInputJsonContext : JsonSerializerContext;
