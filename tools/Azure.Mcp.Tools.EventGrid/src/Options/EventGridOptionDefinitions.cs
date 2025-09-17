// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.EventGrid.Options;

public static class EventGridOptionDefinitions
{
    public const string TopicNameParam = "topic-name";

    public static readonly Option<string> TopicName = new(
        $"--{TopicNameParam}"
    )
    {
        Description = "The name of the Event Grid topic."
    };
}
