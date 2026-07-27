// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ServiceBus.Options.Topic;

public class BaseTopicOptions
{
    /// <summary>
    /// Service Bus namespace.
    /// </summary>
    [Option(Description = ServiceBusOptionDefinitions.NamespaceDescription)]
    public required string Namespace { get; set; }

    /// <summary>
    /// Name of the topic.
    /// </summary>
    [Option(Description = "The name of the topic containing the subscription.")]
    public required string Topic { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
