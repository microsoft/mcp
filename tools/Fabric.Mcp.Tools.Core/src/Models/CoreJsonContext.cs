// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Core.Models;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(CapacityGetCommandResult))]
[JsonSerializable(typeof(CapacityListCommandResult))]
[JsonSerializable(typeof(CapacityListResponse))]
[JsonSerializable(typeof(FabricItem))]
[JsonSerializable(typeof(CreateItemRequest))]
[JsonSerializable(typeof(ItemCreateCommandResult))]
[JsonSerializable(typeof(FabricItemSensitivityLabel))]
[JsonSerializable(typeof(FabricItemSummary))]
[JsonSerializable(typeof(FabricItemTag))]
[JsonSerializable(typeof(ItemListCommandResult))]
[JsonSerializable(typeof(ItemListResponse))]
[JsonSerializable(typeof(CatalogSearchRequest))]
[JsonSerializable(typeof(CatalogSearchResponse))]
[JsonSerializable(typeof(CatalogSearchCommandResult))]
[JsonSerializable(typeof(FabricCapacityMetadata))]
[JsonSerializable(typeof(FabricWorkspaceAppliedTag))]
[JsonSerializable(typeof(FabricWorkspaceIdentity))]
[JsonSerializable(typeof(FabricWorkspaceMetadata))]
[JsonSerializable(typeof(FabricWorkspaceOneLakeEndpoints))]
[JsonSerializable(typeof(WorkspaceGetCommandResult))]
[JsonSerializable(typeof(FabricWorkspaceListEntry))]
[JsonSerializable(typeof(FabricWorkspaceListTag))]
[JsonSerializable(typeof(WorkspaceListCommandResult))]
[JsonSerializable(typeof(WorkspaceListResponse))]
public partial class CoreJsonContext : JsonSerializerContext
{
}

public sealed record ItemCreateCommandResult(FabricItem Item);

public sealed record CatalogSearchCommandResult(CatalogSearchResponse Results);
