// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Options;

public static class EventHubsOptionDefinitions
{
    public const string NamespaceNameName = "namespace-name";

    public static readonly Option<string> NamespaceName = new(
        $"--{NamespaceNameName}"
    )
    {
        Description = "The name of the EventHubs namespace to retrieve. Must be used with --resource-group option.",
        Required = false
    };
}
