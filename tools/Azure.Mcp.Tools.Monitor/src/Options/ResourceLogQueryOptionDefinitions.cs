// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Options;

public static class ResourceLogQueryOptionDefinitions
{
    public const string ResourceIdName = "resource-id";

    public static readonly Option<string> ResourceId = new(
        $"--{ResourceIdName}",
        "The Azure Resource ID to query logs. Example: /subscriptions/<sub>/resourceGroups/<rg>/providers/Microsoft.OperationalInsights/workspaces/<ws>"
    )
    {
        IsRequired = true
    };
}
