// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Services;

public interface IIoTHubDeviceService
{
    Task<DeviceListResult> ListDevices(
        string name,
        string resourceGroup,
        string subscription,
        int? maxCount = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
