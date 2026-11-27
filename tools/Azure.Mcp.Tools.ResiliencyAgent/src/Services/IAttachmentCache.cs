// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Models;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

public interface IAttachmentCache
{
    AgentAttachment Add(AgentAttachment attachment);

    IReadOnlyList<AgentAttachment> Resolve(IReadOnlyList<string>? attachmentIds);

    void Remove(IReadOnlyList<string> attachmentIds);
}
