// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Models;

/// <summary>
/// One immutable local file snapshot to send as an inline A2A <c>FilePart</c>.
/// </summary>
public sealed record AgentAttachment(
    string AttachmentId,
    string Name,
    string MimeType,
    byte[] Content,
    string Sha256,
    DateTimeOffset ExpiresAt);
