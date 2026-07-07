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
[JsonSerializable(typeof(CatalogSearchRequest))]
[JsonSerializable(typeof(CatalogSearchResponse))]
[JsonSerializable(typeof(CatalogSearchCommandResult))]
public partial class CoreJsonContext : JsonSerializerContext
{
}

public sealed record ItemCreateCommandResult(FabricItem Item);

public sealed record CatalogSearchCommandResult(CatalogSearchResponse Results);
