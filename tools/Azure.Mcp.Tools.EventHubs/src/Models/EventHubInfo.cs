// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Models;

public record EventHubInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("resourceGroup")] string ResourceGroup,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("partitionCount")] int? PartitionCount,
    [property: JsonPropertyName("messageRetentionInDays")] int? MessageRetentionInDays,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("createdOn")] DateTimeOffset? CreatedOn,
    [property: JsonPropertyName("updatedOn")] DateTimeOffset? UpdatedOn,
    [property: JsonPropertyName("partitionIds")] IReadOnlyList<string>? PartitionIds);
