// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Text.Json;

namespace Azure.Mcp.Core.Tests.Services.Azure;

internal sealed class ResourceGraphPagingHttpMessageHandler(
    Func<JsonElement, HttpResponseMessage> responseFactory) : HttpMessageHandler
{
    public ConcurrentQueue<JsonElement> Requests { get; } = new();
    public ConcurrentQueue<string?> AuthorizationHeaders { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var document = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
        var body = document.RootElement.Clone();
        Requests.Enqueue(body);
        AuthorizationHeaders.Enqueue(request.Headers.Authorization?.ToString());
        return responseFactory(body);
    }
}
