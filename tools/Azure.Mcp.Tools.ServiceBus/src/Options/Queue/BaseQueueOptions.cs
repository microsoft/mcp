// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ServiceBus.Options.Queue
{
    public class BaseQueueOptions : SubscriptionOptions
    {
        /// <summary>
        /// Service Bus namespace.
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// Name of the queue.
        /// </summary>
        public string? Name { get; set; }
    }
}
