// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ServiceBus.Options.Topic;

public class BaseTopicOptions : GlobalOptions
{
    /// <summary>
    /// Service Bus namespace.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Name of the topic.
    /// </summary>
    public string? TopicName { get; set; }
}
