// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.IoTHub.Models;
using System.Text.Json;

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

    Task<DeviceIdentity> GetDevice(
        string deviceId,
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<DeviceTwin> GetDeviceTwin(
        string deviceId,
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<DeviceTwin> UpdateDeviceTwin(
        string deviceId,
        TwinPatch patch,
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<DeviceTwin>> QueryTwins(
        string query,
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IoTHubQueryPage> RunQuery(
        string query,
        string name,
        string resourceGroup,
        string subscription,
        int? maxCount = null,
        string? continuationToken = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IoTHubRegistryStatistics> GetDeviceStatistics(
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
