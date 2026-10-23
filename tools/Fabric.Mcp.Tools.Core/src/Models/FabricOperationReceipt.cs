// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record FabricOperationReceipt(
    FabricOperationStatus Status,
    Guid? OperationId = null,
    FabricOperationState? LastState = null,
    double? ServerRetryAfterSeconds = null,
    double? RecommendedPollAfterSeconds = null,
    FabricOperationIssue? Issue = null);
