// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

public record IoTHubDescription(
    string Id,
    string Name,
    string Location,
    string ResourceGroup,
    string SubscriptionId,
    string Sku,
    long Capacity,
    string State,
    string HostName);
