// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResiliencyAgent.Options;

public class ResiliencyAgentRequestOptions
{
    [Option(Description = ResiliencyAgentOptionDescriptions.Request)]
    public required string Request { get; set; }

    [Option(Description = ResiliencyAgentOptionDescriptions.ConversationId)]
    public string? ConversationId { get; set; }
}

public sealed class ResiliencyAgentAttachmentRequestOptions : ResiliencyAgentRequestOptions
{
    [Option(Description = ResiliencyAgentOptionDescriptions.AttachmentIds)]
    public string[]? AttachmentIds { get; set; }
}
