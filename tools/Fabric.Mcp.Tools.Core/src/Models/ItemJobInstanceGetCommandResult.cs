// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>One job-instance snapshot and any valid server retry guidance; no polling is performed.</summary>
public sealed record ItemJobInstanceGetCommandResult(
    FabricItemJobInstance JobInstance,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null);
