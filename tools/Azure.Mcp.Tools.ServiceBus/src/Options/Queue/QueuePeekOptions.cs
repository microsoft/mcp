// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ServiceBus.Options.Queue;

public sealed class QueuePeekOptions : BaseQueueOptions
{
    /// <summary>
    /// Maximum number of messages to peek from queue.
    /// </summary>
    [Option(Description = ServiceBusOptionDefinitions.MaxMessagesDescription)]
    public int? MaxMessages { get; set; }
}
