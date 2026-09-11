// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResiliencyAgent.Options.Conversations;

public sealed class ConversationAskOptions
{
    [Option(Description = ResiliencyAgentOptionDescriptions.Request)]
    public required string Request { get; set; }

    [Option(Description = ResiliencyAgentOptionDescriptions.ConversationId)]
    public string? ConversationId { get; set; }
}
