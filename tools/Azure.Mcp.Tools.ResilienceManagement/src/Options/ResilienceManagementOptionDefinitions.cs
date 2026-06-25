// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Options;

public static class ResilienceManagementOptionDefinitions
{
    public const string ServiceGroupName = "service-group";

    public static readonly Option<string> ServiceGroup = new($"--{ServiceGroupName}")
    {
        Description = "The name of the service group.",
        Required = true
    };
}
