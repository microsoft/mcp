// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ServiceBus.Options.Topic;

public class SubscriptionDetailsOptions : BaseTopicOptions
{
    /// <summary>
    /// Name of the subscription.
    /// </summary>
    public string? SubscriptionName { get; set; }
}
