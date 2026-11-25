// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Storage.Tests.Services;

internal sealed class StorageTestHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> send) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(send(request, cancellationToken));
    }
}
