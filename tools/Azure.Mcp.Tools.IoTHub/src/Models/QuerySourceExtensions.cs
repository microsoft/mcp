// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Models;

internal static class QuerySourceExtensions
{
    internal static string ToValue(this QuerySource source) => source switch
    {
        QuerySource.Devices => "devices",
        QuerySource.DeviceModules => "devices.modules",
        QuerySource.DeviceJobs => "devices.jobs",
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
    };
}
