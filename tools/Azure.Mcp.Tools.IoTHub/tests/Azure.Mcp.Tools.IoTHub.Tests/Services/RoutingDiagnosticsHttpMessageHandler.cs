// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;

namespace Azure.Mcp.Tools.IoTHub.Tests.Services;

internal sealed class RoutingDiagnosticsHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> respond) : HttpMessageHandler
{
    public ConcurrentQueue<Uri> RequestUris { get; } = new();

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestUris.Enqueue(request.RequestUri!);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(respond(request, cancellationToken));
    }
}
