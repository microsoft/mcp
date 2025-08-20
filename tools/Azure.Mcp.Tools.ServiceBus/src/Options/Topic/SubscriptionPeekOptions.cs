// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ServiceBus.Options.Topic;

public class SubscriptionPeekOptions : BaseTopicOptions
{
    /// <summary>
    /// Name of the subscription.
    /// </summary>
    public string? SubscriptionName { get; set; }

    /// <summary>
    /// Maximum number of messages to peek from subscription.
    /// </summary>
    public int? MaxMessages { get; set; }
}
