// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

/// <summary>
/// Fabric accepted a request, but its 202 response could not be read. Carries the operation receipt so the
/// caller can resume tracking instead of resubmitting the request.
/// </summary>
internal sealed class FabricOperationReceiptException(FabricOperationReceipt operation)
    : HttpRequestException(operation.Issue?.Message ?? "Fabric accepted the request, but its response could not be read.", null, HttpStatusCode.BadGateway)
{
    internal FabricOperationReceipt Operation { get; } = operation;
}
