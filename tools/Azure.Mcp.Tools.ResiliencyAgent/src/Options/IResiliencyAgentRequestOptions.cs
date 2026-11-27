// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Options;

public interface IResiliencyAgentRequestOptions
{
    string Request { get; }

    string? ConversationId { get; }
}
