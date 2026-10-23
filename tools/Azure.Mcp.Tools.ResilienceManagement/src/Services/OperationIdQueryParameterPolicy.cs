// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

/// <summary>
/// The Drills backend validates the per-operation id from the <c>operation-id</c> request header but reads it from the
/// <c>operationId</c> query parameter to drive the asynchronous work. The generated SDK only emits the header, so a
/// request that omits the query parameter is accepted (202) yet silently no-ops. This policy mirrors the header value
/// into the query string when it is absent so long-running drill operations actually progress.
/// </summary>
internal sealed class OperationIdQueryParameterPolicy : HttpPipelineSynchronousPolicy
{
    private const string OperationIdHeaderName = "operation-id";
    private const string OperationIdQueryName = "operationId";

    public override void OnSendingRequest(HttpMessage message)
    {
        if (message.Request.Headers.TryGetValue(OperationIdHeaderName, out var operationId)
            && !string.IsNullOrEmpty(operationId)
            && !message.Request.Uri.Query.Contains(OperationIdQueryName + "=", StringComparison.OrdinalIgnoreCase))
        {
            message.Request.Uri.AppendQuery(OperationIdQueryName, operationId);
        }
    }
}
