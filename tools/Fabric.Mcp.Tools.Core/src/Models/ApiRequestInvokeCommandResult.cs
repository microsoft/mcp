// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>
/// The response to one write request. Fabric 202 responses add an operation receipt and, when requested
/// after a waited success, the operation result. The response is null only when the caller canceled the wait.
/// </summary>
public sealed record ApiRequestInvokeCommandResult(
    FabricApiResponse? Response,
    FabricOperationReceipt? Operation = null,
    OperationResult? OperationResult = null);
