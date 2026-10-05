// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Models;

public sealed record FileAttachmentResult(
    string AttachmentId,
    string FileName,
    string MimeType,
    long SizeBytes,
    string Sha256,
    DateTimeOffset ExpiresAt);
