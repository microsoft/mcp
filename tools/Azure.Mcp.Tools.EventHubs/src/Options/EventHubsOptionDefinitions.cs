// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Options;

public static class EventHubsOptionDefinitions
{
    public const string Namespace = "namespace";

    public static readonly Option<string> NamespaceName = new(
        $"--{Namespace}"
    )
    {
        Description = "The name of the Event Hubs namespace to retrieve. Must be used with --resource-group option.",
        Required = false
    };
}
