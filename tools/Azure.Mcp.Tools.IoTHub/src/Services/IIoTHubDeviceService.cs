// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Services;

public interface IIoTHubDeviceService
{
    Task<DeviceListResult> ListDevices(
        string hubName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        int? maxCount = null,
        CancellationToken cancellationToken = default);
}
