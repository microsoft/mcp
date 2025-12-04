// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Dps.Options;

/// <summary>
/// Static option definitions for DPS commands.
/// </summary>
public static class DpsOptionDefinitions
{
    public const string InstanceName = "instance";

    public static readonly Option<string> Instance = new(
        $"--{InstanceName}"
    )
    {
        Description = "The name of the Device Provisioning Service instance.",
        Required = false
    };
}
