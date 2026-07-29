// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ServiceBus.Options.Queue;

public class BaseQueueOptions
{
    /// <summary>
    /// Service Bus namespace.
    /// </summary>
    [Option(Description = ServiceBusOptionDefinitions.NamespaceDescription)]
    public required string Namespace { get; set; }

    /// <summary>
    /// Name of the queue.
    /// </summary>
    [Option(Description = "The queue name.")]
    public required string Queue { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
