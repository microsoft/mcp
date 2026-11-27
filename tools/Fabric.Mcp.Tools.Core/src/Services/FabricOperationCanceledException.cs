// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed class FabricOperationCanceledException(FabricOperationReceipt operation, CancellationToken cancellationToken)
    : OperationCanceledException("Waiting was canceled by the caller. The Fabric operation was not canceled.", cancellationToken)
{
    internal FabricOperationReceipt Operation { get; } = operation;
}
