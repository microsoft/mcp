// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResourceHealth;

public static class ResourceHealthOptionDefinitions
{
    public const string ResourceIdName = "resourceId";

    public static readonly Option<string> ResourceId = new(
        $"--{ResourceIdName}"
    )
    {
        Description = "The Azure resource ID to get health status for (e.g., /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Compute/virtualMachines/{vm}).",
        Required = true
    };
}
