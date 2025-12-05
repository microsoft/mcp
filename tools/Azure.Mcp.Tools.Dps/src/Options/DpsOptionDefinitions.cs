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

    public const string HostnameName = "hostname";

    public static readonly Option<string> Hostname = new(
    $"--{HostnameName}")
    {
        Description = "The hostname of the Device Provisioning Service instance.",
        Required = true,
    };

    public const string EnrollmentGroupIdName = "enrollmentGroupId";

    public static readonly Option<string> EnrollmentGroupId = new(
        $"--{EnrollmentGroupIdName}"
    )
    {
        Description = "The ID of the enrollment group.",
        Required = false
    };
}
