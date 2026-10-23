// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Services;

/// <summary>
/// Resolves and briefly caches the hostname of an IoT Hub so that repeated data-plane calls
/// (device list/show, twin get, query run, ...) don't each pay for a control-plane hub lookup.
/// </summary>
public interface IIoTHubHostnameResolver
{
    Task<string> GetHostnameAsync(
        string hubName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
