// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ResiliencyAgent.Options;

public sealed class ResiliencyAgentAttachmentRequestOptions
    : IResiliencyAgentRequestOptions, IAttachmentIdsOption
{
    [Option(Description = ResiliencyAgentOptionDescriptions.Request)]
    public required string Request { get; set; }

    [Option(Description = ResiliencyAgentOptionDescriptions.ConversationId)]
    public string? ConversationId { get; set; }

    [Option(Description = ResiliencyAgentOptionDescriptions.AttachmentIds)]
    public string[]? AttachmentIds { get; set; }
}
