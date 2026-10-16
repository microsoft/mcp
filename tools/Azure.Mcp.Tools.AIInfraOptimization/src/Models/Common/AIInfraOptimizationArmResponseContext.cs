// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Common;

public sealed record AIInfraOptimizationArmResponseContext(
    string? RequestId,
    string? ClientRequestId,
    DateTimeOffset? Date,
    string? RetryAfter);
