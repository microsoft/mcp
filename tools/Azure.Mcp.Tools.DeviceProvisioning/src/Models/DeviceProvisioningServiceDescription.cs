// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public record DeviceProvisioningServiceDescription(
    string Id,
    string Name,
    string Location,
    string ResourceGroup,
    string SubscriptionId,
    string Sku,
    long Capacity,
    string State,
    string ProvisioningState,
    string ServiceOperationsHostName,
    string DeviceProvisioningHostName,
    string IdScope,
    string AllocationPolicy,
    string PublicNetworkAccess,
    bool? EnableDataResidency,
    List<LinkedIoTHubDescription> IotHubs);

