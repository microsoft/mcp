// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Core.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(FabricItem))]
[JsonSerializable(typeof(CreateItemRequest))]
[JsonSerializable(typeof(ItemCreateCommandResult))]
public partial class CoreJsonContext : JsonSerializerContext
{
}

public sealed record ItemCreateCommandResult(FabricItem Item);
