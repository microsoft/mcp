// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.IoTHub.Models;
using Microsoft.Mcp.Core.Options;

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
