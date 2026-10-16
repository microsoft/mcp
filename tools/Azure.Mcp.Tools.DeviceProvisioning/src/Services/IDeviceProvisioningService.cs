// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.DeviceProvisioning.Models;

namespace Azure.Mcp.Tools.DeviceProvisioning.Services;

public interface IDeviceProvisioningService
{
    Task<DeviceProvisioningServiceDescription> GetService(
        string serviceName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}

