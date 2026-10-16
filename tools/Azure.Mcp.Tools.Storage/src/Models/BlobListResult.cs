// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Storage.Models;

public sealed record BlobListResult(
    List<BlobInfo> Blobs,
    List<string> Prefixes,
    string? ContinuationToken);
