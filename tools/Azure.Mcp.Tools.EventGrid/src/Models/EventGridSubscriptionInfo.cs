// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventGrid.Models;

public record EventGridSubscriptionInfo(
    string Name,
    string Type,
    string? EndpointType,
    string? EndpointUrl,
    string? ProvisioningState,
    string? DeadLetterDestination,
    string? Filter,
    int? MaxDeliveryAttempts,
    int? EventTimeToLiveInMinutes,
    string? CreatedDateTime,
    string? UpdatedDateTime
);
