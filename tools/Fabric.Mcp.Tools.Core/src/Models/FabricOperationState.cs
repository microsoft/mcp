// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record FabricOperationState(
    string Status,
    DateTimeOffset? CreatedTimeUtc = null,
    DateTimeOffset? LastUpdatedTimeUtc = null,
    int? PercentComplete = null,
    FabricOperationError? Error = null);
