// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Core.Models;

[JsonSerializable(typeof(FabricItem))]
[JsonSerializable(typeof(CreateItemRequest))]
[JsonSerializable(typeof(ItemCreateCommandResult))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
public partial class CoreJsonContext : JsonSerializerContext;

public sealed record ItemCreateCommandResult(FabricItem Item);
