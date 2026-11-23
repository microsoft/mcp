// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>One complete folder page with an unchanged token for an explicitly requested next page.</summary>
public sealed record FolderListCommandResult(
    IReadOnlyList<FabricFolder> Folders,
    string? ContinuationToken = null,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null);
