// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AIInfraOptimization.Models.Common;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Response;

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.VmSku;

public sealed record VmSkuRecommendResult(
    AIInfraOptimizationRecommendVmSkuResponse Response,
    AIInfraOptimizationArmResponseContext Context);
