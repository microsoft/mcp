// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>One complete page of job snapshots, with an unchanged token for an explicitly requested next page.</summary>
public sealed record ItemJobInstanceListCommandResult(
    IReadOnlyList<FabricItemJobInstance> JobInstances,
    string? ContinuationToken = null,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null);
