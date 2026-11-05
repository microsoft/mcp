// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Models;

public sealed record ValidatedLocalFile(
    string CanonicalPath,
    string Name,
    string MimeType,
    long SizeBytes,
    DateTime LastWriteTimeUtc);
