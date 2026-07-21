// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Telemetry;

/// <summary>
/// Default information provider not tied to any platform specification for DevDeviceId.
/// </summary>
internal class DefaultMachineInformationProvider(ILogger<MachineInformationProviderBase> logger)
    : MachineInformationProviderBase(logger)
{
    /// <summary>
    /// Returns null.
    /// </summary>
    /// <returns></returns>
    public override Task<string?> GetOrCreateDeviceId() => Task.FromResult<string?>(null);
}
