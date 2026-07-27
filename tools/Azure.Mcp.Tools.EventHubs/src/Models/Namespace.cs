// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Models;

public sealed record Namespace(
    string Name,
    string Id,
    string ResourceGroup,
    string Location,
    EventHubsSku Sku,
    string? Status = null,
    string? ProvisioningState = null,
    DateTimeOffset? CreationTime = null,
    DateTimeOffset? UpdatedTime = null,
    string? ServiceBusEndpoint = null,
    string? MetricId = null,
    bool? IsAutoInflateEnabled = null,
    int? MaximumThroughputUnits = null,
    bool? KafkaEnabled = null,
    bool? ZoneRedundant = null,
    Dictionary<string, string>? Tags = null);
