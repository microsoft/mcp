// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Models;

public sealed record EventHub(
    string Name,
    string Id,
    string ResourceGroup,
    string? Location,
    int? PartitionCount,
    int? MessageRetentionInDays,
    string? Status,
    DateTimeOffset? CreatedOn,
    DateTimeOffset? UpdatedOn,
    IReadOnlyList<string>? PartitionIds);
