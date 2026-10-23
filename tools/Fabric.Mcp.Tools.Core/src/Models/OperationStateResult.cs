// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record OperationStateResult(
    Guid OperationId,
    FabricOperationState State,
    double? ServerRetryAfterSeconds,
    double RecommendedPollAfterSeconds);
