// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>
/// The response to one write request. Fabric 202 responses add an operation receipt and, when requested
/// after a waited success, the operation result. The response is null only when Fabric accepted the request but
/// its response could not be read or the caller canceled; the operation receipt is still returned.
/// </summary>
public sealed record ApiRequestInvokeCommandResult(
    FabricApiResponse? Response,
    FabricOperationReceipt? Operation = null,
    OperationResult? OperationResult = null);
