// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ManagedLustre.Models;

public sealed record LustreFileSystem(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("resourceGroupName")] string? ResourceGroupName,
    [property: JsonPropertyName("subscriptionId")] string? SubscriptionId,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("mgsAddress")] string? MgsAddress,
    [property: JsonPropertyName("skuTier")] string? SkuTier,
    [property: JsonPropertyName("storageCapacityTiB")] long? StorageCapacityTiB,
    [property: JsonPropertyName("maintenanceDay")] string? MaintenanceDay,
    [property: JsonPropertyName("maintenanceTime")] string? MaintenanceTime,
    [property: JsonPropertyName("subnetId")] string? SubnetId,
    [property: JsonPropertyName("hsmDataContainer")] string? HsmDataContainer,
    [property: JsonPropertyName("hsmLogContainer")] string? HsmLogContainer,
    [property: JsonPropertyName("rootSquashMode")] string? RootSquashMode,
    [property: JsonPropertyName("noSquashNidList")] string? NoSquashNidList,
    [property: JsonPropertyName("squashUid")] long? SquashUid,
    [property: JsonPropertyName("squashGid")] long? SquashGid
);
