// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Telemetry;

internal interface IMachineInformationProvider
{
    /// <summary>
    /// Gets existing or creates the device id.  In case the cached id cannot be retrieved, or the
    /// newly generated id cannot be cached, a value of null is returned.
    /// </summary>
    Task<string?> GetOrCreateDeviceId();

    /// <summary>
    /// Gets a hash of the machine's MAC address.
    /// </summary>
    Task<string> GetMacAddressHash();
}
