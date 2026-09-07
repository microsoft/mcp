// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

internal static class VmPowerActionExtensions
{
    internal static string ToValue(this VmPowerAction powerAction) => powerAction switch
    {
        VmPowerAction.Start => "start",
        VmPowerAction.Stop => "stop",
        VmPowerAction.Deallocate => "deallocate",
        VmPowerAction.Restart => "restart",
        _ => throw new ArgumentOutOfRangeException(nameof(powerAction), powerAction, null)
    };
}
