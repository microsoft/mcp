// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Options;

public static class EventHubsOptionDefinitions
{
    public const string NamespaceIdName = "namespace-id";
    public const string NamespaceNameName = "namespace-name";

    public static readonly Option<string> NamespaceId = new(
        $"--{NamespaceIdName}"
    )
    {
        Description = "The full resource ID of the EventHubs namespace (e.g., '/subscriptions/sub-id/resourceGroups/rg-name/providers/Microsoft.EventHub/namespaces/namespace-name').",
        Required = false
    };

    public static readonly Option<string> NamespaceName = new(
        $"--{NamespaceNameName}"
    )
    {
        Description = "The name of the EventHubs namespace to retrieve. Must be used with --resource-group option.",
        Required = false
    };
}
