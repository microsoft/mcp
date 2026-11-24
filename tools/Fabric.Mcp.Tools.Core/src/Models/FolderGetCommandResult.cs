// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>One folder metadata snapshot, without inferred parents, paths, or contents.</summary>
public sealed record FolderGetCommandResult(
    FabricFolder Folder,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null);
