// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.EventGrid.Options;

public static class EventGridOptionDefinitions
{
    public const string TopicName = "topic";

    public static readonly Option<string> Topic = new(
        $"--{TopicName}",
        "The name of the Event Grid topic.")
    {
        IsRequired = false
    };
}
