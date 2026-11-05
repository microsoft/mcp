// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Options;

public interface IAttachmentIdsOption
{
    string[]? AttachmentIds { get; }
}
