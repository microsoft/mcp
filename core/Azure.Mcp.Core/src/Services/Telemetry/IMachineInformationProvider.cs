// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Telemetry;

internal interface IMachineInformationProvider
{
    /// <summary>
    /// Gets existing or creates the device id.  In case the cached id cannot be retrieved, or the
    /// newly generated id cannot be cached, a value of null is returned.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task<string?> GetOrCreateDeviceId(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a hash of the machine's MAC address.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task<string> GetMacAddressHash(CancellationToken cancellationToken);
}
