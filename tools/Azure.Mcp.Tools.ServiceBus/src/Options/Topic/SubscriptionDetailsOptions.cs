// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ServiceBus.Options.Topic;

public sealed class SubscriptionDetailsOptions : BaseTopicOptions
{
    /// <summary>
    /// Name of the subscription.
    /// </summary>
    [Option(Description = ServiceBusOptionDefinitions.SubscriptionNameDescription)]
    public required string SubscriptionName { get; set; }
}
