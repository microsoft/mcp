// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.VmSku;

public sealed record VmSkuRecommendErrorResult(
    int Status,
    string? Code,
    string? Target,
    string? RequestId,
    string? ClientRequestId,
    string? RetryAfter,
    string Message);
