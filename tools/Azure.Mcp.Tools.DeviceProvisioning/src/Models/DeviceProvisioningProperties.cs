// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public class DeviceProvisioningProperties
{
    public string? State { get; set; }

    public string? ProvisioningState { get; set; }

    public string? ServiceOperationsHostName { get; set; }

    public string? DeviceProvisioningHostName { get; set; }

    public string? IdScope { get; set; }

    public string? AllocationPolicy { get; set; }

    public string? PublicNetworkAccess { get; set; }

    public bool? EnableDataResidency { get; set; }

    public List<LinkedIoTHubProperties>? IotHubs { get; set; }
}
