// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.InfraIq.Models.Common;
using Azure.Mcp.Tools.InfraIq.Models.Response;

namespace Azure.Mcp.Tools.InfraIq.Models.VmSku;

public sealed record VmSkuRecommendResult(
    InfraIqRecommendVmSkuResponse Response,
    InfraIqArmResponseContext Context);
